using fNbt;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MoNbt {
    // .mca数据格式: 
    // https://zh.minecraft.wiki/w/%E5%8C%BA%E5%9F%9F%E6%96%87%E4%BB%B6%E6%A0%BC%E5%BC%8F
    // https://zh.minecraft.wiki/w/区域文件格式
    // 下文中无论什么k指的都是KiB (1KiB=1024字节), 文件的大小永远是4KB的整数倍.
    /* 构成:
     *  文件头: 0-8191 - 8192字节
     *   0000-4095: 区块存储位置偏移和占用扇区数
     *   4096-8191: 区块时间戳
     *  区块数据: 8192 - 
     *   
     */
    public static class ByteHelper {
        public static int ToInt(byte[] buffer, int i) {
            return (buffer[i] << 24) | (buffer[i + 1] << 16) | (buffer[i + 2] << 8) | buffer[i + 3] << 0;
        }
        // 0        1        2        3
        // 12345678 12345678 12345678 12345678
        public static int FromInt(int value, int offset) {
            offset = (3 - offset) * 8;
            return (value & (255 << offset)) >> offset;
        }
    }
    public struct ChunkHead {
        public int data;
        public int timeStamp;

        // 单位为4k扇叶,因为头数据占用最低为2
        public int DataOffset => ByteHelper.FromInt(data, 0) << 16 | ByteHelper.FromInt(data, 1) << 8 | ByteHelper.FromInt(data, 2);
        // 一个区块最多有255个扇叶 最大数据量为 255*4k ≈ 1M数据
        // 如果超出了这个数字，游戏会使用额外文件辅助保存这个区块, 没文件不考虑.
        public int DataSectorCount => ByteHelper.FromInt(data, 3);
        // 区块的最后更新时间: 对于记录实体的.mca,无数据的区块有有效的LastUpdatedTime
        public DateTime LastUpdatedTime => DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime;
        // 可用性: 认为有偏移则为有效数据
        public bool IsValid => DataOffset != 0;
    }
    public class ChunkData : IComparable<ChunkData> {
        static ConcurrentQueue<ChunkData> Queue = new ConcurrentQueue<ChunkData>();
        public static ChunkData Get(ChunkHead head) {
            ChunkData cd;
            if (!Queue.TryDequeue(out cd)) {
                cd = new ChunkData(head);
            }
            cd.InitData(head);
            return cd;
        }
        public static void ClearList(List<ChunkData> cdList) {
            foreach (var item in cdList) {
                Queue.Enqueue(item);
            }
            cdList.Clear();
        }

        public ChunkHead Head { get; private set; }
        public int DataOffset { get; private set; }
        public int DataSectorCount { get; private set; }
        public DateTime LastUpdatedTime { get; private set; }
        public NbtFile NBT { get; private set; }
        private ChunkData(ChunkHead head) {
            // InitData(head);
            NBT = new NbtFile();
        }
        void InitData(ChunkHead head) { 
            Head = head;
            DataOffset = head.DataOffset;
            DataSectorCount = head.DataSectorCount;
            LastUpdatedTime = head.LastUpdatedTime;
        }

        public override string ToString() {
            // return $"{nameof(DataOffset)}:{DataOffset} {nameof(DataSectorCount)}:{DataSectorCount} {nameof(LastUpdatedTime)}:{LastUpdatedTime} \n {NBT}";
            return NBT.ToString();
        }

        public int CompareTo(ChunkData other) {
            // 不存在重复情况
            if (DataOffset < other.DataOffset) {
                return -1;
            }
            else if (DataOffset > other.DataOffset) {
                return 1;
            }
            else { 
                return 0;
            }
        }
    }
    public class MCAParser {
        int length;

        const int SECTOR_SIZE = 4 * 1024;
        readonly List<ChunkData> list = new List<ChunkData>(1024);

        int ReadLength {
            get => length;
            set {
                // Logger.Log($"读取长度:{value}");
                length = value;
            }
        }

        // Debug
        public string SubstringStream(string filePath,int start, int length) {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                byte[] bs = new byte[length];
                StringBuilder sb = new StringBuilder();
                fs.Seek(start, SeekOrigin.Current);
                fs.Read(bs, 0, bs.Length);
                foreach (var item in bs) {
                    sb.Append($"{item} ");
                }
                return sb.ToString();
            }
        }

        // Clear
        public void Clear() {
            ChunkData.ClearList(list);
        }

        /// <summary> 异步解析 </summary>
        /// <param name="filePath"> 文件路径 </param>
        /// <returns> .mca文件里的<see cref="ChunkData"/>,通过<see cref="ChunkData.NBT"/>调用主要内容 </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<IEnumerable<ChunkData>> ParseAsync(string filePath) {
            list.Clear();
            ChunkHead[] chunkHeads = new ChunkHead[1024];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                string name = Path.GetFileName(filePath);
                if (fs.Length <= 2 * SECTOR_SIZE) {
                    // No data
                    Logger.Log($"{name}中无数据");
                    return list;
                }
                // 每个区块的数据不可能超过1MB,直接声明一个来回复用
                byte[] buffer = new byte[1024 * 1024];
                // 0000-4095: 区块存储位置偏移和占用扇区数
                ReadLength = await fs.ReadAsync(buffer, 0, SECTOR_SIZE);
                for (int i = 0; i < chunkHeads.Length; i++) {
                    chunkHeads[i].data = ByteHelper.ToInt(buffer, i * 4);
                }
                // 4096-8191: 区块时间戳
                ReadLength = await fs.ReadAsync(buffer, 0, SECTOR_SIZE);
                for (int i = 0; i < chunkHeads.Length; i++) {
                    chunkHeads[i].timeStamp = ByteHelper.ToInt(buffer, i * 4);
                }
                // 记录有效数据,创建数据类
                foreach (var head in chunkHeads) {
                    if (head.IsValid) {
                        list.Add(ChunkData.Get(head));
                    }
                }
                list.Sort();

                // pointer的语义是游标
                int pointer;
                int dataLength;
                // 根据ChunkData中解析的信息, 后面的数据不是连续有意义的,有时候会出现无人引用的4KB数据扇面(你🐎,我查了半天)
                // 已经读取了前面两个(0 1)数据头的扇面
                int sectorNum = 2;
                foreach (var chunk in list) {
                    // 无人引用的数据,跳过
                    while (sectorNum < chunk.DataOffset) {
                        fs.Seek(SECTOR_SIZE, SeekOrigin.Current);
                        sectorNum++;
                        Logger.Log($"{name}跳过了扇面{sectorNum}");
                        continue;
                    }
                    // 保险用, 正常的数据不会发生这种事情
                    if (sectorNum > chunk.DataOffset) {
                        throw new IndexOutOfRangeException($"索引超出区块记录");
                    }
                    // 记录区块占用的扇面数增量
                    sectorNum += chunk.DataSectorCount;
                    // 4字节的数据长度
                    ReadLength = await fs.ReadAsync(buffer, 0, 5);
                    pointer = ReadLength;
                    dataLength = ByteHelper.ToInt(buffer, 0);

                    // 1字节的压缩格式码, NBT数据自己在头部记录了压缩格式(你🐎,我查了半天)
                    byte z = buffer[4];

                    // 根据数据长度读入数据
                    // Logger.Log(dataLength.ToString());
                    ReadLength = await fs.ReadAsync(buffer, 0, dataLength);
                    pointer += ReadLength;

                    // 指明压缩格式
                    NbtCompression compression = z switch {
                        1 => NbtCompression.GZip,
                        2 => NbtCompression.ZLib,
                        3 => NbtCompression.None,
                        // fNbt未提供支持
                        // 4 => NbtCompression.LZ4,
                        // 拉来当报错壮丁
                        _ => NbtCompression.AutoDetect
                    };
                    if (compression == NbtCompression.AutoDetect) { 
                        throw new InvalidDataException($"使用了不支持的NBT压缩算法");
                    }

                    // 进行解析
                    chunk.NBT.LoadFromBuffer(buffer, 0, dataLength, compression);

                    // 按'扇面'组织的数据
                    // 采用有意义的数据后,有些无意义的空位,废弃掉.
                    pointer %= SECTOR_SIZE;
                    // 正好充分利用了扇面,不能做废弃处理
                    if (pointer != 0) {
                        pointer = SECTOR_SIZE - pointer;
                        ReadLength = fs.Read(buffer, 0, pointer);
                    }
                }

                return list;
            }
        }
    }
}