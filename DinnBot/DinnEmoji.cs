using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DinnBot
{
    public static class DinnEmoji
    {

        public static List<GuildEmote> Emojis = new List<GuildEmote>();

        public static void RetrieveEmojis(DiscordSocketClient _client)
        {
             var attributeList = DinnCardDatabase.DinnCards.Select(x => x.Attribute1).Distinct().ToList();
            attributeList.AddRange(DinnCardDatabase.DinnCards.Select(x => x.Attribute2).Distinct().ToList());
            attributeList.AddRange(DinnCardDatabase.DinnCards.Select(x => x.Attribute3).Distinct().ToList());
            attributeList.Add("heart");

            var uniqueAttributes = attributeList.Distinct();
            var test = _client.Guilds.SelectMany(x => x.Emotes).ToList();
            foreach (var a in uniqueAttributes)
            {
                if(a != null)
                {
                    var emoji = _client.Guilds.SelectMany(x => x.Emotes).FirstOrDefault(x => x.Name.IndexOf(a, StringComparison.OrdinalIgnoreCase) != -1);
                    if (emoji != null)
                        Emojis.Add(emoji);
                }
            }
        }

        public static string ConvertHeartsToEmojiString(int heartCount)
        {
            var r = "";
            var heartEmoji = Emojis.Where(x => x.Name.Equals("Heart", StringComparison.OrdinalIgnoreCase)).First();
            for(int i = 0; i < heartCount; i++)
            {
                r += $"{heartEmoji}";
            }

            return r;
        }

        public static AttributeObject ConvertAttributesToEmojiString(DinnCard card)
        {
            var attObj = new AttributeObject();
            attObj.Att1 = Emojis.Where(x => x.Name.Equals(card.Attribute1, StringComparison.OrdinalIgnoreCase)).First();
            attObj.Att2 = Emojis.Where(x => x.Name.Equals(card.Attribute2, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            attObj.Att3 = Emojis.Where(x => x.Name.Equals(card.Attribute3, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return attObj;

            //builder.AddField("Attribute 1", Emojis.Where(x => x.Name.Equals(card.Attribute1, StringComparison.OrdinalIgnoreCase)).First(), true);
            //if(card.Attribute2 != null)            
            //    builder.AddField("Attribute 2", Emojis.Select(x => x.Name.Equals(card.Attribute2, StringComparison.OrdinalIgnoreCase)).First(), true);
            //if(card.Attribute3 != null)
            //    builder.AddField("Attribute 3", Emojis.Select(x => x.Name.Equals(card.Attribute3, StringComparison.OrdinalIgnoreCase)).First(), true);
            //return builder;
        }
    }

    public class AttributeObject
    {
        public GuildEmote Att1 { get; set; }
        public GuildEmote? Att2 { get; set; }
        public GuildEmote? Att3 { get; set; }

        public override string ToString()
        {
            return $"{Att1}{Att2}{Att3}";
        }
    }
}
