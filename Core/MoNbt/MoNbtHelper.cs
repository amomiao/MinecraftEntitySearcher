using fNbt;

namespace MoNbt {
    public static class MoNbtHelper {
        /// <summary> 增加了对<see cref="NbtCompound"/>转字符串的支持 </summary>
        public static string GetString(this NbtTag nbt) {
            string str = string.Empty;
            str += nbt switch {
                ICollection<NbtTag> iTag => string.Join(" ", iTag.Select((t) => t.GetString())),
                // 饿啊, 没有接口
                NbtByteArray nba => string.Join(" ", nba.IntArrayValue.Select(i => i.ToString())),
                NbtIntArray nia => string.Join(" ", nia.IntArrayValue.Select(i => i.ToString())),
                NbtLongArray nla => string.Join(" ", nla.IntArrayValue.Select(i => i.ToString())),
                _ => string.Join(" ", nbt.StringValue)
            };
            return str;
        }
    }
}