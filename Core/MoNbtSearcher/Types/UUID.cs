using System;
using System.Security.Cryptography;
using System.Text;

namespace MoNbtSearcher {
    // 请求在线UUID
    // https://api.mojang.com/users/profiles/minecraft/{username}

    [Obsolete("没做出来",true)]
    public class UUID {
        string playerName;
        public string PlayerName { 
            get => playerName;
            private set {
                playerName = value;
                ID = GetUUID(playerName);
            }
        }
        public string ID { get; private set; }

        public UUID(string playerName) {
            PlayerName = playerName;
        }

        public override string ToString() {
            return ID;
        }

        string GetUUID(string playerName) {
            // 1. 将玩家名称转为小写并进行 MD5 哈希
            using (MD5 md5 = MD5.Create()) {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(playerName.ToLower()));

                // 2. 修改哈希值的特定字节，确保符合 UUID v4 格式
                // 设置第6个字节为 UUID 版本4
                hashBytes[6] &= 0x0F;  // 清除高位
                hashBytes[6] |= 0x40;  // 设置为 UUID v4

                // 设置第8个字节为变体（RFC 4122）
                hashBytes[8] &= 0x3F;  // 清除高位
                hashBytes[8] |= 0x80;  // 设置为变体1

                return ToHex(hashBytes, false);
            }
        }

        string ToHex(byte[] bytes, bool lowerCase = true) {
            if (bytes == null)
                return null;
            var result = new StringBuilder();
            var format = lowerCase ? "x2" : "X2";
            for (var i = 0; i < bytes.Length; i++) {
                result.Append(bytes[i].ToString(format));
            }
            return result.ToString();
        }
    }
}