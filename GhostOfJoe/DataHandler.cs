using System.Data.SQLite;
using Discord;
using Discord.WebSocket;

namespace GhostOfJoe;

public static class DataHandler {
    
    
    //TODO this should be imported from an external file. or point to the same folder as the exe.
    private const string DatabasePath = "C:\\Users\\evanriker\\Desktop\\GhostOfJoe\\hostOfJoe\\GhostOfJoe\\bin\\ServerData.db";
    
    private const string ServerDataConnection = $"Data Source={DatabasePath};Version=3;";
    
    public static async Task<List<string>> GetGames(ulong guildID) {
        string FilePath = "";
        var games = new List<string>();
        
        try {
            using (SQLiteConnection connection = new SQLiteConnection(ServerDataConnection)) {
                connection.Open();

                FilePath = connection.FileName;

                string sql = @"SELECT title 
                           FROM games
                           WHERE server_id = @serverId";

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = sql;

                command.Parameters.AddWithValue("@serverId", guildID);

                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        games.Add(reader.GetString(0));
                    }
                }
            }
        }
        catch (SQLiteException ex) {
            await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(GetGames), FilePath));
            throw;
        }
        return games;
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
        
    public static string[] GetTitlesForUser(SocketGuildUser member) {
        var titles = new List<string>();
        
        // Connect to the database
        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();
            
            string sql = @"
            SELECT titles.title
            FROM titles 
            JOIN users  ON users.user_id = titles.user_id
            WHERE users.discordUser_id = @UserId";
            
            SQLiteCommand command = connection.CreateCommand(sql);
            
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
    
    public static async Task AddTitle(SocketGuildUser member, string title) {
        await using (var connection = new SQLiteConnection(ServerDataConnection)) {
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

                SQLiteCommand insertCommand = connection.CreateCommand(insertUserSql);
                
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
                int numRowsChanged = insertTitleCommand.ExecuteNonQuery();

                if (numRowsChanged != 1) {
                    await Program.LogAsync(new LogMessage(LogSeverity.Critical, nameof(AddTitle),
                        $"An insert of a single title effected more than one row." +
                        $"Attempting to add the title`{title}` to `{member.Username}`(`{member.Id}`) on the Table " +
                        $"`{member.Guild.Name}`(`{member.Guild.Id}`)"));
                }
            }
        }
    }

    
    
    public static Dictionary<string, string> GetServerSettings(SocketGuild guild) {
        var settings = new Dictionary<string, string>();

        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            // Query to get Table settings
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
            
            // Query to insert a new Table
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

            // Check if the game already exists for the Table
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
                        WHERE game_id = (
                            SELECT game_id FROM games
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
                    
                } catch (Exception){
                    transaction.Rollback();
                    throw; // Rethrow exception if something goes wrong
                }
            }
        }
    }
    
    public static bool AddCategory(ulong guildId, string gameTitle, string categoryName, string unit, bool higherBetter) {
        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            // Begin transaction
            using (var transaction = connection.BeginTransaction()) {
                try {
                    // Get the game ID
                    string getGameIdSql = @"
                        SELECT game_id FROM games
                        WHERE title = @GameTitle 
                          AND server_id = @ServerId;";
                    
                    SQLiteCommand getGameIdCommand = connection.CreateCommand(getGameIdSql);
                    
                    
                    getGameIdCommand.Parameters.AddWithValue("@GameTitle", gameTitle);
                    getGameIdCommand.Parameters.AddWithValue("@ServerId", guildId);
                    
                    object? result = getGameIdCommand.ExecuteScalar();
                    if (result == null) {
                        throw new InvalidOperationException("Game not found.");
                    }
                    int gameId = Convert.ToInt32(result);
                    

                    // Add category
                    string addCategorySql = @"
                        INSERT INTO categories (game_id, name, unit, higherBetter)
                        VALUES 
                        (@GameId, @Name, @Unit, @HigherBetter);";

                    using (var addCategoryCommand = new SQLiteCommand(addCategorySql, connection)) {
                        addCategoryCommand.Parameters.AddWithValue("@GameId", gameId);
                        addCategoryCommand.Parameters.AddWithValue("@Name", categoryName);
                        addCategoryCommand.Parameters.AddWithValue("@Unit", unit);
                        addCategoryCommand.Parameters.AddWithValue("@HigherBetter", higherBetter ? 1 : 0);

                        int AddResult = addCategoryCommand.ExecuteNonQuery();
                        
                        // Commit transaction if insert was successful
                        if (AddResult > 0) {
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
    
    
    public static async Task<List<string>?> GetCategoriesWithScores(IUser member, IGuild guild, string gameTitle) {
        var result = new List<string>();

        await AddMember(guild, member);

        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            // Get the game ID for the specified title
            string gameIdSql = @"
                SELECT game_id 
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
                SELECT categories.category_id, categories.name 
                FROM categories
                WHERE categories.game_id = @GameId;";

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
                        WHERE scores.category_id = @CategoryId 
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
    
    public static async Task<List<string>?> GetCategoriesWithScores(IGuildUser member, string gameTitle) {
        return await GetCategoriesWithScores(member, member.Guild, gameTitle);
    }
    
    public static List<string>? GetCategories(IGuild guild, string gameTitle) {
        var result = new List<string>();

        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            // Get the game ID for the specified title
            string gameIdSql = @"
                SELECT game_id 
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
                SELECT categories.category_id, categories.name 
                FROM categories
                WHERE categories.game_id = @GameId;";

            SQLiteCommand categoriesCommand = connection.CreateCommand();

            categoriesCommand.CommandText = categoriesSql;
            
            categoriesCommand.Parameters.AddWithValue("@GameId", game_id);
            
            using (SQLiteDataReader reader = categoriesCommand.ExecuteReader()) {
                while (reader.Read()) {
                    string categoryName = reader.GetString(1);
                    
                    result.Add(categoryName);
                }
            }
        }

        return result;
    }
    
    
    public static bool RemoveCategory(SocketGuild guild, string gameTitle, string categoryName) {
        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            // Begin transaction
            using (var transaction = connection.BeginTransaction()) {
                try {
                    // Get the category ID
                    string getCategoryIdSql = @"
                        SELECT c.category_id 
                        FROM categories c
                        JOIN games g ON c.game_id = g.game_id
                        WHERE g.title = @GameTitle 
                          AND g.server_id = @ServerId 
                          AND c.name = @CategoryName;";

                    int categoryId;
                    SQLiteCommand getCategoryIdCommand = connection.CreateCommand(getCategoryIdSql);
                    
                    
                    getCategoryIdCommand.Parameters.AddWithValue("@GameTitle", gameTitle);
                    getCategoryIdCommand.Parameters.AddWithValue("@ServerId", guild.Id);
                    getCategoryIdCommand.Parameters.AddWithValue("@CategoryName", categoryName);

                    var getResult = getCategoryIdCommand.ExecuteScalar();
                    if (getResult == null) {
                        throw new InvalidOperationException("Category not found.");
                    }
                    categoryId = Convert.ToInt32(getResult);
                    

                    // Remove category
                    string removeCategorySql = @"
                        DELETE FROM categories
                        WHERE category_id = @CategoryId";

                    SQLiteCommand removeCategoryCommand = connection.CreateCommand(removeCategorySql);
                    
                    
                    removeCategoryCommand.Parameters.AddWithValue("@CategoryId", categoryId);

                    int RemoveResult = removeCategoryCommand.ExecuteNonQuery();
                    
                    // Commit transaction if delete was successful
                    if (RemoveResult > 0) {
                        transaction.Commit();
                        return true; // Returns true if the delete was successful
                    } else {
                        transaction.Rollback();
                        return false;
                    }
                    
                } catch {
                    transaction.Rollback(); 
                    throw; // Rethrow exception if something goes wrong
                }
            }
        }
    }

    public static async Task<bool> GetSafeFlow(IGuild guild) {
        bool output = true;
        
        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            string sql = @"SELECT safeFlow
                           FROM servers
                           WHERE server_id = @serverId";

            SQLiteCommand command = connection.CreateCommand(sql);

            command.Parameters.AddWithValue("@serverId", guild.Id);

            using (SQLiteDataReader result = command.ExecuteReader()) {
                int safeFlow = Convert.ToInt32(result);

                if (safeFlow == 1) {
                    output = true;
                }
                else if (safeFlow == 0) {
                    output = false;
                }
                else {
                    await Program.LogAsync(new LogMessage(LogSeverity.Critical, nameof(GetSafeFlow),
                        $"so... um your SQLite trigger didnt work. The safeFlow value for the guild " +
                        $"{guild.Name} ({guild.Id}) is {safeFlow}, not 0 or 1."));
                }
            }
        }

        return output;
    }

    public static async Task AddMember(IGuildUser member) {
        
        await AddMember(member.Guild, member);
    }

    public static async Task AddMember(IGuild guild, IUser member) {
        using (var connection = new SQLiteConnection(ServerDataConnection)) {
            connection.Open();

            string hasMemberSql = @"SELECT COUNT(*)
                                    FROM users
                                    WHERE server_id = @guildId
                                        AND discordUser_id = @memberId;";
            

            SQLiteCommand hasMemberCommand = connection.CreateCommand(hasMemberSql);

            hasMemberCommand.Parameters.AddWithValue("@guildId", guild.Id);
            hasMemberCommand.Parameters.AddWithValue("@memberId", member.Id);

            object? hasMemberResult = hasMemberCommand.ExecuteScalar();

            int numUsers = Convert.ToInt32(hasMemberResult);

            
            if (numUsers == 0) {
                string insertSql = @"INSERT INTO users (discordUser_id, server_id) 
                                   VALUES 
                                   (@memberId, @guildId);";

                SQLiteCommand insertCommand = connection.CreateCommand(insertSql);

                insertCommand.Parameters.AddWithValue("@memberId", member.Id);
                insertCommand.Parameters.AddWithValue("@guildId", guild.Id);

                object reader = insertCommand.ExecuteNonQuery();

                int numEffected = Convert.ToInt32(reader);

                if (numEffected != 1) {
                    await Program.LogAsync(new LogMessage(LogSeverity.Critical, nameof(AddMember),
                        $"Inserting a new user effected more than one row. Server: `{guild.Name}`(`{guild.Id}`) " +
                        $"Member: `{member.Username}`(`{member.Id}`)"));
                }
            }
            else if (numUsers != 1) {
                await Program.LogAsync(new LogMessage(LogSeverity.Critical, nameof(AddMember),
                    $"Duplicate user found. Server `{guild.Name}`(`{guild.Id}`) contains {numUsers} references to " +
                    $"The user `{member.Username}`(`{member.Id}`)"));
            }

        }
    }
    
    private static SQLiteCommand CreateCommand(this SQLiteConnection connection, string sql) {
        SQLiteCommand output = connection.CreateCommand();
        output.CommandText = sql;

        return output;
    }
}