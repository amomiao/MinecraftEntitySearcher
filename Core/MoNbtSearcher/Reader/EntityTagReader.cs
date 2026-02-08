using fNbt;
using MoNbt;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MoNbtSearcher {
    public class EntityTagReader : IThreadWorker {
        ConcurrentDictionary<string, ConcurrentQueue<NbtCompound>> entityQueDic = new ConcurrentDictionary<string, ConcurrentQueue<NbtCompound>>();
        ConcurrentDictionary<string, string> filterDic = new ConcurrentDictionary<string, string>();

        public int LoadTotalNum { get; private set; }
        public int LoadedNum { get; private set; }
        public bool IsCompleted { get; private set; }

        public IEnumerable<KeyValuePair<string, ConcurrentQueue<NbtCompound>>> DimEntityTagDic => entityQueDic;

        /// <summary> 设置阅读时的过滤 </summary>
        public void SetTagFilter(string key, string value) {
            // 单线程做 不会错
            if (!filterDic.ContainsKey(key)) {
                filterDic.TryAdd(key, string.Empty);
            }
            filterDic[key] = value;
        }

        public void Clear() {
            entityQueDic.Clear();
            filterDic.Clear();
        }

        bool Filter(NbtCompound comp) {
            foreach (var item in filterDic) {
                // 没有请求搜索的Key, 失败
                if (!comp.TryGet(item.Key, out var value)) {
                    return false;
                }
                string checkValue = item.Key switch {
                    // 对Owner做特殊处理
                    EntityPoiData.Owner => PlayerData.GetUUIDString((NbtIntArray)value),
                    _ => value.GetString()
                };
                if (!checkValue.Contains(item.Value)) {
                    Logger.Log($"{item.Key}: {value} != {item.Value}");
                    return false;
                }
            }
            return true;
        }

        bool TryAddEntity(NbtTag e, ref ConcurrentQueue<NbtCompound> entityQue, ref Queue<NbtTag> passengerQue) {
            if (e is not NbtCompound c) {
                return false;
            }
            // 把乘客类算进来
            if (c.TryGet<NbtList>(Flag.PassengersTagName, out var plist)) {
                foreach (var item in plist) {
                    passengerQue.Enqueue(item);
                }
            }
            // 过滤
            if (!Filter(c)) {
                return false;
            }
            entityQue.Enqueue(c);
            return true;
        }

        void ReadChunkNbt(NbtFile chunkNbt, ref ConcurrentQueue<NbtCompound> entityQue) {
            // elist后续要增加挂载的乘客,所以不能在这里顺手遍历
            if (!chunkNbt.RootTag.TryGet<NbtList>(Flag.EntityTagName, out var elist)) {
                return;
            }
            // 线程专属, 无安全风险
            Queue<NbtTag> passengerQue = new Queue<NbtTag>();
            // 这个List使用for遍历也只能加一次
            foreach (var e in elist) {
                TryAddEntity(e,ref entityQue, ref passengerQue);
            }
            // 单开一个队列来处理乘客
            while (passengerQue.Count > 0) {
                TryAddEntity(passengerQue.Dequeue(), ref entityQue, ref passengerQue);
            }
        }

        public void Start(MCAReader mcaReader, int threadNum = 1, params string[] localEntityDirPaths) {
            IsCompleted = false;
            ConcurrentQueue<NbtFile>[] ques = new ConcurrentQueue<NbtFile>[localEntityDirPaths.Length];
            for (int i = 0; i < localEntityDirPaths.Length; i++) {
                ques[i] = mcaReader.GetDimNbtQue(localEntityDirPaths[i]);
            }
            // 这里没有多线程, 不会出错
            foreach (var localEntityDirPath in localEntityDirPaths) {
                if (!entityQueDic.ContainsKey(localEntityDirPath)) {
                    entityQueDic.TryAdd(localEntityDirPath, new ConcurrentQueue<NbtCompound>());
                }
                // 新加载, 清除旧数据
                entityQueDic[localEntityDirPath].Clear();
            }
            LoadTotalNum = ques.Sum((q) => q.Count);
            LoadedNum = 0;
            this.ThreadAsyncDo(
                threadNum,
                async () => {
                    ConcurrentQueue<NbtFile> inQue;
                    ConcurrentQueue<NbtCompound> outQue;
                    for (int i = 0; i < ques.Length; i++) {
                        inQue = ques[i];
                        outQue = entityQueDic[localEntityDirPaths[i]];
                        while (inQue.Count > 0) {
                            if (!inQue.TryDequeue(out var nf)) {
                                // 没有成功取出, 跳出
                                break;
                            }
                            ReadChunkNbt(nf, ref outQue);
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