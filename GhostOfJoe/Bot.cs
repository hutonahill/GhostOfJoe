using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        string ErrorMsg = "## Log: ";

        if (!string.IsNullOrEmpty(log.Source)) {
            ErrorMsg += $"\n### Source \n\t`{log.Source}` ";
        }

        if (log.Message != "") {
            ErrorMsg += $"\n ### Log Message: \n``` {log.Message} ``` ";
        }

        if (log.Exception != null) {
            ErrorMsg += $"\n### Exception Type: \n`{log.Exception.GetType().Name} ` " +
                        $"\n### Exception Message: \n```\n{log.Exception.Message}\n``` " +
                        $"\n### Callstack: \n```\n{log.Exception.StackTrace}\n``` ";
        }

        if (log.Severity == LogSeverity.Critical) {
            SocketUser admin = _client.GetUser(config.AdminUser);

           ErrorMsg = $"{admin.Mention}: \n{ErrorMsg}";
        }


        Debug.Assert(channel != null, nameof(channel) + " != null");
        await SendMessageAsync(channel, ErrorMsg)!;

        Console.WriteLine(ErrorMsg);
    }

    private static async Task SendMessageAsync(ITextChannel channel, string message) {
        const int maxMessageLength = 2000;
        
        for (int i = 0; i < message.Length; i += maxMessageLength) {
            string chunk = message.Substring(i, Math.Min(maxMessageLength, message.Length - i));
            await channel.SendMessageAsync(chunk);
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