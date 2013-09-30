using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PluginVK
{
    public class Crypto
    {
        public string Encrypt(string str, string keyCrypt)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(str), keyCrypt));
        }

        public string Decrypt(string str, string keyCrypt)
        {
            if (str != null)
            {
                using (CryptoStream Cs = InternalDecrypt(Convert.FromBase64String(str), keyCrypt))
                using (StreamReader Sr = new StreamReader(Cs))
                {
                    return Sr.ReadToEnd();
                }
            }
            else return null;
        }

        private byte[] Encrypt(byte[] key, string value)
        {
            using (SymmetricAlgorithm Sa = Rijndael.Create())
            using (ICryptoTransform Ct = Sa.CreateEncryptor((new PasswordDeriveBytes(value, null)).GetBytes(16), new byte[16]))
            using(MemoryStream Ms = new MemoryStream())
            using (CryptoStream Cs = new CryptoStream(Ms, Ct, CryptoStreamMode.Write))
            {

                Cs.Write(key, 0, key.Length);
                Cs.FlushFinalBlock();

                byte[] Result = Ms.ToArray();
                return Result;
            }
        }

        private CryptoStream InternalDecrypt(byte[] key, string value)
        {
            SymmetricAlgorithm sa = Rijndael.Create();
            ICryptoTransform ct = sa.CreateDecryptor((new PasswordDeriveBytes(value, null)).GetBytes(16), new byte[16]);

            MemoryStream ms = new MemoryStream(key);
            return new CryptoStream(ms, ct, CryptoStreamMode.Read);
        }
    }
}
