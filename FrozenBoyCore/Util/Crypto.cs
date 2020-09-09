using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore.Util {
    public class Crypto {

        public static string MD5(byte[] backbuffer) {
            byte[] hash;
            using var md5 = System.Security.Cryptography.MD5.Create();
            md5.TransformFinalBlock(backbuffer, 0, backbuffer.Length);
            hash = md5.Hash;

            StringBuilder result = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
                result.Append(hash[i].ToString("X2"));

            return result.ToString();
        }
    }
}
