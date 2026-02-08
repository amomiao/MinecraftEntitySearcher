namespace Tommy.Helper {
    public class TommyInIKV {
        public string Key { get; private set; }
        public string InIComment { get; private set; }
        public string InIValue { get; private set; }

        /// <param name="key"> 键 </param>
        /// <param name="value"> 值 </param>
        /// <param name="comment"> 注释 </param>
        public TommyInIKV(string key, string value, string comment = "") {
            Key = key;
            InIComment = comment;
            InIValue = value;
        }

        public void Add2Table(TomlTable table) {
            table.Add(Key, new TomlString() {
                Comment  = InIComment,
                Value = InIValue,
            });
        }
    }
}