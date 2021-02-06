using Discord;
using Discord.Commands;
using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinnBot.Modules
{
    public class DinnCardModule : ModuleBase<SocketCommandContext>
    {
        [Command("lookup")]
        [Alias("card")]
        [Summary("Displays a Dinn card by name.")]
        public async Task SayAsync([Remainder][Summary("The card to lookup")] string cardName)
        {            
            var dinCard = DinnCardDatabase.DinnCards.Where(x => x.Name.Equals(cardName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var message = "";
            if(dinCard == null)
            {
                var cardList = DinnCardDatabase.DinnCards.Select(x => x.Name).ToList();
                var top = Process.ExtractOne(cardName, cardList);
                if(top.Score > 80)
                {
                    Console.WriteLine($"Found at least an 80% match for {cardName} -> {top.Value} | {top.Score}");
                    dinCard = DinnCardDatabase.DinnCards.Where(x => x.Name.Equals(top.Value, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();                    
                }
                else
                {
                    Console.WriteLine($"Sending -> {cardName} was not found in the Dinn Card Database! Did you mean `{top.Value} | {top.Score}`?");
                    message = $"{cardName} was not found in the Dinn Card Database! Did you mean `{top.Value}`?";
                    await Context.Channel.SendMessageAsync(message);
                    return;
                }

            }
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle(dinCard.Name);                
                builder.AddField("Type", dinCard.Type, true);
                if(dinCard.Attribute1 != null)
                    builder.AddField("Attributes", DinnEmoji.ConvertAttributesToEmojiString(dinCard).ToString(), true);
                if (dinCard.HeartValue > 0)
                    builder.AddField("Heart Value", DinnEmoji.ConvertHeartsToEmojiString(dinCard.HeartValue).ToString(), true);
                //builder.AddField("Heart Value", dinCard.HeartValue, true);
                if (dinCard.BasePower > -10)
                    builder.AddField("Base Power", dinCard.BasePower, true);
                builder.AddField("Card Text", dinCard.cardText, false);
                builder.WithFooter(footer => footer.Text = $"Released: {dinCard.Released}");
                builder.WithThumbnailUrl(dinCard.ImageURL);
                if (dinCard.Attribute1 == "Sentient")
                    builder.WithColor(Color.Red);
                else if (dinCard.Attribute1 == "Beast")
                    builder.WithColor(Color.Purple);
                else if (dinCard.Attribute1 == "Divine")
                    builder.WithColor(Color.Orange);
                else if (dinCard.Attribute1 == "Horror")
                    builder.WithColor(Color.LightOrange);
                else if (dinCard.Attribute1 == "Given")
                    builder.WithColor(Color.Blue);
                else if (dinCard.Attribute1 == "Essence")
                    builder.WithColor(Color.Green);
                                
                await Context.Channel.SendMessageAsync("", false, builder.Build());                
                        
        }
        
        [Command("define")]
        [Alias("keyword","term")]
        [Summary("Defines a given keyword.")]
        public async Task DefineAsync([Remainder][Summary("The term to define")] string termName)
        {
            var dinnTerm = DinnTermDatabase.DinnTerms.Where(x => x.Name.Equals(termName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var message = "";
            if (dinnTerm == null)
            {
                message = $"{termName} was not found in the Dinn Term Database :(";
                await Context.Channel.SendMessageAsync(message);
            }
            else
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle(dinnTerm.Name);
                builder.AddField("Definition", dinnTerm.Definition, false);
                if(dinnTerm.Example != null)
                {
                    builder.AddField("Example", dinnTerm.Example, false);
                }

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }
    }
}
