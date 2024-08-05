
using System.Data.SQLite;
using System.Diagnostics;
using Discord.WebSocket;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GhostOfJoe {
    class Program {

        private static readonly string configPath = "config.json";

        private static readonly string ErrorPath = "Errors.json";

        private static Random rand = new Random();
        
        public static Config? config;
        
        public  static readonly List<ulong> AdminServers = new List<ulong> { 859184385889796096, 573289805472071680 };
        
        private static JsonSerializerSettings? JsonSettings;
        
        private static void Main() => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync() {
            
            JsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented, // For readable output
                TypeNameHandling = TypeNameHandling.None, // Optional: Specify if you need type name handling
                Converters = new List<JsonConverter> { new SettingBaseConverter() } // Register the custom converter
            };
            
            // THESE TWO LINES WIPE ALL DATA
            config = new Config();
            SaveConfig();
            
            LoadConfig();

            var configuration = new ConfigurationBuilder()
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(options => {
                    options.ClearProviders();
                    options.AddConsole();
                })
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IBot, Bot>()
                .BuildServiceProvider();

            try {
                IBot bot = serviceProvider.GetRequiredService<IBot>();

                await bot.StartAsync(serviceProvider);

                do {
                    var keyInfo = Console.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Q) {
                        await bot.StopAsync();
                        return;
                    }
                } while (true);
            }
            catch (Exception exception) {
                Console.WriteLine(exception.Message);
                Environment.Exit(-1);
            }
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

        public static List<string> GetGames(SocketGuild guild) {
            var games = new List<string>();

            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                string sql = "SELECT title FROM games";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        games.Add(reader.GetString(0));
                    }
                }
                
            }

            return games;
        }
        
        public static string ApplyTitle(SocketGuildUser member) {
            
            string[] titles = GetTitlesForUser(member);

            if (titles.Length == 0) {
                return member.Mention;
            }

            string randomTitle = titles[rand.Next(titles.Length)];
            return randomTitle.Replace("<username>", member.Mention);
        }
        
        public static bool RemoveTitle(SocketGuildUser member, string title) {
            try {
                using (var connection = new SQLiteConnection(ServerDataConnection)) {
                    connection.Open();

                    // Query to remove a title associated with a user
                    string sql = @"
                        DELETE FROM titles
                        WHERE title = @Title AND user_id = (
                            SELECT user_id FROM users WHERE discordUser_id = @DiscordUserId AND server_id = @ServerId
                        );";

                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = sql;
                    
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@DiscordUserId", member.Id);
                    command.Parameters.AddWithValue("@ServerId", member.Guild.Id);

                    command.ExecuteNonQuery();
                
                    // If rowsAffected is greater than 0, the title was removed
                    return true; 
                    
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can implement your logging here)
                Console.WriteLine($"Error removing title: {ex.Message}");
                return false; // An error occurred
            }
        }
        
        private static string[] GetTitlesForUser(SocketGuildUser member) {
            var titles = new List<string>();
            
            // Connect to the database
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();
                
                string sql = @"
                SELECT titles.title
                FROM titles 
                JOIN users  ON users.user_id = titles.user_id
                WHERE users.discordUser_id = @UserId";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                command.Parameters.AddWithValue("@UserId", member.Id);
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        titles.Add(reader.GetString(0));
                    }
                }
                
            }

            return titles.ToArray();
        }
        
        public static SocketGuildUser? GetMemberWithTitle(string title, SocketGuild guild) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                string sql = @"
                    SELECT users.discordUser_id
                    FROM titles
                    JOIN users  ON titles.user_id = users.user_id
                    WHERE titles.title = @Title AND users.server_id = @ServerId
                ";

                SQLiteCommand command = connection.CreateCommand();

                command.CommandText = sql;

                // add our paramiters
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@ServerId", guild.Id);

                var userId = command.ExecuteScalar() as long?; // Assuming user_id is of type long

                // If no user ID is found, return null
                if (userId == null) {
                    return null;
                }

                // Find and return the member with the given user ID
                return guild.GetUser((ulong)userId);
                
            }
        }
        
        public static void AddTitle(SocketGuildUser member, string title) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // First, check if the member already exists in the users table
                ulong discordUserId = member.Id;
                ulong serverId = member.Guild.Id;
                int userId;

                // Check if the user already has an entry in the users table
                string checkUserSql = @"
                    SELECT COUNT(*)
                    FROM users
                    WHERE discordUser_id = @DiscordUserId AND server_id = @ServerId";

                SQLiteCommand command = connection.CreateCommand();

                command.CommandText = checkUserSql;
                
                command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
                command.Parameters.AddWithValue("@ServerId", serverId);

                object? result = command.ExecuteScalar();

                // If the user does not exist, insert them into the users table
                if (result == null) {
                    string insertUserSql = @"
                    INSERT INTO users (discordUser_id, server_id)
                    VALUES (@DiscordUserId, @ServerId);
                    SELECT last_insert_rowid();";

                    SQLiteCommand insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = insertUserSql;

                    
                    insertCommand.Parameters.AddWithValue("@DiscordUserId", discordUserId);
                    insertCommand.Parameters.AddWithValue("@ServerId", serverId);
                    userId = (int)insertCommand.ExecuteScalar();
                    
                }
                else
                {
                    userId = (int)result;
                }
                

                // Now, add the title with the associated user_id
                string insertTitleSql = @"
                INSERT INTO titles (title, user_id)
                VALUES (@Title, @UserId)";

                using (var insertTitleCommand = new SQLiteCommand(insertTitleSql, connection))
                {
                    insertTitleCommand.Parameters.AddWithValue("@Title", title);
                    insertTitleCommand.Parameters.AddWithValue("@UserId", userId);
                    insertTitleCommand.ExecuteNonQuery();
                }
            }
        }

        public static List<string>? GetCategoriesWithScores(SocketUser member, SocketGuild guild, string gameTitle) {
            var result = new List<string>();

            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // Get the game ID for the specified title
                long gameId;
                string gameIdSql = @"SELECT games_id 
                                     FROM games 
                                     WHERE title = @Title AND server_id = @ServerId";

                SQLiteCommand gameIdCommand = connection.CreateCommand();
                gameIdCommand.CommandText = gameIdSql;
                
                gameIdCommand.Parameters.AddWithValue("@Title", gameTitle);
                gameIdCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                Object? gameIdResult = gameIdCommand.ExecuteScalar();
                if (gameIdResult == null) {
                    return null;
                } // Return empty if no game found

                gameId = (int)gameIdResult;
                

                // Get categories for the specified game
                string categoriesSql = @"
                    SELECT categories.categories_id, categories.name 
                    FROM categories
                    WHERE categories.games_id = @GameId";

                SQLiteCommand categoriesCommand = connection.CreateCommand();

                categoriesCommand.CommandText = categoriesSql;
                
                
                categoriesCommand.Parameters.AddWithValue("@GameId", gameId);
                using (var reader = categoriesCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        long categoryId = reader.GetInt64(0);
                        string categoryName = reader.GetString(1);

                        // Check if the user has a score for this category
                        string scoreSql = @"
                            SELECT scores.value
                            FROM scores 
                            JOIN users ON scores.user_id = users.user_id
                            WHERE scores.categorie_id = @CategoryId 
                                AND users.discordUser_id = @DiscordUserId 
                                AND users.server_id = @ServerId";

                        SQLiteCommand scoreCommand = connection.CreateCommand();
                        scoreCommand.CommandText = scoreSql;


                        scoreCommand.Parameters.AddWithValue("@CategoryId", categoryId);
                        scoreCommand.Parameters.AddWithValue("@DiscordUserId", member.Id);
                        scoreCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                        object? scoreResult = scoreCommand.ExecuteScalar();

                        string output;

                        // Format the result
                        if (scoreResult != null) {
                            output = ($"**{categoryName}** {member.Username}: {scoreResult}");
                        }
                        else {
                            output = $"**{categoryName}**";
                        }

                        result.Add(output);
                    }
                }
            }

            return result;
        }
        
        public static List<string>? GetCategoriesWithScores(SocketGuildUser member, string gameTitle) {
            return GetCategoriesWithScores(member, member.Guild, gameTitle);
        }
        
        public static Dictionary<string, string> GetServerSettings(SocketGuild guild) {
            var settings = new Dictionary<string, string>();

            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // Query to get server settings
                string sql = "SELECT * FROM servers WHERE server_id = @ServerId";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                command.Parameters.AddWithValue("@ServerId", guild.Id);

                using (var reader = command.ExecuteReader()) {
                    if (reader.Read()) {
                        // Populate the dictionary with column names and their corresponding values
                        for (int i = 0; i < reader.FieldCount; i++) {
                            string columnName = reader.GetName(i);
                            string value = $"{reader[i]}";
                            settings[columnName] = value;
                        }
                    }
                }
            }
            return settings;
        }
        
        public static void AddServer(SocketGuild guild) {
            using(var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();
                
                // Query to insert a new server
                string sql = @"
                    INSERT INTO servers (server_id, safeFlow)
                    VALUES (@ServerId, @SafeFlow)";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                command.Parameters.AddWithValue("@ServerId", guild.Id);
                command.Parameters.AddWithValue("@SafeFlow", 1); // Default value for safeFlow

                command.ExecuteNonQuery();
                
            }
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

        public static void SaveConfig() {
            // Serialize the config object to JSON
            string configJson = JsonConvert.SerializeObject(Program.config, JsonSettings);

            // Write the JSON to the config file
            File.WriteAllText(Program.configPath, configJson);
        }
        
        
        private const string ServerDataConnection = "Data Source=ServerData.db;Version=3;";
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
