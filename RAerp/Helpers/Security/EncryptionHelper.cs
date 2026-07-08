using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RAerp.Helpers.Security
{
    public class EncryptionHelper
    {
        //public static string EncryptPassword(string password, string salt)
        //{
        //    var convertSalt = Convert.FromBase64String(salt);
        //    string encryptedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        //                                password: password,
        //                                salt: convertSalt,
        //                                prf: KeyDerivationPrf.HMACSHA256,
        //                                iterationCount: 10000,
        //                                numBytesRequested: 256 / 8
        //                            ));

        //    return encryptedPassword;
        //}

        public static string GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var randomGenerator = RandomNumberGenerator.Create())
            {
                randomGenerator.GetBytes(salt);
                return BitConverter.ToString(salt).Replace("-", "").ToLower();
            }
        }

        public static async Task<string> EncryptData(string plainData, string saltData)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(saltData);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            await streamWriter.WriteAsync(plainData);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static async Task<string>DecryptData(string encryptedData, string saltData)
        {
            if (IsBase64String(encryptedData))
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(encryptedData);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(saltData);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return await streamReader.ReadToEndAsync();
                            }
                        }
                    }
                }
            }
            else
                return encryptedData;
        }

        public static bool IsBase64String(string value)
        {
            Span<byte> buffer = new Span<byte>(new byte[value.Length]);
            return Convert.TryFromBase64String(value, buffer, out _);
        }
    }
}
