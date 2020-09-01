using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Rest;

namespace FunctionApp2
{
    public class LyondellExtract
    {
        [FunctionName("LyondellExtract")]
        public static void Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            string connectionString = "Data Source=usdcadev00098.beis.com;Connection Timeout=120;Initial Catalog=XTool;Persist Security Info=False;  MultipleActiveResultSets=true; User=XToolUser; Password=XToolUserPassword1";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var text = "select * from Lyondell";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        var r = Serialize(dr);
                        string json = JsonConvert.SerializeObject(r, Formatting.Indented);
                        log.Info(json);
                     //   MakeRequest(json);
                    }
                }
            }
        }

        //public static void getToken()
        //{
        //    var uri = "https://login.microsoftonline.com/fbe62081-06d8-481dbaa0-34149cfefa5f/oauth2/v2.0/token";
        //    var client = new RestClient("https://login.microsoftonline.com/fbe62081-06d8-481dbaa0-34149cfefa5f/oauth2/v2.0/token");
            
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //    request.AddParameter("client_id", "15c9-9ec7-4b4d-88b3-abc358835522...");
        //    request.AddParameter("client_secret", "Ouo1LTz5uV52.jVf.h3Qy7ByQ_y-a...");
        //    request.AddParameter("grant_type", "client_credentials");
        //    request.AddParameter("scope", "api://247ce071-c688-4406-a6cc-bb5dd9843c7c/.default");
        //    IRestResponse response = client.Execute(request);
        //    Console.WriteLine(response.Content);
        //}

        private static IEnumerable<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }

        private static Dictionary<string, object> SerializeRow(IEnumerable<string> cols, SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        private static async void MakeRequest(string json)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "{subscription key}");
            client.DefaultRequestHeaders.Add("Authorization", "{access token}");
            var uri = "https://apim-eco-dev.lyondell.com/SIPASupplierData/v1/Insert Supplier Data?" + queryString;            HttpResponseMessage response;
            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(json);            using (var content = new StringContent(json))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await client.PostAsync(uri, content);
            }
        }
    }
}