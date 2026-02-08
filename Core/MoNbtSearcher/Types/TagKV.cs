using fNbt;
using System;

namespace MoNbtSearcher {
    [Serializable]
    public class TagKV {
        public string key;
        public string value;
        public NbtTag tag;

        public TagKV(string key, string value) {
            this.key = key;
            this.value = value;
        }

        public TagKV(string key, NbtTag tag) {
            this.key = key;
            this.tag = tag;
        }
    }
}