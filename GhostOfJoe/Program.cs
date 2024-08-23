

using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;



using Discord;



namespace GhostOfJoe {
    class Program {

        private static readonly string configPath = "config.json";

        private static readonly string ErrorPath = "Errors.json";

        private static Random rand = new Random();
        
        public static Config? config;
        
        public  static readonly List<ulong> AdminServers = new List<ulong> { 859184385889796096, 573289805472071680 };
        
        private static JsonSerializerSettings? JsonSettings;
        

        

        private static IConfiguration _configuration;
        private static DiscordSocketClient _client;
        
        //TODO need to securely import this value.
        private static string DISCORD_KEY = "<API KEY>";
        
        // a logging channel on my personal discord
        private static readonly ulong LoggingChannel = 1269384832064950384;
        
        // my user_id
        private static readonly ulong AdminUser = 168496575369183232;
        
        
        private static readonly DiscordSocketConfig _socketConfig = new() {
            GatewayIntents =  GatewayIntents.GuildMembers | GatewayIntents.MessageContent | GatewayIntents.Guilds,
            AlwaysDownloadUsers = true,
        };

        private static readonly InteractionServiceConfig _interactionServiceConfig = new() {
            LocalizationManager = new ResxLocalizationManager("InteractionFramework.Resources.CommandLocales", Assembly.GetEntryAssembly(),
                new CultureInfo("en-US")),
            ThrowOnError = true,
            UseCompiledLambda = true
        };

        private static InteractionService _interactionService;
        
        public static async Task Main(string[] args) {

            DISCORD_KEY = Environment.GetEnvironmentVariable("API_KEY") ?? throw new InvalidOperationException();
            
            JsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented, // For readable output
                TypeNameHandling = TypeNameHandling.None, // Optional: Specify if you need type name handling
                Converters = new List<JsonConverter> { new SettingBaseConverter() } // Register the custom converter
            };
            
            // THESE TWO LINES WIPE ALL DATA
            config = new Config();
            SaveConfig();
            
            LoadConfig();
            
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "DC_")
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            
            _client = new DiscordSocketClient(_socketConfig);
            
            _client.Log += LogAsync;
            _client.Ready += ClientReadyAsync;
            _client.JoinedGuild += HandleGuildJoined;
            _client.InteractionCreated += HandleInteractionCreated;

            _client.AutocompleteExecuted += HandleAutocompleteExecution;
            

            // Bot token can be provided from the Configuration object we set up earlier
            await _client.LoginAsync(TokenType.Bot, DISCORD_KEY);
            await _client.StartAsync();

            

            await Task.Delay(Timeout.Infinite);
        }
        
        private static async Task HandleAutocompleteExecution(SocketAutocompleteInteraction arg){
            var context = new InteractionContext(_client, arg, arg.Channel);
            await _interactionService.ExecuteCommandAsync(context, null);
        }
        
        private static async Task HandleSlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3) {
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
                    default:
                        break;
                }
            }
        }
        
        private static async Task HandleInteractionCreated(SocketInteraction interaction) {
            try {
                SocketInteractionContext ctx = new(_client, interaction);
                await _interactionService.ExecuteCommandAsync(ctx, null);
            }
            catch {
                if (interaction.Type == InteractionType.ApplicationCommand) {
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }
        
        private static async Task HandleGuildJoined(SocketGuild guild) {
            ulong joeID = 238096751607676928;
        
            // add an entry
            DataHandler.AddServer(guild);
        
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
                    await LogAsync(new LogMessage(LogSeverity.Info, "HandleGuildJoined", 
                        "Default channel is not a text channel or does not exist."));
                }
            }
        }
        
        private static async Task ClientReadyAsync() {
            
            _interactionService = new InteractionService(_client, new InteractionServiceConfig
            {
                UseCompiledLambda = true,
                ThrowOnError = true
            });
            
            _interactionService.SlashCommandExecuted += HandleSlashCommandExecuted;
            
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await _interactionService.RegisterCommandsGloballyAsync();
            
            Console.WriteLine($"Logged in as {_client.CurrentUser.Username} - {_client.CurrentUser.Id}");
        }
        
        public static async Task LogAsync(LogMessage log) {
            

            ITextChannel? channel = _client.GetChannel(LoggingChannel) as ITextChannel;

            string ErrorMsg = "## Log: ";

            if (log.Message != "") {
                ErrorMsg += $"\n ### Log Message: \n` {log.Message} `";
            }

            if (log.Exception != null) {
                ErrorMsg += $"\n### Exception Type: \n`{log.Exception.GetType().Name} ` " +
                            $"\n### Exception Message: \n```\n{log.Exception.Message}\n```" +
                            $"\n### Callstack: \n```\n{log.Exception.StackTrace}\n```";
            }
            
            if (log.Severity == LogSeverity.Critical) {
                SocketUser admin = _client.GetUser(AdminUser);
            
                await channel?.SendMessageAsync($"{admin.Mention}: \n{ErrorMsg}")!;
            }
            else {
                await channel?.SendMessageAsync(ErrorMsg)!;
            }
            
            Console.WriteLine(ErrorMsg);
        }
        
        
        public static void SetSettingValue<T>(Dictionary<string, SettingBase?> settings, string key, T newValue) {
            // Check if the key exists in the dictionary
            if (settings.TryGetValue(key, out SettingBase? settingBase)) {
                // Attempt to cast to Setting<T>
                if (settingBase is Setting<T> setting) {
                    // Update the Value property
                    setting.Value = newValue;
                }
                else {
                    throw new InvalidCastException($"Setting with key '{key}' is of type Setting<{settings[key]!.getType().Name}>.");
                }
            }
            else {
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");
            }
            
            SaveConfig();
        }

        public static T GetSettingValue<T>(Dictionary<string, SettingBase?> settings, string key) {
            // Check if the key exists in the dictionary
            if (settings.TryGetValue(key, out SettingBase? settingBase)){
                // Attempt to cast to Setting<T>
                if (settingBase is Setting<T> setting){
                    return setting.Value; // Return the value if the cast is successful
                }
                else{
                    throw new InvalidOperationException($"Setting with key '{key}' is not of type Setting<{typeof(T).Name}>.");
                }
            }
            else{
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");
            }
        }
        
        public static string GrabError(string key) {
            try {
                var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(ErrorPath));
                if (errors != null && errors.ContainsKey(key) && errors[key].Any()) {
                    return errors[key][new Random().Next(errors[key].Count)];
                }
                else {
                    return key;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An unexpected error occurred: {e}");
                return key;
            }
        }
        
        public static string ApplyTitle(SocketGuildUser member) {
            
            string[] titles = DataHandler.GetTitlesForUser(member);

            if (titles.Length == 0) {
                return member.Mention;
            }

            string randomTitle = titles[rand.Next(titles.Length)];
            return randomTitle.Replace("<username>", member.Mention);
        }
        
        

        
        private static void LoadConfig() {
            // Check if the config file exists
            if (!File.Exists(configPath)) {
                // If it doesn't exist, create a default config object and save it
                Program.config = new Config(); // Initialize with default values if necessary
                SaveConfig(); // Create the file with default settings
            }
            else {
                // If it exists, read the config file
                string configJson = File.ReadAllText(configPath);
    
                // Deserialize the JSON into the Config object
                config = JsonConvert.DeserializeObject<Config>(configJson, JsonSettings);
            }
        }

        private static void SaveConfig() {
            // Serialize the config object to JSON
            string configJson = JsonConvert.SerializeObject(Program.config, JsonSettings);

            // Write the JSON to the config file
            File.WriteAllText(configPath, configJson);
        }
    }
    
    

    

    public class Config {

        public Dictionary<string, SettingBase?> GlobalSettings { get; set; }

        public Config() {
            GlobalSettings = new Dictionary<string, SettingBase?> {
                { "NsfwFlow", new Setting<bool>("Flow is considered NSFW", true) },
                { "ScoreRoundsTo", new Setting<uint>("Round scores to this place", 10) }
            };
        }
    }
    
}
