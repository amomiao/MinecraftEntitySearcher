using Microsoft.Win32;
using MoNbt;

namespace MoNbtSearcher.Wpf {
    public class WindowViewModel : ViewModelBase {
        static string Tip = "选择存档文件下的level.dat";

        private List<string> dimPaths;
        private List<EntityPoiData> resultList = new List<EntityPoiData>();
        private string readProgress;  // 用于绑定进度
        private string levelPath = Tip;
        private int nowDataPageNum = 1;
        private int nowDataPageMaxNum = 1;

        NbtSearcher ns;

        public bool IsInitial { get; private set; } = false;
        public bool IsMcaInitial { get; private set; } = false;
        public List<string> LoadedPathList { get; private set; } = new List<string>();
        public List<string> DimPaths => dimPaths;
        public List<EntityPoiData> ResultList => resultList;
        public string[] DimNames {
            get {
                string[] names = new string[dimPaths.Count];
                string str;
                for (int i = 0; i < dimPaths.Count; i++) {
                    MoNbtSearcherHelper.TryDimI18N(dimPaths[i],out str);
                    names[i] = str;
                }
                return names;
            }
        }
        public string[] LoadedDimNames {
            get {
                string[] names = new string[LoadedPathList.Count];
                string str;
                for (int i = 0; i < LoadedPathList.Count; i++) {
                    MoNbtSearcherHelper.TryDimI18N(LoadedPathList[i], out str);
                    names[i] = str;
                }
                return names;
            }
        }

        public string LevelPath {
            get => levelPath;
            set => SetProperty(ref levelPath, string.IsNullOrEmpty(value) ? Tip : value);
        }
        public string ReadProgress {
            get => readProgress;
            set => SetProperty(ref readProgress, value);  // 通知属性变化
        }
        public int NowDataPageNum { 
            get => nowDataPageNum;
            set => SetProperty(ref nowDataPageNum, value);
        }
        public int NowDataPageMaxNum {
            get => nowDataPageMaxNum;
            set => SetProperty(ref nowDataPageMaxNum, value);
        }

        // 构造函数
        public WindowViewModel() {
            MoNbtSearcherHelper.LoadLocalData();
        }

        public bool TryChoosingLevel() {
            // 创建文件选择对话框
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Minecraft Level Files|level.dat";  // 只显示 level.dat 文件
            openFileDialog.Title = "选择 Minecraft 的 level.dat 文件";

            // 如果用户选择了文件
            if (openFileDialog.ShowDialog() == true) {
                LevelPath = openFileDialog.FileName;
            }
            else { 
                LevelPath = string.Empty;
            }

            ns = new NbtSearcher();
            if (!ns.TryInit(openFileDialog.FileName, out dimPaths)) {
                Logger.LogUserWarning("无效路径!");
                return false;
            }
            MoNbtSearcherHelper.TryGetPlayerData(ns.Root);
            MoNbtSearcherHelper.SaveIncreDimName(dimPaths);
            IsInitial = true;

            IsMcaInitial = false;
            LoadedPathList.Clear();

            return true;
        }

        public async Task LoadDimFile(List<string> localEntityPathList) {
            foreach (var loaded in LoadedPathList) {
                if (localEntityPathList.Contains(loaded)) { 
                    localEntityPathList.Remove(loaded);
                }
            }
            foreach (var item in localEntityPathList) {
                LoadedPathList.Add(item);
            }
            if (localEntityPathList.Count == 0) {
                Logger.LogUserWarning("维度已加载");
                return;
            }
            //Logger.LogUserWarning("开始读取");
            ReadProgress = "准备中...";
            Task task = Task.Run(() => {
                while (!ns.MCAReader.IsCompleted) {
                    // 更新进度
                    ReadProgress = ns.MCAReader.GetProgress();
                }
                //Logger.LogUserWarning("读取完成");
                ReadProgress = $"加载完成,读取了{ns.MCAReader.LoadTotalNum}个.mca文件";
            });

            ns.StartReadMCA(MoNbtSearcherHelper.ConfigData.ThreadNum, localEntityPathList.ToArray());
            IsMcaInitial = true;

            await task;
        }

        // 搜索功能
        public async Task SearchEnitiyNbt(TagKV[] filterDatas, params string[] localEntityPaths) {
            ResultList.Clear();

            ReadProgress = "搜索已启动,准备中...";

            var task = Task.Run(() => {
                while (!ns.EntityTagReader.IsCompleted) {
                    // 等待数据读取完成
                    ReadProgress = ns.EntityTagReader.GetProgress();
                }
                foreach (var kv in ns.EntityTagReader.DimEntityTagDic) {
                    foreach (var item in kv.Value) {
                        ResultList.Add(new EntityPoiData(kv.Key, item));
                    }
                }
                string str = $"搜索完成,得到数据{ResultList.Count}条.";
                Logger.LogUserWarning(str);
                ReadProgress = str;

                ns.EntityTagReader.Clear();
            });

            ns.StartReadEntityTag(filterDatas, MoNbtSearcherHelper.ConfigData.ThreadNum, localEntityPaths);

            // 等待搜索完成
            await task;
        }

        /// <summary> 尝试获取分页数据 </summary>
        public bool TryGetPageDatas((int userNum, int index) page, int pageSize, ref List<EntityPoiData> list) {

            NowDataPageMaxNum = resultList.Count == 0 ? 1 : (int)MathF.Ceiling(resultList.Count / (float)pageSize);
            if (page.index < 0) { 
                return false;
            }
            list.Clear();
            for (int i = page.index * pageSize; i < Math.Min(resultList.Count, page.index * pageSize + pageSize); i++) {
                list.Add(resultList[i]);
            }
            if (list.Count > 0) {
                // 把值加回来
                NowDataPageNum = page.userNum;
                return true;
            }
            return false;
        }
    }
}
