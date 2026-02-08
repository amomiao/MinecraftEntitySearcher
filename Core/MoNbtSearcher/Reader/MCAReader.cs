using fNbt;
using MoNbt;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace MoNbtSearcher {
    public class MCAReader : IThreadWorker {
        // 每次用完就抛弃,尺寸可以浮动
        ConcurrentQueue<string>[] mcaFileQueArr;
        // 持久记录的数据,得用字典
        ConcurrentDictionary<string, ConcurrentQueue<NbtFile>> nbtQueDic = new ConcurrentDictionary<string, ConcurrentQueue<NbtFile>>();

        public int ThreadNum { get; private set; }
        public bool IsCompleted { get; private set; }
        public int LoadTotalNum { get; private set; }
        public int LoadedNum { get; private set; }

        /// <summary> 得到一个维度的数据 </summary>
        /// <param name="localEntityDirPath"> 此处仅作为key使用 </param>
        public ConcurrentQueue<NbtFile> GetDimNbtQue(string localEntityDirPath) {
            // 数据只读,所以浅复制引用即可
            ConcurrentQueue<NbtFile> nq = new ConcurrentQueue<NbtFile>();
            if (!nbtQueDic.ContainsKey(localEntityDirPath)) {
                Logger.Log($"不可以用的Key:{localEntityDirPath}");
                return nq;
            }
            foreach (var item in nbtQueDic[localEntityDirPath]) {
                nq.Enqueue(item);
            }
            return nq;
        }

        /// <param name="root"> 根目录路径 </param>
        /// <param name="threadNum"> 线程数 </param>
        /// <param name="localEntityDirPaths"> 实体目录自根目录的相对路径 </param>
        public void Start(string root, int threadNum = 1, params string[] localEntityDirPaths) {
            IsCompleted = false;
            LoadTotalNum = 0;
            LoadedNum = 0;
            ThreadNum = threadNum;
            // 获取加载目录
            mcaFileQueArr = new ConcurrentQueue<string>[localEntityDirPaths.Length];
            string path;
            for (int i = 0; i < mcaFileQueArr.Length; i++) {
                mcaFileQueArr[i] = new ConcurrentQueue<string>();
                path = Path.Combine(root, localEntityDirPaths[i]);
                foreach (var file in Directory.GetFiles(path)) {
                    if (Path.GetExtension(file) != Flag.MCA) {
                        continue;
                    }
                    mcaFileQueArr[i].Enqueue(file);
                }
            }
            LoadTotalNum = mcaFileQueArr.Sum((q) => q.Count);
            // 这里没有多线程, 不会出错
            foreach (var localEntityDirPath in localEntityDirPaths) {
                if (!nbtQueDic.ContainsKey(localEntityDirPath)) {
                    nbtQueDic.TryAdd(localEntityDirPath,new ConcurrentQueue<NbtFile>());
                }
                // 新加载, 清除旧数据
                nbtQueDic[localEntityDirPath].Clear();
            }
            // 线程加载
            this.ThreadAsyncDo(
                threadNum,
                async () => {
                    for (int i = 0; i < mcaFileQueArr.Length; i++) {
                        while (mcaFileQueArr[i].Count > 0) {
                            if (!mcaFileQueArr[i].TryDequeue(out var fp)) {
                                // 没有成功取出, 跳出
                                break;
                            }
                            Logger.Log($"开始加载:{fp}");
                            var items = await new MCAParser().ParseAsync(fp);
                            foreach (var item in items) {
                                nbtQueDic[localEntityDirPaths[i]].Enqueue(item.NBT);
                            }
                            Logger.Log($"完成加载:{fp}");
                            LoadedNum++;
                        }
                    }
                },
                (e) => Logger.Log(e.Message, 2),
                () => IsCompleted = true
            );
        }
    }
}