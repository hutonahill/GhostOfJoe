
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
                string gameIdSql = @"
                    SELECT games_id 
                    FROM games 
                    WHERE title = @Title AND server_id = @ServerId;";

                SQLiteCommand gameIdCommand = connection.CreateCommand();
                gameIdCommand.CommandText = gameIdSql;
                
                gameIdCommand.Parameters.AddWithValue("@Title", gameTitle);
                gameIdCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                Object? game_id = gameIdCommand.ExecuteScalar();
                if (game_id == null) {
                    return null;
                } // Return empty if no game found
                

                // Get categories for the specified game
                string categoriesSql = @"
                    SELECT categories.categories_id, categories.name 
                    FROM categories
                    WHERE categories.games_id = @GameId;";

                SQLiteCommand categoriesCommand = connection.CreateCommand();

                categoriesCommand.CommandText = categoriesSql;
                
                categoriesCommand.Parameters.AddWithValue("@GameId", game_id);
                
                using (SQLiteDataReader reader = categoriesCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        int categoryId = reader.GetInt32(0);
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
        
        public static (bool exists, string? dataType) GetSettingDataType(string key) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                string sql = "SELECT name, type " +
                             "FROM pragma_table_info('servers') " +
                             "WHERE name = @Key;";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                command.Parameters.AddWithValue("@Key", key);

                using (var reader = command.ExecuteReader()) {
                    if (reader.HasRows) {
                        reader.Read();
                        return (true, reader["type"].ToString());
                    }
                }
                
            }
            return (false, null);
        }

        public static bool TryParseType(string dataType, string value, out object parsedValue) {
            parsedValue = null!;

            switch (dataType.ToLower()) {
                case "integer":
                    if (int.TryParse(value, out int intValue)) {
                        parsedValue = intValue;
                        return true;
                    }
                    break;
                case "real":
                    if (double.TryParse(value, out double doubleValue)) {
                        parsedValue = doubleValue;
                        return true;
                    }
                    break;
                case "text":
                    parsedValue = value;
                    return true;
                case "bit":
                    if (int.TryParse(value, out int bitValue) && (bitValue == 0 || bitValue == 1)) {
                        parsedValue = bitValue;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static bool UpdateSetting(string key, object value, SocketGuild serverId) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                string sql = $@"UPDATE servers 
                                SET @Key = @Value 
                                WHERE server_id = @ServerId;";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Value", value);
                command.Parameters.AddWithValue("@ServerId", serverId.Id);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
                
            }
        }
        
        public static bool AddGame(SocketGuild guild, string title) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // Check if the game already exists for the server
                string checkSql = @"
                    SELECT COUNT(*)
                    FROM games
                    WHERE title = @Title 
                        AND server_id = @ServerId";

                SQLiteCommand checkCommand = connection.CreateCommand();
                checkCommand.CommandText = checkSql;

                
                checkCommand.Parameters.AddWithValue("@Title", title);
                checkCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (count > 0) {
                    return false; // Game already exists
                }
                
                
                

                // Insert new game
                string insertSql = @"
                    INSERT INTO games (title, server_id)
                    VALUES 
                    (@Title, @ServerId)";

                SQLiteCommand insertCommand = connection.CreateCommand();
                insertCommand.CommandText = insertSql;

                
                insertCommand.Parameters.AddWithValue("@Title", title);
                insertCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                int result = insertCommand.ExecuteNonQuery();
                return result > 0; // Returns true if the insert was successful
                
            }
        }

        public static bool RemoveGame(SocketGuild guild, string title) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // Begin transaction
                using (var transaction = connection.BeginTransaction()) {
                    try {
                        // Delete categories associated with the game
                        string deleteCategoriesSql = @"
                            DELETE FROM categories
                            WHERE games_id = (
                                SELECT games_id FROM games
                                WHERE title = @Title AND server_id = @ServerId
                            )";

                        SQLiteCommand deleteCategoriesCommand = connection.CreateCommand();
                        deleteCategoriesCommand.CommandText = deleteCategoriesSql;
                        
                        deleteCategoriesCommand.Parameters.AddWithValue("@Title", title);
                        deleteCategoriesCommand.Parameters.AddWithValue("@ServerId", guild.Id);
                        deleteCategoriesCommand.ExecuteNonQuery();
                        
                        // Delete game
                        string deleteGameSql = @"
                            DELETE FROM games
                            WHERE title = @Title AND server_id = @ServerId";

                        SQLiteCommand deleteGameCommand = connection.CreateCommand();

                        deleteGameCommand.CommandText = deleteGameSql;

                        
                        deleteGameCommand.Parameters.AddWithValue("@Title", title);
                        deleteGameCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                        int result = deleteGameCommand.ExecuteNonQuery();

                        // Commit transaction if delete was successful
                        if (result > 0) {
                            transaction.Commit();
                            return true; // Returns true if the delete was successful
                        } else {
                            transaction.Rollback();
                            return false;
                        }
                        
                    } catch (Exception ex){
                        transaction.Rollback();
                        throw; // Rethrow exception if something goes wrong
                    }
                }
            }
        }
        
        public static bool AddCategory(SocketGuild guild, string gameTitle, string categoryName, string unit, bool higherBetter) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // Begin transaction
                using (var transaction = connection.BeginTransaction()) {
                    try {
                        // Get the game ID
                        string getGameIdSql = @"
                            SELECT games_id FROM games
                            WHERE title = @GameTitle 
                              AND server_id = @ServerId;";

                        int gameId;
                        using (var getGameIdCommand = new SQLiteCommand(getGameIdSql, connection)) {
                            getGameIdCommand.Parameters.AddWithValue("@GameTitle", gameTitle);
                            getGameIdCommand.Parameters.AddWithValue("@ServerId", guild.Id);

                            object? result = getGameIdCommand.ExecuteScalar();
                            if (result == null) {
                                throw new InvalidOperationException("Game not found.");
                            }
                            gameId = Convert.ToInt32(result);
                        }

                        // Add category
                        string addCategorySql = @"
                            INSERT INTO categories (games_id, name, unit, higherBetter)
                            VALUES 
                            (@GameId, @Name, @Unit, @HigherBetter);";

                        using (var addCategoryCommand = new SQLiteCommand(addCategorySql, connection)) {
                            addCategoryCommand.Parameters.AddWithValue("@GameId", gameId);
                            addCategoryCommand.Parameters.AddWithValue("@Name", categoryName);
                            addCategoryCommand.Parameters.AddWithValue("@Unit", unit);
                            addCategoryCommand.Parameters.AddWithValue("@HigherBetter", higherBetter ? 1 : 0);

                            int result = addCategoryCommand.ExecuteNonQuery();
                            
                            // Commit transaction if insert was successful
                            if (result > 0) {
                                transaction.Commit();
                                return true; // Returns true if the insert was successful
                            } else {
                                transaction.Rollback();
                                return false;
                            }
                        }
                    } catch {
                        transaction.Rollback();
                        throw; // Rethrow exception if something goes wrong
                    }
                }
            }
        }
        
        public static bool RemoveCategory(SocketGuild guild, string gameTitle, string categoryName) {
            using (var connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                // Begin transaction
                using (var transaction = connection.BeginTransaction()) {
                    try {
                        // Get the category ID
                        string getCategoryIdSql = @"
                            SELECT c.categories_id FROM categories c
                            JOIN games g ON c.games_id = g.games_id
                            WHERE g.title = @GameTitle 
                              AND g.server_id = @ServerId 
                              AND c.name = @CategoryName;";

                        int categoryId;
                        using (var getCategoryIdCommand = new SQLiteCommand(getCategoryIdSql, connection)) {
                            getCategoryIdCommand.Parameters.AddWithValue("@GameTitle", gameTitle);
                            getCategoryIdCommand.Parameters.AddWithValue("@ServerId", guild.Id);
                            getCategoryIdCommand.Parameters.AddWithValue("@CategoryName", categoryName);

                            var result = getCategoryIdCommand.ExecuteScalar();
                            if (result == null) {
                                throw new InvalidOperationException("Category not found.");
                            }
                            categoryId = Convert.ToInt32(result);
                        }

                        // Remove category
                        string removeCategorySql = @"
                            DELETE FROM categories
                            WHERE categories_id = @CategoryId";

                        using (var removeCategoryCommand = new SQLiteCommand(removeCategorySql, connection)) {
                            removeCategoryCommand.Parameters.AddWithValue("@CategoryId", categoryId);

                            int result = removeCategoryCommand.ExecuteNonQuery();
                            
                            // Commit transaction if delete was successful
                            if (result > 0) {
                                transaction.Commit();
                                return true; // Returns true if the delete was successful
                            } else {
                                transaction.Rollback();
                                return false;
                            }
                        }
                    } catch {
                        transaction.Rollback();
                        throw; // Rethrow exception if something goes wrong
                    }
                }
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
