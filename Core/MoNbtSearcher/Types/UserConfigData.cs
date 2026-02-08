using fNbt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoNbtSearcher {
    [Serializable]
    public class UserConfigData {
        public int threadNum;
        public List<PlayerData> playerDataList = new List<PlayerData>();
        public int ThreadNum => Math.Clamp(threadNum, 1, 256);

        public UserConfigData() {
            threadNum = Flag.DefaultThreadNum;
        }
        // 十六进制带字母的
        public void AddPlayerUUIDMO(FtbTeamData data) {
            PlayerData pd = null;
            foreach (var item in playerDataList) {
                if (item.player_name == data.player_name) {
                    pd = item;
                    if (!pd.uuidMoList.Contains(data.id)) {
                        pd.uuidMoList.Add(data.id);
                    }
                    return;
                }
            }
            pd = new PlayerData();
            playerDataList.Add(pd);
            pd.player_name = data.player_name;
            pd.uuidMoList.Add(data.id);
        }
        // NBT里十进制的
        public void AddPlayerUUIDEN(string uuidMo, NbtIntArray nbtUUID) {
            PlayerData GetPlayerData(string id) {
                foreach (var pd in playerDataList) {
                    foreach (var pid in pd.uuidMoList) {
                        if (pid == id) {
                            return pd;
                        }
                    }
                }
                return null;
            }

            if (playerDataList.Count == 0) {
                return;
            }
            PlayerData player = GetPlayerData(uuidMo);
            // 无则使用MojangUUID作为名称
            if (player == null) {
                player = new PlayerData { 
                    player_name = uuidMo,
                };
                playerDataList.Add(player);
            }

            string uuid = PlayerData.GetUUIDString(nbtUUID);
            if (!player.uuidEnList.Contains(uuid)) { 
                player.uuidEnList.Add(uuid);
            }
        }
        // 尝试使用nbt里的uuid获得玩家数据
        public bool TryGetPlayerName(NbtIntArray nbtUUID, out PlayerData pd) {
            string uuid = PlayerData.GetUUIDString(nbtUUID);
            pd = null;
            foreach (var p in playerDataList) {
                if (p.uuidEnList.Contains(uuid)) {
                    pd = p;
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public class PlayerData {
        // 这能重复建议结婚
        //public static long GetLid(NbtIntArray nia) => nia.IntArrayValue.Select(v => (long)v).Sum();
        public static string GetUUIDString(NbtIntArray nia) => string.Join("", nia.IntArrayValue.Select(i=>i.ToString()));

        public string player_name;
        public List<string> uuidMoList = new List<string>();
        public List<string> uuidEnList = new List<string>();
        public bool isShow = true;
    }

    public class FtbTeamData {
        public string id;
        public string player_name;
    }
}