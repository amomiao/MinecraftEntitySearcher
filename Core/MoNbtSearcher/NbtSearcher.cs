using MoNbt;
using System.Collections.Generic;
using System.IO;

namespace MoNbtSearcher {
    public class NbtSearcher {
        public string Root { get; private set; }
        public List<string> LocalEntityDirList { get; private set; } = new List<string>();
        public MCAReader MCAReader { get; private set; } = new MCAReader();
        public EntityTagReader EntityTagReader { get; private set; } = new EntityTagReader();

        public bool IsLevelDatPath(string levelDatPath) {
            return Path.GetFileName(levelDatPath) == Flag.LevelDatName;
        }

        /// <summary> 获取子目录有'实体文件夹'的目录 </summary>
        /// <param name="levelDatPath"> 指示存档位置的文件<see cref="LevelDatName"/> </param>
        public bool TryInit(string levelDatPath, out List<string> localEntityDirList) {
            localEntityDirList = null;
            LocalEntityDirList.Clear();
            // 必须选择"level.dat"以确定存档
            if (!IsLevelDatPath(levelDatPath)) {
                // 交给外部处理
                // Logger.LogUserWarning($"没有选择{Flag.LevelDatName}");
                return false;
            }
            // 根目录
            Root = Path.GetDirectoryName(levelDatPath);
            Queue<string> dirPathQue = new Queue<string>();
            string dirPath;
            string childDirName;
            string localPath;
            dirPathQue.Enqueue(Root);
            while (dirPathQue.Count > 0) {
                dirPath = dirPathQue.Dequeue();
                foreach (var childDirPath in Directory.GetDirectories(dirPath)) {
                    childDirName = Path.GetFileName(childDirPath);
                    if (childDirName == Flag.EntityDirName) {
                        localPath = childDirPath[(Root.Length + 1)..];
                        LocalEntityDirList.Add(localPath);
                    }
                    dirPathQue.Enqueue(childDirPath);
                }

            }
            localEntityDirList = LocalEntityDirList;
            return true;
        }

        /// <summary> 获取保存实体的.mca文件中的Nbt </summary>
        public void StartReadMCA(int threadNum = 1, params string[] localEntityPaths) {
            MCAReader.Start(Root, threadNum, localEntityPaths);
        }

        /// <summary> 获取Nbt中的实体文件 </summary>
        public void StartReadEntityTag(IEnumerable<TagKV> filterDatas, int threadNum = 1, params string[] localEntityPaths) {
            if (filterDatas != null) {
                foreach (var item in filterDatas) {
                    EntityTagReader.SetTagFilter(item.key, item.value);
                }
            }
            EntityTagReader.Start(MCAReader, threadNum, localEntityPaths);
        }
    }
}