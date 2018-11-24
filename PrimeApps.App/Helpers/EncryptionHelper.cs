using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using PrimeApps.Model.Common.Cache;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Helpers
{
    public static class EncryptionHelper
    {
        public class EncryptedKey
        {
            public int FieldId { get; set; }
            public int TenantId { get; set; }
            public int AppId { get; set; }
            public string Key { get; set; }
        }

        public static string Encrypt(string clearText, int fieldId, UserItem appUser, IConfiguration _configuration)
        {
            /*
             string encryptionKey;

            //saves the key to elastic server
            var elasticConnectionSettings = new Nest.ConnectionSettings(new Uri(_configuration.GetConnectionString("ElasticConnection")));
            elasticConnectionSettings.DefaultIndex("encrypted_fields");
            elasticConnectionSettings.BasicAuthentication("elastic", "uC4yW8JABl63IVBUOlXeQFoX");
            var elasticClient = new Nest.ElasticClient(elasticConnectionSettings);
            var keyObj = new EncryptedKey();

            var response = elasticClient.Search<EncryptedKey>(s => s
                .Query(q => q.Term("fieldId", fieldId) && q.Term("tenantId", appUser.TenantId) && q.Term("appId", appUser.AppId))
            );

            if (response.Documents.Count > 0)
            {
                encryptionKey = response.Documents.First().Key;
            }
            else
            {
                var keySize = 256;
                encryptionKey = GenerateKey(keySize);
                keyObj = new EncryptedKey { FieldId = fieldId, TenantId = appUser.TenantId, AppId = appUser.AppId, Key = encryptionKey };
                var responseCreateIndex = elasticClient.Index(keyObj, idx => idx.Index("encrypted_fields"));
            }

            //creates an ecrypted string for field value
            var clearBytes = Encoding.Unicode.GetBytes(clearText);

            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }

            return clearText;
             */

            return "";
        }

        public static string Decrypt(string cipherText, int fieldId, UserItem appUser, IConfiguration _configuration)
        {
            /*
             var elasticConnectionSettings = new Nest.ConnectionSettings(new Uri(_configuration.GetConnectionString("ElasticConnection")));
            elasticConnectionSettings.DefaultIndex("encrypted_fields");
            elasticConnectionSettings.BasicAuthentication("elastic", "uC4yW8JABl63IVBUOlXeQFoX");
            var elasticClient = new Nest.ElasticClient(elasticConnectionSettings);

            var response = elasticClient.Search<EncryptedKey>(s => s
                .Query(q => q.Term("fieldId", fieldId) && q.Term("tenantId", appUser.TenantId) && q.Term("appId", appUser.AppId))
            );

            string encryptionKey = response.Documents.First().Key;
            var cipherBytes = Convert.FromBase64String(cipherText);

            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
             */

            return "";
        }

        private static string GenerateKey(int iKeySize)
        {
            var aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = iKeySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.GenerateIV();
            var ivStr = Convert.ToBase64String(aesEncryption.IV);
            aesEncryption.GenerateKey();
            var keyStr = Convert.ToBase64String(aesEncryption.Key);
            var completeKey = ivStr + "," + keyStr;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(completeKey));
        }
    }
}
