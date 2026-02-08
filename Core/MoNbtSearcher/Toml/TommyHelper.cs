using System.Collections.Generic;
using System.IO;
using Tommy.Serializer;

namespace Tommy.Helper {
    public static class TommyHelper {
        /// Toml提供的IO功能
        /// <summary> 将<see cref="TomlTable"/>保存到本地 </summary>
        /// <param name="path"> 绝对路径, 文件名以'.toml'结尾 </param>
        public static void Table2File(this TomlTable table, string path) {
            using (StreamWriter writer = File.CreateText(path)) {
                table.WriteTo(writer);
                // 刷新流
                writer.Flush();
            }
        }

        /// <summary> 读入文本并生成<see cref="TomlTable"/>类 </summary>
        /// <param name="path"> 绝对路径, 文件名以'.toml'结尾 </param>
        public static TomlTable File2Table(string path) {
            using StreamReader reader = File.OpenText(path);
            using TOMLParser parser = new TOMLParser(reader);
            return parser.Parse();
        }
        /// <summary> 使用字符串生成<see cref="TomlTable"/>类 </summary>
        public static TomlTable String2Table(string toml) {
            using StreamReader reader = new StreamReader(toml);
            using TOMLParser parser = new TOMLParser(reader);
            return parser.Parse();
        }

        #region Serializer
        public static class Serializer {
            /// <summary> <see cref="Tommy.Serializer"/>原生的序列化.toml文件的方法,直接将文本写入文件 </summary>
            public static void ToTomlFile(object obj, string path)
                => TommySerializer.ToTomlFile(obj, path);
            /// <summary> <see cref="Tommy.Serializer"/>拓展的序列化.toml文件的方法,获得文本的字符串 </summary>
            public static string ToTomlText(object obj)
                => TommySerializer.ToTomlText(obj);

            /// <summary> <see cref="Tommy.Serializer"/>原生的反序列化.toml文件的方法,直接读入文件进行处理 </summary>
            public static T FromTomlFile<T>(string path) where T : class, new()
                => TommySerializer.FromTomlFile<T>(path);
            /// <summary> <see cref="Tommy.Serializer"/>拓展的反序列化.toml文件的方法,输入字符串进行处理 </summary>
            public static T FromTomlText<T>(string text) where T : class, new()
                => TommySerializer.FromTomlText<T>(text);
        }
        #endregion Serializer

        #region INI
        public static class INI {
            /// <summary> 使用Toml写类似ini的结构 </summary>
            public static TomlTable ToTomlTable(params TommyInIKV[] kvs) {
                TomlTable table = new TomlTable();
                foreach (var kv in kvs) {
                    kv.Add2Table(table);
                }
                return table;
            }
            /// <summary> 使用Toml写类似ini的结构 </summary>
            public static TomlTable ToTomlTable(Dictionary<string,string> dic) {
                TomlTable table = new TomlTable();
                foreach (var kv in dic) {
                    table.Add(kv.Key, new TomlString { Value = kv.Value });
                }
                return table;
            }
            /// <summary> 从一个table里读取ini,仿.ini所以遍历使用<see cref="TomlTable.RawTable"/>即可 </summary>
            public static Dictionary<string, string> FromToml(TomlTable table) {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (var kv in table.RawTable) {
                    if (!dic.ContainsKey(kv.Key)) {
                        dic.Add(kv.Key, string.Empty);
                    }
                    dic[kv.Key] = kv.Value;
                }
                return dic;
            }
            /// <summary> 从一个table里读取ini,仿.ini所以遍历使用<see cref="TomlTable.RawTable"/>即可 </summary>
            public static Dictionary<string, string> FromTomlText(string toml) {
                return FromToml(String2Table(toml));
            }
        }
        #endregion INI
    }
}