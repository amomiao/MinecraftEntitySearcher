using fNbt;
using LitJson;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Tommy.Helper;

namespace MoNbtSearcher {
    public static class MoNbtSearcherHelper {
        /// <summary> 修改窗体开发平台时只需要修改这里 </summary>
        public class CrossPlatform {
            /// <summary> 获得程序运行的根目录 </summary>
            public static string EXE_ROOT_PATH = AppDomain.CurrentDomain.BaseDirectory;
            /// <summary> 普通的日志 </summary>
            /// <param name="weight"> 根据权重可以进行一些行为 </param>
            public static void Log(string message, int weight = 0) {
                if (weight == 0) {
                    // 普通日志输出到调试窗口
                    Debug.WriteLine(message);
                }
                else if (weight == 2) {
                    // 错误日志输出到调试窗口
                    Debug.WriteLine(message);
                }
            }
            /// <summary> 会显式提示用户的日志 </summary>
            public static void LogUserWarning(string message) {
                // 显示警告框
                MessageBox.Show(message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static UserConfigData ConfigData;
        public static Dictionary<string, string> DimI18NDic;
        // 兴趣点是简单单词, 添加中文反而会歧异, 不翻译了
        // public static Dictionary<string, string> PoiI18NDic;

        public static void LoadConfigData() => ConfigData = JsonMapper.ToObject<UserConfigData>(File.ReadAllText(Flag.ConfigFullPath));
        public static void SaveConfigData() => File.WriteAllText(Flag.ConfigFullPath, JsonMapper.ToJson(ConfigData));

        /// <summary> 修改线程数 </summary>
        /// <returns> 返回被牵制过后的线程数 </returns>
        public static int ChangeThreadNum(int num) { 
            ConfigData.threadNum = num;
            SaveConfigData();
            return ConfigData.threadNum;
        }

        /// <summary> 启动后加载本地数据 </summary>
        public static void LoadLocalData() {
            DimI18NDic = new Dictionary<string, string>();
            // PoiI18NDic = new Dictionary<string, string>();
            SetDefaultData();
            ReadFileData();
        }
        #region LoadLocalData
        static void SetDefaultData() {
            void AddKV(Dictionary<string, string> dic, KeyValuePair<string, string> kv) {
                dic.Add(kv.Key, kv.Value);
            }

            // 原版维度翻译
            AddKV(DimI18NDic, Flag.Dim.Overworld);
            AddKV(DimI18NDic, Flag.Dim.Nether);
            AddKV(DimI18NDic, Flag.Dim.End);

            if (!Directory.Exists(Flag.CommonPath)) {
                Directory.CreateDirectory(Flag.CommonPath);
            }
            // 配置数据
            ConfigData = new UserConfigData();
            if (!File.Exists(Flag.ConfigFullPath)) {
                SaveConfigData();
            }
            // 维度翻译
            if (!File.Exists(Flag.DimI18NFullPath)) {
                TommyHelper.Table2File(TommyHelper.INI.ToTomlTable(DimI18NDic), Flag.DimI18NFullPath);
            }
        }
        static void ReadFileData() {
            LoadConfigData();
            foreach (var kv in TommyHelper.INI.FromToml(TommyHelper.File2Table(Flag.DimI18NFullPath))) {
                if (!DimI18NDic.ContainsKey(kv.Key)) {
                    DimI18NDic.Add(kv.Key, string.Empty);
                }
                DimI18NDic[kv.Key] = kv.Value;
            }
        }
        #endregion LoadLocalData

        /// <summary> 尝试本地化维度名称 </summary>
        public static bool TryDimI18N(string key, out string name) {
            if (!DimI18NDic.ContainsKey(key)) {
                name = key;
                return false;
            }
            name = DimI18NDic[key];
            return true;
        }

        /// <summary> 尝试本地化玩家名称 </summary>
        public static bool TryIdI18N(NbtIntArray nbtUUID, ref string name) {
            if (ConfigData.TryGetPlayerName(nbtUUID, out var pd)) {
                name = pd.player_name;
            }
            return false;
        }

        /// <summary> 尝试获取新的用户数据并保存 </summary>
        public static bool TryGetPlayerData(string saveRoot) {
            // 尝试从Ftbteams.player里获取数据
            FtbTeamData String2Obj(string str) {
                // 定义正则表达式来匹配 id 和 player_name
                string idPattern = @"id:\s*""([^""]+)""";
                string playerNamePattern = @"player_name:\s*""([^""]+)""";

                // 使用正则表达式提取 id 和 player_name
                string id = Regex.Match(str, idPattern).Groups[1].Value;
                string playerName = Regex.Match(str, playerNamePattern).Groups[1].Value;

                return new FtbTeamData() {
                    player_name = playerName,
                    id = id,
                };
            }
            string path = Path.Combine(saveRoot, Flag.FtbteamsPlayDirName);
            if (!Directory.Exists(path)) {
                return false;
            }
            string nbtStr;
            // 读取ftbteams的.snbt数据, 得到player_name及其MojangUUID
            string[] fps = Directory.GetFiles(path).Where((p) => Path.GetExtension(p) == Flag.SNBT).ToArray();
            FtbTeamData data;
            foreach (var f in fps) {
                nbtStr = File.ReadAllText(f);
                data = String2Obj(nbtStr);
                ConfigData.AddPlayerUUIDMO(data);
            }
            // 从playerdata里得数据, 得到MojangUUID和EntityUUID
            path = Path.Combine(saveRoot, Flag.PlayerDataDirName);
            fps = Directory.GetFiles(path).Where((p) => Path.GetExtension(p) == Flag.DAT).ToArray();
            string uuidMo;
            NbtFile nbt = new NbtFile();
            foreach (var f in fps) {
                uuidMo = Path.GetFileName(Path.GetFileNameWithoutExtension(f));
                nbt.LoadFromFile(f);
                if (nbt.RootTag is NbtCompound nc &&
                    nc.TryGet("UUID", out var tag) &&
                    tag is NbtIntArray nia) {
                    ConfigData.AddPlayerUUIDEN(uuidMo, nia);
                }
            }
            // 获取成功, 保存数据
            SaveConfigData();
            return true;
        }

        /// <summary> 保存新增的维度名,给本地化提供方便 </summary>
        public static void SaveIncreDimName(List<string> localEntityDirList) {
            foreach (var item in localEntityDirList) {
                if (!DimI18NDic.ContainsKey(item)) {
                    DimI18NDic.Add(item, item);
                }
            }
            TommyHelper.Table2File(TommyHelper.INI.ToTomlTable(DimI18NDic), Flag.DimI18NFullPath);
        }
    }
}