using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GhostOfJoe {
    class Program {
        private static DiscordSocketClient _client;
        private static CommandService _commands;
        private static IServiceProvider _services;
        public static Config config;
        
        public  static readonly List<ulong> AdminServers = new List<ulong> { 859184385889796096, 573289805472071680 };
        public static readonly ulong LoggingChannel = 1269384832064950384;
        public static readonly ulong AdminUser = 168496575369183232;
        
        private static string configPath = "config.json";

        private static string ErrorPath = "Errors.json";

        static async Task Main(string[] args) {
            
            LoadConfig();
            
            await RunBotAsync();
        }
        
        public static string ApplyTitle(SocketGuildUser member, ulong serverID) {
            Dictionary<string, string> titles = config.ServerData[serverID].Titles;
            string username = member.ToString();

            if (titles.TryGetValue(username, out string? title)) {
                return title.Replace("<username>", member.Mention);
            }
            else {
                return $"{member.Mention}";
            }
        }

        public static string GrabError(string key) {
            try {
                var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(ErrorPath));
                if (errors.ContainsKey(key) && errors[key].Any()) {
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
        
        public static async Task RunBotAsync(){
            _client = new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 100 });
            _commands = new CommandService();
            _services = new ServiceCollection().BuildServiceProvider();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += HandleCommandAsync;

            LoadConfig();

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task LogAsync(LogMessage log) {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }
        

        private static Task ReadyAsync() {
            Console.WriteLine($"Logged in as {_client.CurrentUser.Username} - {_client.CurrentUser.Id}");
            return Task.CompletedTask;
        }

        private static void LoadConfig() {
            string configJson = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<Config>(configJson);
        }

        public static void SaveConfig() {
            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, configJson);
        }

        private static async Task RegisterCommandsAsync() {
            await _commands.AddModulesAsync(AppDomain.CurrentDomain.GetAssemblies(), _services);
        }
        
        public static void SetSettingValue<T>(Dictionary<string, SettingBase> settings, string key, T newValue) {
            // Check if the key exists in the dictionary
            if (settings.TryGetValue(key, out SettingBase settingBase)) {
                // Attempt to cast to Setting<T>
                if (settingBase is Setting<T> setting) {
                    // Update the Value property
                    setting.Value = (T)newValue;
                }
                else {
                    Console.WriteLine($"Setting with key '{key}' is not of type Setting<{settings[key].getType().Name}>.");
                }
            }
            else {
                Console.WriteLine($"Setting with key '{key}' not found.");
            }
        }

        public static T GetSettingValue<T>(Dictionary<string, SettingBase> settings, string key) {
            // Check if the key exists in the dictionary
            if (settings.TryGetValue(key, out SettingBase settingBase))
            {
                // Attempt to cast to Setting<T>
                if (settingBase is Setting<T> setting)
                {
                    return setting.Value; // Return the value if the cast is successful
                }
                else
                {
                    throw new InvalidOperationException($"Setting with key '{key}' is not of type Setting<{typeof(T).Name}>.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");
            }
        }
        
        
        private static async Task HandleCommandAsync(SocketMessage messageParam){
            if (!(messageParam is SocketUserMessage message)) return;

            var context = new SocketCommandContext(_client, message);

            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix(config.Prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }

    public class CommandModule : ModuleBase<SocketCommandContext> {
        [Command("ping")]
        public async Task PingAsync() {
            var member = Context.User as SocketGuildUser;
            await ReplyAsync(Program.ApplyTitle(member));
        }

        [Command("grant_title")]
        public async Task GrantTitleAsync(SocketGuildUser member, [Remainder] string title) {
            var username = member.ToString();
            Program.config.ServerData[Context.Guild.Id].Titles[username] = title;
            Program.SaveConfig();
            await ReplyAsync($"{member.Mention} shall hereby be referred to as \"{Program.ApplyTitle(member)}\"");
        }

        [Command("flow")]
        public async Task FlowAsync([Remainder] string userMessage) {

            ulong targetChannelId = Context.Channel.Id;
            
            ITextChannel channel = Context.Client.GetChannel(targetChannelId) as ITextChannel;
            
            if (targetChannelId != null && Program.config.GlobalSettings[] != channel.IsNsfw){
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
            Dictionary<string, SettingBase> settings = Program.config.GlobalSettings;

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
                Program.SetSettingValue(Program.config.GlobalSettings, key, value);
                Program.SaveConfig();
                return ($"Setting '{key}' has been updated to '{value}'.");
            }
            else {
                return ($"Setting '{key}' does not exist.");
            }
        }
        
        [Command("get_settings")]
        public async Task getSettingsAsync() {
            
            Dictionary<string, SettingBase> targetSettings = Program.config.ServerData[Context.Guild.Id].ServerSettings
            
            await ReplyAsync($"## Current Local Settings:\n{getSettings(targetSettings)}");
        }

        private string getSettings(Dictionary<string, SettingBase> settings) {
            List<string> settingList = new List<string>();
            
            foreach (string key in settings.Keys) {
                settingList.Add($"- **{key}** \n {settings[key]}");
            }

            return string.Join("\n", settingList);
        }
    }

    public class Config {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public Dictionary<ulong, Data> ServerData { get; set; }

        public Dictionary<string, SettingBase> GlobalSettings { get; set; } = new Dictionary<string, SettingBase> {
            { "NsfwFlow", new Setting<bool>("Flow is considered NSFW", true) }
        };
    }

    public class Data {
        public Dictionary<string, string> Titles { get; set; }

        public Dictionary<string, SettingBase> ServerSettings { get; set; } = new Dictionary<string, SettingBase> {
            { "SafeFlow", new Setting<bool>("Spoiler excepts from the Book of Flow", true) }
        };
    }

    

    public abstract class SettingBase {
        public string Description { get; set; }
        
        public abstract Type getType();
    }

    public class Setting<T> : SettingBase {
        public T Value { get; set; }

        public override string ToString() {
            return $"\t- {Description}\n" +
                   $"\t- Type: '{typeof(T)}' \n" +
                   $"\t- Value: '{Value}'";
        }

        

        public override Type getType() {
            return typeof(T);
        }
        
        public Setting(string description, T value) {
            Description = description;
            Value = value;
        }
    }
}
