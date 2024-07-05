using Newtonsoft.Json.Linq;
using GeodicBankAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JsonContent = GeodicBankAPI.Models.JsonContent;

namespace GeodicBankAPI
{
    public static class GodicUtil
    {
        public static void LogStuff(string stuff, string folderName)
        {
            var directory = $"C:/{folderName}/";
            var fileName = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.DayOfYear.ToString() + "_" + "Log.txt";
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.AppendAllText(directory + fileName, Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + stuff + Environment.NewLine + Environment.NewLine);
        }

        public static async Task SendSms(string receiverNumber, string body, string apiToken, string from, string patientId)
        {
            //Util.LogStuff("url = " + url);
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
                    using (HttpResponseMessage res = await client.PostAsync(@"https://www.bulksmsnigeria.com/api/v1/sms/create", new JsonContent(new { api_token = apiToken, from, body, to = receiverNumber, dnd = 2 })).ConfigureAwait(false))
                    using (HttpContent content = res.Content)
                    {
                        var response = await content.ReadAsStringAsync().ConfigureAwait(false);
                        LogStuff(response, "GodicApiLogs");
                        res.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                LogStuff(ex.ToString(), "GodicApiLogs");
            }

        }

        public static async Task SendEmailNotification(object payload)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var httpResponse = await client.PostAsync($"https://placeholder.ng/api/Notifications/email", new JsonContent(payload)))
                {
                    var content = httpResponse.Content;
                    var resultString = await content.ReadAsStringAsync();
                    LogStuff($"Payload = {payload}\nResponse = {resultString}", "EmailNotifications");
                }
            }
        }

        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static byte[] EncryptStringToBytes(string plainText, string initializationVector)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("YourSecretKeyGoesHere");
                // Set an initialization vector (IV) for additional security
                aesAlg.IV = Encoding.UTF8.GetBytes(initializationVector);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        public static string DecryptStringFromBytes(byte[] cipherText, string initializationVector)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("YourSecretKeyGoesHere");

                // Set an initialization vector (IV) for additional security
                aesAlg.IV = Encoding.UTF8.GetBytes(initializationVector);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static dynamic GetValueOrDefault(JObject jObject, string propertyName)
        {
            if (jObject.TryGetValue(propertyName, out var value))
            {
                if (value.Type == JTokenType.String)
                {
                    return value.ToString();
                }
                else if (value.Type == JTokenType.Object)
                {
                    var nameProperty = value.Value<string>("Name");
                    if (nameProperty != null)
                    {
                        return nameProperty;
                    }
                }
                else if (value.Type == JTokenType.Boolean)
                {
                    return (bool)value ? "YES" : "NO";
                }
            }

            return null;
        }

        public static string ConvertPascalCaseToSplitWords(string pascalCaseString)
        {
            string splitWords = Regex.Replace(pascalCaseString, "(?<!^)([A-Z])", " $1");
            return splitWords.Trim();
        }

        static string RemoveTrailingDashes(string input)
        {
            if (input == null) return input;
            return Regex.Replace(input, @"^-+", "");
        }
    }
}
