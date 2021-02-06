using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace DinnBot
{
    public class DinnCard
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Attribute1 { get; set; }        
        public string Attribute2 { get; set; }
        public string Attribute3 { get; set; }        
        public int HeartValue { get; set; } = 0;
        public int BasePower { get; set; } = -10;
        public string cardText { get; set; }
        public string Released { get; set; }
        public string ImageURL { get; set; }

        public override string ToString()
        {
            var attributeString = $":{Attribute1}:";
            if (Attribute2 != null)
                attributeString += $" :{Attribute2}:";
            if (Attribute3 != null)
                attributeString += $" :{Attribute3}:";


            string message = "\n";
            message += $"Name: `{Name}`\n";
            message += $"Type: `{Type}`\n";
            message += $"Attributes: `{attributeString}`\n";

            if(HeartValue > 0)
            {
                message += $"Heart Value: `{HeartValue}`\n";
            }

            if(BasePower > -10)
            {
                message += $"BasePower: `{BasePower}`\n";
            }

            if (cardText.Length > 0)
                message += $"CardText: `{cardText}`\n";
            else
                message += $"CardText: `None!`\n";

            message += $"Released: `{Released}`";

            return message;
        }
    }

    public static class DinnCardDatabase
    {

        public static List<DinnCard> DinnCards = new List<DinnCard>();
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "DjinnCardDatabase";

        public static void LoadDatabase()
        {            
            //DinnCards = JsonConvert.DeserializeObject<List<DinnCard>>(File.ReadAllText("DinnCards.json"));
            UserCredential credential;

            Console.WriteLine($"{DateTime.Now} - Updating DINN Card Database from Google Sheets.");

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
            string range = "Master Card List!A:J";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            try
            {
                DinnCards.Clear();
                for (int i = 0; i < values.Count; i++)
                {
                    if (i == 0)
                    {
                        //Skip first row as headers;
                        continue;
                    }
                    try
                    {
                        var card = new DinnCard();
                        card.Name = values[i][0].ToString();
                        card.Type = values[i][1].ToString();
                        var attribute1 = values[i][2].ToString();
                        if (attribute1.Length > 1)
                            card.Attribute1 = attribute1;
                        var attribute2 = values[i][3].ToString();
                        if (attribute2.Length > 1)
                            card.Attribute2 = attribute2;
                        var attribute3 = values[i][4].ToString();
                        if (attribute3.Length > 1)
                            card.Attribute3 = attribute3;
                        var heartValueS = values[i][5].ToString();
                        var basePowerS = values[i][6].ToString();
                        if (heartValueS.Length > 0)
                            card.HeartValue = Int32.Parse(heartValueS);
                        if (basePowerS.Length > 0)
                            card.BasePower = Int32.Parse(basePowerS);
                        card.cardText = values[i][7].ToString();
                        card.Released = values[i][8].ToString();
                        card.ImageURL = values[i][9].ToString();

                        DinnCards.Add(card);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
