using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Reflection;
using System.ComponentModel;

namespace DinnBot
{
    class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        static void Main(string[] args)
                => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            Console.WriteLine("Djinn - Version 0.5");
            _client = new DiscordSocketClient();
            _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false
            });
            _client.Log += Log;
            _commands.Log += Log;
            _services = ConfigureServices();

        }

        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();

            return map.BuildServiceProvider();
        }


        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerAsync();            
            // Centralize the logic for commands into a separate method.
            await InitCommands();

            var token = "Nzk5Nzg4MTQ3MzUyNjY2MTQy.YAIqwQ.-V_zLhz51_9453jIPTlQJW_RRyA";


            // Login and connect.
            await _client.LoginAsync(TokenType.Bot,
                // < DO NOT HARDCODE YOUR TOKEN >
                //Environment.GetEnvironmentVariable("DiscordToken"));
                token);
            await _client.StartAsync();

            //_client.Connected += _client_Connected;
            _client.Ready += _client_Ready;
            _client.UserJoined += _client_UserJoined;

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
        }

        private Task _client_Ready()
        {
            DinnEmoji.RetrieveEmojis(_client);     
            return Task.CompletedTask;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                DinnCardDatabase.LoadDatabase();
                DinnTermDatabase.LoadDatabase();
                Thread.Sleep(60000);
            }
        }

        private async Task _client_UserJoined(SocketGuildUser user)
        {

            if(user.Guild.Id == 724397985420804167)
            {
                var channel = _client.GetChannel(724397985420804170) as SocketTextChannel;
                await channel.SendMessageAsync($"Welcome to the Dinn Discord Server {user.Mention}! Let us know if you have any questions =)");

            }
        }

        private async Task InitCommands()
        {
            // Either search the program and add all Module classes that can be found.
            // Module classes MUST be marked 'public' or they will be ignored.
            // You also need to pass your 'IServiceProvider' instance now,
            // so make sure that's done before you get here.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<SomeModule>(_services);
            // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            // We don't want the bot to respond to itself or other bots.
            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;
            // Replace the '!' with whatever character
            // you want to prefix your commands with.
            // Uncomment the second half if you also want
            // commands to be invoked by mentioning the bot instead.
            if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully).
                var result = await _commands.ExecuteAsync(context, pos, _services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed.
                // This does not catch errors from commands with 'RunMode.Async',
                // subscribe a handler for '_commands.CommandExecuted' to see those.
                //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                //    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}
