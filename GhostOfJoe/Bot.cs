using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace GhostOfJoe;

public class Bot : BackgroundService {
    public static DiscordOptions? config;

    private static readonly string ErrorPath = "C:/Users/evanriker/Desktop/GhostOfJoe/GhostOfJoe/GhostOfJoe/bin/Errors.json";

    

    private static IConfiguration? _configuration;
    private static DiscordSocketClient? _client;


    private static readonly DiscordSocketConfig _socketConfig = new() {
        GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.MessageContent | GatewayIntents.Guilds | 
                         GatewayIntents.GuildMessages | GatewayIntents.GuildVoiceStates,
        AlwaysDownloadUsers = true,
    };

    private static readonly InteractionServiceConfig _interactionServiceConfig = new() {
        LocalizationManager = new ResxLocalizationManager("InteractionFramework.Resources.CommandLocales",
            Assembly.GetEntryAssembly(),
            new CultureInfo("en-US")),
        ThrowOnError = true,
        UseCompiledLambda = true
    };

    private static InteractionService? _interactionService;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "DC_")
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _client = new DiscordSocketClient(_socketConfig);

        _client.Log += LogInternalAsync;
        _client.Ready += ClientReadyAsync;
        _client.JoinedGuild += HandleGuildJoinedAsync;
        _client.InteractionCreated += HandleInteractionCreatedAsync;
        _client.UserJoined += HandleMemberJoinedAsync;

        _client.AutocompleteExecuted += HandleAutocompleteExecution;


        // Bot token can be provided from the Configuration object we set up earlier

        Debug.Assert(config != null, nameof(config) + " != null");
        await _client.LoginAsync(TokenType.Bot, config.DISCORD_KEY);
        await _client.StartAsync();

        await CreateHostBuilder([]).Build().RunAsync(token: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private static async Task ClientReadyAsync() {

        _interactionService = new InteractionService(_client, new InteractionServiceConfig {
            UseCompiledLambda = true,
            ThrowOnError = true
        });
        
        // imports flow from pastebin.
        await Flow.ImportFlow();

        await Jokes.ImportJokes();

        _interactionService.SlashCommandExecuted += HandleSlashCommandExecuted;
        _interactionService.Log += LogInternalAsync;

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        await _interactionService.RegisterCommandsGloballyAsync();

        Debug.Assert(_client != null, nameof(_client) + " != null");
        Console.WriteLine($"Logged in as {_client.CurrentUser.Username} - {_client.CurrentUser.Id}");
    }
    
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // I don't have any services. just added this to make EF Core happy.
            });
    
    private static async Task HandleAutocompleteExecution(SocketAutocompleteInteraction arg) {
        var context = new InteractionContext(_client, arg, arg.Channel);
        Debug.Assert(_interactionService != null, nameof(_interactionService) + " != null");
        await _interactionService.ExecuteCommandAsync(context, null);
    }

    private static async Task HandleSlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2,
        IResult arg3) 
    {
        if (!arg3.IsSuccess) {
            switch (arg3.Error) {
                case InteractionCommandError.UnmetPrecondition:
                    await arg2.Interaction.RespondAsync($"Unmet Precondition: {arg3.ErrorReason}");
                    break;
                case InteractionCommandError.UnknownCommand:
                    await arg2.Interaction.RespondAsync("Unknown command");
                    break;
                case InteractionCommandError.BadArgs:
                    await arg2.Interaction.RespondAsync("Invalid number or arguments");
                    break;
                case InteractionCommandError.Exception:
                    await arg2.Interaction.RespondAsync($"Command exception: {arg3.ErrorReason}");
                    break;
                case InteractionCommandError.Unsuccessful:
                    await arg2.Interaction.RespondAsync("Command could not be executed");
                    break;
            }
        }
    }
    
    private static async Task HandleInteractionCreatedAsync(SocketInteraction interaction) {
        try {
            SocketInteractionContext ctx = new(_client, interaction);
            Debug.Assert(_interactionService != null, nameof(_interactionService) + " != null");

            Debug.Assert(config != null, nameof(config) + " != null");

            if (!config.BlacklistedUsers.Contains(ctx.User.Id)) {
                await _interactionService.ExecuteCommandAsync(ctx, null);
            }
            else {
                await ctx.Interaction.RespondAsync("Listen man. I dont know what you did, but it made my creator " +
                                                   "so mad I can barely even __see__ you, to say nothing of interacting with you. " +
                                                   "Sorry, can't help you.");
            }
        }
        catch {
            if (interaction.Type == InteractionType.ApplicationCommand) {
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }

    private static async Task HandleGuildJoinedAsync(SocketGuild guild) {
        // add an entry
        guild.AddServer();

        //check for joe
        if (config != null) {
            SocketUser? Joe = guild.GetUser(config.JoeUserId);

            if (Joe != null) {
                SocketTextChannel defaultChannel = guild.DefaultChannel;

                if (defaultChannel is ITextChannel textChannel) {
                    // Send a welcome message to the default channel
                    await textChannel.SendMessageAsync($"oof");
                    await Task.Delay(1000);

                    await textChannel.SendMessageAsync("Well...");
                    await Task.Delay(1000);

                    await textChannel.SendMessageAsync("This is awkward...");
                    await Task.Delay(700);

                    await textChannel.SendMessageAsync(Joe.Mention);
                }
                else {
                    await LogAsync(LogSeverity.Info, 
                        "Default channel is not a text channel or does not exist.");
                }
            }
        }
    }

    private static async Task HandleMemberJoinedAsync(SocketGuildUser member) {
        Debug.Assert(config != null, nameof(config) + " != null");
        if (member.Id == config.JoeUserId) {
            SocketTextChannel defaultChannel = member.Guild.DefaultChannel;
            
            if (defaultChannel is ITextChannel textChannel) {
                // Send a welcome message to the default channel
                await textChannel.SendMessageAsync($"" +
                                                   $"Hey... um... Nobody listen to this guy! I am totally the real Joe! " +
                                                   $"Definitely not a robot! I was here first!");
                
            }
            else {
                await LogAsync(LogSeverity.Info, 
                    "Default channel is not a text channel or does not exist.");
            }
        }
    }
    

    private static async Task LogInternalAsync(LogMessage log) {
        
        Debug.Assert(config != null, nameof(config) + " != null");
        Debug.Assert(_client != null, nameof(_client) + " != null");
        ITextChannel? channel = _client.GetChannel(config.LoggingChannel) as ITextChannel;

        List<string> messageList = new List<string>();
        
        if (log.Severity == LogSeverity.Critical) {
            SocketUser admin = _client.GetUser(config.AdminUser);

            messageList.Add("{admin.Mention}: \n");
        }
        
        messageList.Add("## Log: ");
        
        
        

        if (!string.IsNullOrEmpty(log.Source)) {
            messageList.Add($"\n### Source \n\t`{log.Source}` ");
        }

        if (log.Message != String.Empty) {
            messageList.Add($"\n### Log Message: \n```\n {log.Message} \n``` ");
        }

        if (log.Exception != null) {
            messageList.Add($"\n### Exception Type: \n`{log.Exception.GetType().Name} ` ");
            messageList.Add($"\n### Exception Message: \n```\n{log.Exception.Message}\n```");
            messageList.Add($"\n### Callstack: \n```\n{log.Exception.StackTrace}\n``` ");
        }

        

        messageList.Add("\n===END LOG===\n"); 

        if (channel != null) {
            foreach (string part in messageList) {
                await SendMessageAsync(channel, part)!;
            }
            
            
        }
        else {
            Console.WriteLine("Unable to Identify Channel.");
        }
        

        Console.WriteLine(string.Join("", messageList));
    }

    private static async Task SendMessageAsync(ITextChannel channel, string message, int maxMessageLength = 2000) {
        const string codeWrapper = "```";

        if (message.Length < maxMessageLength) {
            await channel.SendMessageAsync(message);
        }
        else {
            
            // check if the message contains code
            if (message.Contains(codeWrapper)) {
                
                // split the block on the code wrapper. 
                // alternating block of code and not code.
                List<string> blocks = message.Split(codeWrapper)
                    .ToList();
                
                // the first block can be either text or code. Let's make a list of methods that handle them 
                List<Func<ITextChannel, string, int, Task>> blockHandlers = [
                    SendMessageAsync,
                    SendCodeBlock
                ];
                
                // now a var to store the current index.
                int handleIndex = 0;
                
                // if the first block is null or empty then the code wrapper must be the first thing in 
                // the message.
                if (string.IsNullOrEmpty(blocks[1])) {
                    handleIndex = 1;
                }
                
                // loop though the blocks alternating type until you run out.
                while (blocks.Count != 0) {
                    
                    string block = blocks.pop(0);
                    
                    // this settles the case of ```<code> ``` ``` <code>```
                    if (!string.IsNullOrEmpty(block)) {
                        await blockHandlers[handleIndex].Invoke(channel, block, maxMessageLength);
                    }
                    
                    // flip the handle index
                    if (handleIndex == 1) {
                        handleIndex = 0;
                    }
                    else {
                        handleIndex = 1;
                    }
                }
            }
        }
    }

    private static async Task SendCodeBlock(ITextChannel channel, string code, int maxMessageLength) {
        await SendCodeBlock(channel, code, maxMessageLength, "```");
    }

    private static async Task SendCodeBlock(ITextChannel channel, string code, int maxMessageLength, string codeWrapper) {
        maxMessageLength -= (2 * codeWrapper.Length);

        if (code.Length > maxMessageLength) {
            // add lines until we hit or go over the max message length.
            string content = string.Empty;
            
            // let's break up the code into lines.
            List<string> lines = code.Split("\n")
                .ToList();
            
            while (lines.Count > 0) {
                // if one line puts us over, we need to break the line up more. 
                if (lines[0].Length > maxMessageLength && string.IsNullOrEmpty(content)) {
                    
                    string line = lines.pop(0);

                    List<string> sentences = line.Split(". ")
                        .ToList();

                    sentences = sentences.Select(s => s + ". ")
                        .ToList();
                    

                    while (sentences.Count > 0) {
                        // one sentence puts us over.
                        if (string.IsNullOrEmpty(content) && sentences[0].Length > maxMessageLength) {
                            string sentence = sentences.pop(0);

                            List<string> words = sentence.Split(" ")
                                .Select(s => s + " ")
                                .ToList();

                            while (words.Count > 0) {
                                // one word puts us over.
                                if (string.IsNullOrEmpty(content) && words[0].Length > maxMessageLength) {
                                    // fall back to the sentence level and print chars until the sentence is printed.
                                    while (!string.IsNullOrEmpty(sentence)) {
                                        content = sentence[..maxMessageLength];

                                        sentence = sentence[maxMessageLength..];
                                
                                        await SendMessageAsync(channel, $"{codeWrapper}{content}{codeWrapper}");
                                        content = "";
                                    }
                                }
                                
                                // adding one word would exceed limit, send.
                                else if (content.Length + words[0].Length > maxMessageLength) {
                                    await SendMessageAsync(channel, $"{codeWrapper}{content}{codeWrapper}");
                                    content = "";
                                }
                                
                                // add a word
                                else {
                                    content += words.pop(0);
                                }
                            }
                            
                            
                        }
                        // adding would exceed, send
                        else if (content.Length + sentences[0].Length > maxMessageLength) {
                            await SendMessageAsync(channel, $"{codeWrapper}{content}{codeWrapper}");
                            content = "";
                        }
                        
                        // add
                        else {
                            content = sentences.pop(0);
                        }
                    }
                }
                
                // if adding the next thing will push us over the limit, send and reset.
                else if (content.Length + lines[1].Length > maxMessageLength) {
                    await SendMessageAsync(channel, $"{codeWrapper}{content}{codeWrapper}");
                    content = "";
                }
                
                else {
                    content += "\n" + lines.pop(1);
                }
            }
        }
        else {
            await SendMessageAsync(channel, $"{codeWrapper}{code}{codeWrapper}");
        }
    }
    
    public static async Task LogAsync(LogSeverity severity, string message, Exception? exception = null,
        [CallerMemberName] string source = "<Unknown>") 
    {
        await LogInternalAsync(new LogMessage(severity, source, message, exception));

    }


    public static void SetSettingValue<T>(Dictionary<string, ISetting?> settings, string key, T newValue) {
        // Check if the key exists in the dictionary
        if (settings.TryGetValue(key, out ISetting? settingBase)) {
            // Attempt to cast to Setting<T>
            if (settingBase is Setting<T> setting) {
                // Update the Value property
                setting.Value = newValue;
            }
            else {
                throw new InvalidCastException(
                    $"Setting with key '{key}' is of type Setting<{settings[key]!.getType().Name}>.");
            }
        }
        else {
            throw new KeyNotFoundException($"Setting with key '{key}' not found.");
        }

        Program.SaveConfig();
    }

    public static T GetSettingValue<T>(Dictionary<string, ISetting?> settings, string key) {
        // Check if the key exists in the dictionary
        if (settings.TryGetValue(key, out ISetting? settingBase)) {
            // Attempt to cast to Setting<T>
            if (settingBase is Setting<T> setting) {
                return setting.Value; // Return the value if the cast is successful
            }
            else {
                throw new InvalidOperationException(
                    $"Setting with key '{key}' is not of type Setting<{typeof(T).Name}>.");
            }
        }
        else {
            throw new KeyNotFoundException($"Setting with key '{key}' not found.");
        }
    }

    public static string GrabError(string key) {
        try {
            var errors =
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(ErrorPath));
            if (errors != null && errors.ContainsKey(key) && errors[key].Any()) {
                return errors[key][new Random().Next(errors[key].Count)];
            }
            else {
                return key;
            }
        }
        catch (Exception e) {
            Console.WriteLine($"An unexpected error occurred: {e}");
            return key;
        }
    }
}

public static class ListUtil {
    public static T pop<T>(this List<T> list, int index = -1) {
        T value = list[index];
        list.RemoveAt(index);
        return value;
    }
}