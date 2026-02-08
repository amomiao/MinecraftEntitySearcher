using fNbt;
using MoNbt;
using System.Collections.Generic;
using System.Text;

namespace MoNbtSearcher {
    public class EntityPoiData {
        public const string Owner = "Owner";

        public readonly static string[] DefaultPoiKeys = new string[] {
            "id",
            "Pos",
            Owner,
            "Health",
            "ModelId",
        };

        public string dim = string.Empty;
        public List<TagKV> tags = new List<TagKV>();
        public readonly NbtCompound RawTag = null;

        public EntityPoiData(string dim, NbtCompound comp, params string[] pois) {
            this.dim = dim;
            if (pois == null || pois.Length == 0) {
                pois = DefaultPoiKeys;
            }
            RawTag = comp;
            // pois = RawTag.Names.Select((t) => t.ToString()).ToArray();
            foreach (var poi in pois) {
                if (!comp.TryGet<NbtTag>(poi, out var poiTag)) {
                    continue;
                }
                tags.Add(new TagKV(poi, poiTag));
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            MoNbtSearcherHelper.TryDimI18N(dim,out string name);
            NbtTag nbtTag;
            string key;
            string value;
            sb.AppendLine($"维度:{name}");
            foreach (var kv in tags) {
                nbtTag = kv.tag;
                key = kv.key;
                value = nbtTag.GetString();
                if (kv.key == "Pos") {
                    value = "XYZ:" + value;
                }
                else if (kv.key == "Owner" &&
                    nbtTag is NbtIntArray uuid &&
                    MoNbtSearcherHelper.TryIdI18N(uuid, ref value)) {
                }
                sb.AppendLine($"{key}:{value}");
            }
            return sb.ToString();
        }
    }
}