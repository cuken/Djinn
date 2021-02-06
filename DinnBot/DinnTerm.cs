using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DinnBot
{
    public class DinnTerm
    {
        public string Name { get; set; }
        public string Definition { get; set; }
        public string Example { get; set; }
    }

    public static class DinnTermDatabase
    {
        public static List<DinnTerm> DinnTerms = new List<DinnTerm>();
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "DjinnCardDatabase";

        public static void LoadDatabase()
        {
            UserCredential credential;
            Console.WriteLine($"{DateTime.Now} - Updating DINN Term ruleset from Google Sheets.");

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            String spreadsheetId = "1YoOdowodbPv_QGvuvPcwvGA1yqyZjZBmuY0TlbUi56I";
            string range = "Keyword Definition!A:C";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            try
            {
                DinnTerms.Clear();
                for (int i = 0; i < values.Count; i++)
                {
                    if (i == 0)
                    {
                        //Skip first row as headers;
                        continue;
                    }
                    try
                    {
                        var term = new DinnTerm();
                        term.Name = values[i][0].ToString();
                        try
                        {
                            term.Definition = values[i][1].ToString();
                            term.Example = values[i][2].ToString();
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error adding {term.Name}, still attempting to add existing info. {ex.Message}");
                        }
                        DinnTerms.Add(term);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
