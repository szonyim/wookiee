﻿using System.Security.Cryptography;
using System.Text;

namespace ExoticWookieeChat.Util
{
    public class PasswordUtil
    {
        /// <summary>
        /// Generate SHA256 hashed string from input
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string GenerateSHA256String(string inputString)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        /// <summary>
        /// Generate SHA512 hashed string from input
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        /// <summary>
        /// Generate string from byte array
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
    }
}