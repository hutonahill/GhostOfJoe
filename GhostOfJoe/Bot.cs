using System.Diagnostics;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GhostOfJoe;

public class Bot : IBot{
    private static DiscordSocketClient _client;
    private static CommandService _commands;
    private static ServiceProvider? _services;
    private readonly ILogger<Bot> _logger;
    private readonly IConfiguration _configuration;
    
    private static readonly string DISCORD_KEY =
        "MTI2OTIxNDkxNjA3NDY3MjE5OQ.GxhXiw.Abg1sicQ0VjS2A-19ty6o0XCyE5-mprGU4dTo8";
    
    private static readonly string Prefix = "!";
    
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
        _client.Ready += ClientReadyAsync;
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
    
    public static async Task LogAsync(LogMessage log) {
        Console.WriteLine(log);

        ITextChannel? channel = _client.GetChannel(LoggingChannel) as ITextChannel;
        
        if (log.Severity == LogSeverity.Critical) {
            SocketUser admin = _client.GetUser(AdminUser);
            
            await channel?.SendMessageAsync($"{admin.Mention}: `{log.ToString()}`")!;
        }
        else {
            await channel?.SendMessageAsync(log.ToString())!;
        }
    }
    
    private static async Task ClientReadyAsync() {
        
        await _client.Rest.DeleteAllGlobalCommandsAsync();

        SlashCommandProperties speakCommand = new SlashCommandBuilder()
            .WithName("speak")
            .WithDescription("Tells it to say")
            .AddOption("text", ApplicationCommandOptionType.String, "The text you want the bot to reply with", 
                true, true).Build();
        
        
        try {
            await _client.Rest.CreateGlobalCommand(speakCommand);
            

        }
        catch(ApplicationCommandException exception) {

            await LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(Bot)}.{nameof(ClientReadyAsync)}",
                "Something went wrong when registering commands.", exception));
        }
        catch(HttpException exception) {

            await LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(Bot)}.{nameof(ClientReadyAsync)}",
                "Something went wrong when registering commands.", exception));
        }
        
        Console.WriteLine($"Logged in as {_client.CurrentUser.Username} - {_client.CurrentUser.Id}");
    }
    
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        // Let's add a switch statement for the command name so we can handle multiple commands in one event.
        switch(command.Data.Name)
        {
            case "speak":
                await HandleSpeak(command);
                break;
        }
    }

    public async Task HandleSpeak(SocketSlashCommand command) {
        string text = (string)command.Data.Options.First().Value;

        await command.RespondAsync(text);
    }

    private static async Task HandleGuildJoined(SocketGuild guild) {
        ulong joeID = 238096751607676928;
        
        // add an entry
        Program.AddServer(guild);
        
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
        
    [SlashCommand("ping", "You there?")]
    public async Task PingAsync() {
        var member = Context.User as SocketGuildUser;
        if (member == null) throw new ArgumentNullException(nameof(member));
        
        await ReplyAsync(Program.ApplyTitle(member));
    }
    
    /*[SlashCommand("speak", "simon says")]
    public async Task SpeakAsync(string text) {
        SocketSlashCommand command =
            new SocketSlashCommand(Context.Client, new InteractionContext(Context.Client.Rest), Context.Channel, Context.User);

    }*/

    [SlashCommand("grant_title", "Commemorate acts of glory by granting the honored one a glorious title!")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task GrantTitleAsync(SocketGuildUser member, [Remainder] string title) {

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
    
    
    [SlashCommand("revoke_title", "Strips a disgraced bussy of that once glorious title")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RevokeTitleAsync(SocketGuildUser member, [Remainder] string title) {

        if (Program.RemoveTitle(member, title)) {

            await ReplyAsync($"{member.Mention} has been striped of the title '{title}'");
        }
        else {
            await ReplyAsync("my brain did a bad. Might have to talk to a doctor or something.");
        }
    }
    
    
    [SlashCommand("get_game_list", "Returns a list of games from the high score database")]
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

    [SlashCommand("add_game", "Adds a command to the high score database")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddGameAsync([Remainder] string game) {
        if (Program.AddGame(Context.Guild, game)) {
            await ReplyAsync($"{game} has been added!");
        }
        else {
            await ReplyAsync("Sorry bub, can't do that one. I bet you already tried to add that");
        }
    }


    [SlashCommand("remove_game", "Removes a game from the high score database")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveGameAsync([Remainder] string game) {
        bool success = false;
        try {
            success = Program.RemoveGame(Context.Guild, game);
        }
        catch (Exception ex) {
            await Bot.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(RemoveCategoryAsync)}",
                $"Game removal failed: {ex.Message}", ex));
        }
        
        
        if (success) {
            await ReplyAsync("The game is gone. Sure hope I didnt delete anything to impressive.");
        }
        else {
            await ReplyAsync("My dude doesn't like a game so much they try to delete it and it didnt even exist. Or perhaps I am crazy");
        }
    }
    
    
    [SlashCommand("get_game_categories", "Get the categories for a game in the high score database")]
    public async Task GetGameCategoriesAsync(string game) {
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


    /*public async Task AddCategoryGameAutocomplete() {
        string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();
        
        IEnumerable<AutocompleteResult> results = new[]
        {
            new AutocompleteResult("foo", "foo_value"),
            new AutocompleteResult("bar", "bar_value"),
            new AutocompleteResult("baz", "baz_value"),
        }.Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase)); // only send suggestions that starts with user's input; use case insensitive matching


        // max - 25 suggestions at a time
        await (Context.Interaction as SocketAutocompleteInteraction).RespondAsync(results.Take(25));
    }*/
    
    [SlashCommand("add_category", "Adds a category to a game ")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddCategoryAsync(string game, string category, string unit, bool higherBetter) {
        bool success = false;
        try {
            success = Program.AddCategory(Context.Guild, game, category, unit, higherBetter);
        }
        catch (InvalidOperationException ex) {
            await ReplyAsync($"{game}? never heard of it. But {category} certainly sounds interesting. " +
                             $"You will have to tell me about this game.");
        }
        catch (Exception ex) {
            await ReplyAsync("I dont know man... I dont feel so good. Somethings off. My brain did a goof. ");
            await Bot.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(AddCategoryAsync)}",
                "Failed to add game category", ex));
        }

        if (success) {
            await ReplyAsync(
                $"Well, well, well. Another realm of competition. {category}, eh? " +
                $"Ive always though motes banked was enough, but to each their own.");
        }
        else {
            
        }
    }
    
    
    [SlashCommand("removing_category", "Removes a category from a game")]
    
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveCategoryAsync(string game, string category) {
        bool success = false;
        try {
            success = Program.RemoveCategory(Context.Guild, game, category);
        }
        catch (Exception ex) {
            await Bot.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(RemoveCategoryAsync)}",
                $"Game removal failed: {ex.Message}", ex));
        }
        
        
        if (success) {
            await ReplyAsync("The game is gone. Sure hope I didnt delete anything to impressive.");
        }
        else {
            await ReplyAsync("My dude doesn't like a game so much they try to delete it and it didnt even exist. Or perhaps I am crazy");
        }
    }
    
    
    [SlashCommand("flow", "cite a passage from the Book of Flow")]
    public async Task FlowAsync([Remainder] string userMessage) {
        
        ulong targetChannelId = Context.Channel.Id;
        
        ITextChannel? channel = Context.Client.GetChannel(targetChannelId) as ITextChannel;

        Debug.Assert(Program.config != null, "Program.config != null");
        Dictionary<string, SettingBase?> settings = Program.config.GlobalSettings;

        bool NsfwFlow = Program.GetSettingValue<bool>(settings, "NsfwFlow");

        Debug.Assert(channel != null, nameof(channel) + " != null");
        if (NsfwFlow != channel.IsNsfw){
            await ReplyAsync("This command can only be used in NSFW channels.");
            return;
        }
        
        

        string result = Flow.CiteFlow(userMessage);
        await ReplyAsync(result);
    }
    
    
    [SlashCommand("get_global_settings", "get a list of my global settings. Look don't touch.")]
    public async Task GetGlobalSettingsAsync() {
        Debug.Assert(Program.config != null, "Program.config != null");
        await ReplyAsync($"## Current Global Settings:\n{getSettings(Program.config.GlobalSettings)}");
    }
    
    
    [SlashCommand("set_global_setting", "Modify my global settings. Bussies only.")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetGlobalSettingAsync(string key, string value) {
        if (Program.AdminServers.Contains(Context.Guild.Id)) {
            Debug.Assert(Program.config != null, "Program.config != null");
            Dictionary<string, SettingBase?> settings = Program.config.GlobalSettings;

            Type SettingType = settings[key]!.getType();
        
        
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
        else {
            await ReplyAsync("Sorry, this servers not cool enough to have admin powers. Long live the party bus");
        }
    }
    
    private string SetGlobalSetting<T>(string key, T value) {
        if (!Program.AdminServers.Contains(Context.Guild.Id)) {
            return ("Sorry, this server is not cool enough to have admin powers. Long live the Party Bus");
        }

        Debug.Assert(Program.config != null, "Program.config != null");
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
    
    
    [SlashCommand("get_settings", "Get a list of the servers settings")]
    public async Task getSettingsAsync() {

        Dictionary<string, string> targetSettings = Program.GetServerSettings(Context.Guild);

        string output = "";

        foreach (KeyValuePair<string,string> pair in targetSettings) {
            output += $"**{pair.Key}**: {pair.Value}\n";
        }
        
        await ReplyAsync($"## Current Local Settings:\n{output}");
    }
    
    
    [SlashCommand("set_settings", "Change your server settings.")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task SetSettingsAsync(string key, string value) {
        try {
            var (exists, dataType) = Program.GetSettingDataType(key);
    
            if (!exists || dataType == null) {
                await ReplyAsync($"The setting '{key}' does not exist.");
                return;
            }

            if (!Program.TryParseType(dataType, value, out var parsedValue)) {
                await ReplyAsync($"Invalid value '{value}' for the setting '{key}' of type '{dataType}'.");
                return;
            }

            if (Program.UpdateSetting(key, parsedValue, Context.Guild)) {
                await ReplyAsync($"Successfully updated the setting '{key}' to '{value}'.");
            } else {
                await ReplyAsync($"Failed to update the setting '{key}'.");
            }
        } catch (Exception ex) {
            await ReplyAsync($"An error occurred: {ex.Message}");
        }
    }
    
    private string getSettings(Dictionary<string, SettingBase?> settings) {
        List<string> settingList = new List<string>();
        
        foreach (string key in settings.Keys) {
            settingList.Add($"- **{key}** \n {settings[key]}");
        }

        return string.Join("\n", settingList);
    }
}