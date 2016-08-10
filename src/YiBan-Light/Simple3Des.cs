using System;
using System.Security.Cryptography;

namespace YiBan_Light_Lib
{
    /// <summary>
    /// 加密类
    /// </summary>
    internal sealed class Simple3Des : IDisposable
    {
        private readonly TripleDES _tripleDes = System.Security.Cryptography.TripleDES.Create();
       
        private static byte[] TruncateHash(string key, int length)
        {
            SHA1 sha1 = null;
            SHA1 tempsha1 = null;
            byte[] hash;
            try
            {
                tempsha1 = SHA1.Create();
                sha1 = tempsha1;
                tempsha1 = null;
                // Hash the key.
                byte[] keyBytes = System.Text.Encoding.Unicode.GetBytes(key);
                hash = sha1.ComputeHash(keyBytes);
                // Truncate or pad the hash.
                Array.Resize(ref hash, length);
            }
            finally
            {
                tempsha1?.Dispose();
                sha1?.Dispose();
            }
            return hash;
        }

        /// <summary>
        /// 用于生成包装类
        /// </summary>
        /// <param name="key">加密密码</param>
        public Simple3Des(string key)
        {
            // Initialize the crypto provider.
            _tripleDes.Key = TruncateHash(key, _tripleDes.KeySize / 8);
            _tripleDes.IV = TruncateHash("", _tripleDes.BlockSize / 8);
        }

        /// <summary>
        /// 加密文本
        /// </summary>
        /// <param name="plaintext">明文</param>
        /// <returns>密文</returns>
        public string EncryptData(string plaintext)
        {

            // Convert the plaintext string to a byte array.
            var plaintextBytes = System.Text.Encoding.Unicode.GetBytes(plaintext);

            // Create the stream.
            var ms = new System.IO.MemoryStream();
            // Create the encoder to write to the stream.
            var encStream = new CryptoStream(ms, _tripleDes.CreateEncryptor(), CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            encStream.Write(plaintextBytes, 0, plaintextBytes.Length);
            encStream.FlushFinalBlock();

            // Convert the encrypted stream to a printable string.
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="encryptedtext">密文</param>
        /// <returns>明文</returns>
        public string DecryptData(string encryptedtext)
        {

            // Convert the encrypted text string to a byte array.
            var encryptedBytes = Convert.FromBase64String(encryptedtext);

            // Create the stream.
            var ms = new System.IO.MemoryStream();
            // Create the decoder to write to the stream.
            var decStream = new CryptoStream(ms, _tripleDes.CreateDecryptor(), CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            decStream.FlushFinalBlock();

            // Convert the plaintext stream to a string.
            return System.Text.Encoding.Unicode.GetString(ms.ToArray());
        }

        /// <summary>
        /// 销毁该对象
        /// </summary>
        /// <param name="disposing">是否正在释放</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _tripleDes.Dispose();
            }

            // free native resources

        }
        //Dispose

        ~Simple3Des()
        {
            Dispose(false);
        }

        /// <summary>
        /// 销毁该对象
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }
        //Dispose



    }
}
