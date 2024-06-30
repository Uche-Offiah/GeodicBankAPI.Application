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
    }
}
