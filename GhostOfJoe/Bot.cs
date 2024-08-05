using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GhostOfJoe;

public class Bot : IBot{
    private static DiscordSocketClient _client;
    private static CommandService _commands;
    private static ServiceProvider? _services;
    private readonly ILogger<Bot> _logger;
    private readonly IConfiguration _configuration;
    
    private static readonly string DISCORD_KEY =
        "MTI2OTIxNDkxNjA3NDY3MjE5OQ.GxhXiw.Abg1sicQ0VjS2A-19ty6o0XCyE5-mprGU4dTo8";
    
    private static readonly string Prefix = "/";
    
    public static readonly ulong LoggingChannel = 1269384832064950384;
    public static readonly ulong AdminUser = 168496575369183232;
    
    
    public Bot(ILogger<Bot> logger, IConfiguration configuration) {
        _logger = logger;
        _configuration = configuration;

        DiscordSocketConfig config = new() {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers
        };

        _client = new DiscordSocketClient(config);
        _commands = new CommandService();
    }
    
    public async Task StartAsync(ServiceProvider services) {

        _logger.LogInformation($"Starting up with token {DISCORD_KEY}");

        _services = services;
        
        _commands = new CommandService();

        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        
        var socketConfig = new DiscordSocketConfig {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            | GatewayIntents.AllUnprivileged
        };
        
        _client = new DiscordSocketClient(socketConfig);
        
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleCommandAsync;
        _client.JoinedGuild += HandleGuildJoined;

        await _client.LoginAsync(TokenType.Bot, DISCORD_KEY);
        await _client.StartAsync();
        
        
        await Task.Delay(-1);
    }

    public Task StopAsync() {
        throw new NotImplementedException();
    }

    private static async Task HandleCommandAsync(SocketMessage messageParam){
        if (messageParam.Author.IsBot) {
            return;
        }
        
        SocketUserMessage message = (SocketUserMessage)messageParam;
        
        
        SocketCommandContext context = new SocketCommandContext(_client, message);
        
        int argPos = 0;
        if (message.HasStringPrefix(Prefix, ref argPos)) {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
        }
    }
    
    private static Task LogAsync(LogMessage log) {
        Console.WriteLine(log);

        ITextChannel? channel = _client.GetChannel(LoggingChannel) as ITextChannel;

        channel?.SendMessageAsync(log.ToString());

        return Task.CompletedTask;
    }
    
    private static Task ReadyAsync() {
        Console.WriteLine($"Logged in as {_client.CurrentUser.Username} - {_client.CurrentUser.Id}");
        return Task.CompletedTask;
    }

    private static async Task HandleGuildJoined(SocketGuild guild) {
        ulong joeID = 238096751607676928;
        
        // add an entry
        
        //check for joe
        SocketUser Joe = guild.GetUser(joeID);

        if (Joe != null) {
            var defaultChannel = guild.DefaultChannel;
            
            if (defaultChannel != null && defaultChannel is ITextChannel textChannel) {
                // Send a welcome message to the default channel
                await textChannel.SendMessageAsync($"oof");
                await Task.Delay(1000);

                await textChannel.SendMessageAsync("Well...");
                await Task.Delay(1000);

                await textChannel.SendMessageAsync("This is awkward...");
                await Task.Delay(700);
                
                await textChannel.SendMessageAsync(Joe.Mention);
            }
            else
            {
                Console.WriteLine("Default channel is not a text channel or does not exist.");
            }
        }
    }
}

public class CommandModule : ModuleBase<SocketCommandContext> {
        
        [Command("ping")]
        public async Task PingAsync() {
            var member = Context.User as SocketGuildUser;
            if (member == null) throw new ArgumentNullException(nameof(member));
            
            await ReplyAsync(Program.ApplyTitle(member));
        }

        [Command("grant_title")]
        public async Task GrantTitleAsync(SocketGuildUser member, [Remainder] string title) {
            ulong userId = member.Id;

            SocketGuildUser? holder = Program.GetMemberWithTitle(title, member.Guild);
            
            if (holder != null && holder != member) {
                

                await ReplyAsync($"Hark! {holder.Mention} already possesses the title '{title}' and no user " +
                                         $"may possess the title of another");
            }
            else if (holder != null && holder == member) {
                await ReplyAsync(
                    $"{member.Mention} already has the title '{title}'. Perhaps they need a different one?");
            }
            else {
                Program.AddTitle(member, title);
                await ReplyAsync($"{member.Mention} shall hereby be referred to as " +
                                 $"\"{title.Replace("<username>", member.Mention)}\"");
            }
        }

        [Command("revoke_title")]
        public async Task RevokeTitleAsync(SocketGuildUser member, [Remainder] string title) {
            

            

            if (Program.RemoveTitle(member, title)) {

                await ReplyAsync($"{member.Mention} has been striped of the title '{title}'");
            }
            else {
                ReplyAsync("my brain did a bad. Might have to talk to a doctor or something.");
            }
        }

        [Command("get_game_list")]
        public async Task GetGameListAsync() {
            // Retrieve the game list
            List<string> gameList = Program.GetGames(Context.Guild);

            
            // Format the game list into a single string
            string gameListString = string.Join("\n", $"- {gameList}");

            // Send the list to the user
            if (gameList.Count > 0) {
                await ReplyAsync($"**Games**:\n{gameListString}");
            } else {
                await ReplyAsync("No games available.");
            }
        }

        [Command("get_Game_Categories")]
        public async Task GetGameCategories(string game) {
            List<string>? categoryList = Program.GetCategoriesWithScores(Context.Message.Author, Context.Guild, game);
            
             if(categoryList != null){   
                 string categoryString = "";
            
                 foreach (string element in categoryList) {
                     categoryString += element + "\n";
                 }
                await ReplyAsync($"**Categories**:\n{categoryString}");
            }
            else {
                await ReplyAsync("We dont got that game bro :/");
            }
        }
        
        [Command("flow")]
        public async Task FlowAsync([Remainder] string userMessage) {
            
            ulong targetChannelId = Context.Channel.Id;
            
            ITextChannel channel = Context.Client.GetChannel(targetChannelId) as ITextChannel;

            Dictionary<string, SettingBase?> settings = Program.config.GlobalSettings;

            bool NsfwFlow = Program.GetSettingValue<bool>(settings, "NsfwFlow");
            
            if (NsfwFlow != channel.IsNsfw){
                await ReplyAsync("This command can only be used in NSFW channels.");
                return;
            }
            
            

            string result = Flow.CiteFlow(userMessage);
            await ReplyAsync(result);
        }

        [Command("get_global_settings")]
        public async Task GetGlobalSettingsAsync() {
            
            await ReplyAsync($"## Current Global Settings:\n{getSettings(Program.config.GlobalSettings)}");
        }

        [Command("set_global_setting")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetGlobalSettingAsync(string key, string value) {
            Dictionary<string, SettingBase?> settings = Program.config.GlobalSettings;

            Type SettingType = settings[key].getType();
            
            
            try {
                // Convert the string value to the appropriate type
                object convertedValue = Convert.ChangeType(value, SettingType);

                // Call the method to set the global settings
                await ReplyAsync(SetGlobalSetting(key, convertedValue));
            }
            catch (InvalidCastException) {
                await ReplyAsync($"Failed to convert '{value}' to a {SettingType.Name}.");
            }
            catch (FormatException) {
                await ReplyAsync($"The value '{value}' is not in a valid format for a {SettingType.Name}.");
            }
        }
        
        public string SetGlobalSetting<T>(string key, T value) {
            if (!Program.AdminServers.Contains(Context.Guild.Id)) {
                return ("Sorry, this server is not cool enough to have admin powers. Long live the Party Bus");
            }
            
            if (Program.config.GlobalSettings.ContainsKey(key)) {
                try {
                    Program.SetSettingValue(Program.config.GlobalSettings, key, value);
                }
                catch (InvalidCastException ex) {
                    return ex.Message;
                }
                catch (KeyNotFoundException ex) {
                    return ex.Message;
                }
                return ($"Setting '{key}' has been updated to '{value}'.");
            }
            else {
                return ($"Setting '{key}' does not exist.");
            }
        }
        
        [Command("get_settings")]
        public async Task getSettingsAsync() {

            Dictionary<string, string> targetSettings = Program.GetServerSettings(Context.Guild);

            string output = "";

            foreach (KeyValuePair<string,string> pair in targetSettings) {
                output += $"**{pair.Key}**: {pair.Value}\n";
            }
            
            await ReplyAsync($"## Current Local Settings:\n{output}");
        }

        private string getSettings(Dictionary<string, SettingBase?> settings) {
            List<string> settingList = new List<string>();
            
            foreach (string key in settings.Keys) {
                settingList.Add($"- **{key}** \n {settings[key]}");
            }

            return string.Join("\n", settingList);
        }
    }