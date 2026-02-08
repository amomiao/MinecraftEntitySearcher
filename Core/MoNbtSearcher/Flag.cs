using System.Collections.Generic;
using System.IO;

namespace MoNbtSearcher {
    public static class Flag {
        public static int DefaultThreadNum = 8;

        public readonly static string MCA = ".mca";
        public readonly static string SNBT = ".snbt";
        public readonly static string DAT = ".dat";

        public readonly static string LevelDatName = "level.dat";
        public readonly static string EntityDirName = "entities";
        public readonly static string EntityTagName = "Entities";
        public readonly static string PassengersTagName = "Passengers";
        public readonly static string FtbteamsPlayDirName = "ftbteams\\player";
        public readonly static string PlayerDataDirName = "playerdata";

        // exePath
        public readonly static string LocalCommonDirPath = "MoNbtSearcher/Common";
        public readonly static string ConfigName = "Config.json";
        public readonly static string DimI18NName = "DimI18N.toml";
        public readonly static string PoiI18NName = "PoiI18N.toml";

        public static string RootPath => MoNbtSearcherHelper.CrossPlatform.EXE_ROOT_PATH;
        public static string CommonPath => Path.Combine(RootPath, LocalCommonDirPath);
        public static string ConfigFullPath => Path.Combine(CommonPath, ConfigName);
        public static string DimI18NFullPath => Path.Combine(CommonPath, DimI18NName);
        public static string PoiI18NFullPath => Path.Combine(CommonPath, PoiI18NName);

        public static class Dim {
            public readonly static KeyValuePair<string, string> Overworld = new(EntityDirName, "Overworld");
            public readonly static KeyValuePair<string, string> Nether = new(Path.Combine("DIM-1", EntityDirName), "Nether");
            public readonly static KeyValuePair<string, string> End = new(Path.Combine("DIM1", EntityDirName), "End");
        }
    }
}