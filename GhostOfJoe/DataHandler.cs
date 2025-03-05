using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using GhostOfJoe.Data;
using GhostOfJoe.Models;


namespace GhostOfJoe;

public static class DataHandler {
    
    /// <summary>
    /// Gets a users ID in the database.
    /// </summary>
    /// <param name="user">The User to find</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    /// <returns>The database userId or null if user is not in database.</returns>
    private static async Task<int?> getUserIdAsync(this IGuildUser user, ServerDataContext? context = null) {
        return await getUserIdAsync(user.Guild, user, context);
    }
    
    /// <summary>
    /// Gets a users ID in the database. Returns null if there is no such user.
    /// </summary>
    /// <param name="guild">The guild the user is in</param>
    /// <param name="user">The user to find</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    /// <returns>The database userId or null if user is not in database.</returns>
    private static async Task<int?> getUserIdAsync(IGuild guild, IUser user, ServerDataContext? context = null) {
        bool CreatedContext = false;
        if (context == null) {
            context = new ServerDataContext();
            CreatedContext = true;
        }

        int? result = null;
        
        List<int> userIdResult = context.Users
            .Where(u => u.server_id == guild.Id && u.discordUser_id == user.Id)
            .Select(u => u.user_id).ToList();

        if (await userIdResult.OnlyOneAsync($"Got more than one User for discordUserId == `{user.Id}` " +
                                       $"and server_id == `{guild.Id}`")) {
            result = userIdResult.First();
        }
        
        
        if (CreatedContext) {
            await context.DisposeAsync();
        }
        
        return result;
    }
    
    /// <summary>
    /// If a user does not exist within the database, add it to the database.
    /// </summary>
    /// <param name="guild">The server the user is a part of</param>
    /// <param name="user">The user to add</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    private static async Task AddUserAsync(this IGuild guild, IUser user, ServerDataContext? context = null) {
        bool CreatedContext = false;
        if (context == null) {
            context = new ServerDataContext();
            CreatedContext = true;
        }

        if (await getUserIdAsync(guild, user, context) == null) {
            Users newUser = new Users();

            newUser.discordUser_id = user.Id;
            newUser.server_id = guild.Id;
        
            context.Users.Add(newUser);
            await context.SaveChangesAsync();
        }
        
        
        if (CreatedContext) {
            await context.DisposeAsync();
        }
    }
    
    /// <summary>
    /// If a user does not exist within the database, add it to the database.
    /// </summary>
    /// <param name="member">The member to add</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    private static async Task AddUserAsync(this IGuildUser member, ServerDataContext? context = null) {
        await member.Guild.AddUserAsync(member, context);
    }
    
    /// <summary>
    /// Verifies that a list contains only one value. Used when selecting a single value out of a database.
    ///
    /// Will warn administrators if there is more than one value in the list.
    /// </summary>
    /// <param name="list">The list to be checked</param>
    /// <param name="message">A customizable message to send to the admins</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Returns true if the list contains a single item.</returns>
    private static async Task<bool> OnlyOneAsync<T>(this List<T> list, string? message = null) {
        if (list.Count == 0) {
            return false;
        }
        else if (list.Count > 1) {
            message ??= $"List containing `{typeof(T).FullName}` was supposed to contain 1 or 0 items. It contained {list.Count}." +
                        $"\n ```{list}```";

            await Bot.LogAsync(LogSeverity.Critical, message);
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Gets a game from a server
    /// </summary>
    /// <param name="guild">The server with the game</param>
    /// <param name="title">The title of the game.</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    /// <returns>the game or null if none was found</returns>
    private static async Task<Games?> GetGame(this IGuild guild, string title, ServerDataContext? context = null) {
        // handle the optional context
        bool CreatedContext = false;
        if (context == null) {
            context = new ServerDataContext();
            CreatedContext = true;
        }
        
        // create an output var.
        Games? output = null;
        
        // try to get the game
        List<Games> gamesList = context.Games
            .Where(g => g.title == title && g.server_id == guild.Id)
            .ToList();
        
        
        // if only one result returned, send it to the user.
        if (await gamesList.OnlyOneAsync($"Found multiple Games with the title `{title} on server {guild.Name}(`")) {
            output = gamesList.First();
        }
        
        
        // if we created the context, dispose of it.
        if (CreatedContext) {
            await context.DisposeAsync();
        }
        
        return output;
    }

    /// <summary>
    /// Returns a category given a game and a category name
    /// </summary>
    /// <param name="game">Game that contains the category</param>
    /// <param name="name">The name of the category</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    /// <returns>The category or null if none was found</returns>
    private static async Task<Categories?> GetCategory(this Games game, string name, ServerDataContext? context = null) {
        // handle the optional context
        bool CreatedContext = false;
        
        if (context == null) {
            context = new ServerDataContext();
            CreatedContext = true;
        }

        Categories? output = null;

        List<Categories> categoriesList = context.Categories
            .Where(c => c.game == game && c.name == name)
            .ToList();

        if (await categoriesList.OnlyOneAsync(
                $"Found a duplicate category `{name}` for the game `{game.title}`(`{game.game_id}`).")) {
            output = categoriesList.First();
        }
            
        // if we created the context, dispose of it.
        if (CreatedContext) {
            await context.DisposeAsync();
        }

        return output;
    }

    /// <summary>
    /// gets a user from the database
    /// </summary>
    /// <param name="guild">The guild the target user is a member of</param>
    /// <param name="user">The user to find.</param>
    /// <param name="context">Optional, Lets you avoid creating a new instance of context.</param>
    /// <returns>The user object or null if none wer found.</returns>
    private static async Task<Users?> GetDbUserAsync(this IGuild guild, IUser user, ServerDataContext? context = null) {
        // handle the optional context
        bool CreatedContext = false;
        
        if (context == null) {
            context = new ServerDataContext();
            CreatedContext = true;
        }

        Users? output = null;
        List<Users> usersList = context.Users
                .Where(u => u.discordUser_id == user.Id && u.server_id == guild.Id)
                .ToList();

        if (await usersList.OnlyOneAsync($"Found a duplicate user on the server {guild.Name} ")) {
            output = usersList.First();
        }
        
        
        // if we created the context, dispose of it.
        if (CreatedContext) {
            await context.DisposeAsync();
        }

        return output;
    }
    
    /// <summary>
    /// Returns a list of games that are registerd to a guild
    /// </summary>
    /// <param name="guild">The target guild</param>
    /// <returns>a list of the game titles</returns>
    public static async Task<List<string>> GetGames(this IGuild guild) {
        List<string> games;
        
        try {
            await using ServerDataContext context = new ServerDataContext();
            games = context.Games
                .Where(g => g.server_id == guild.Id)
                .Select(g => g.title)
                .ToList();
        }
        catch (Exception ex) {
            await Bot.LogAsync(LogSeverity.Error,  ex.Message, ex);
            throw;
        }
        
        return games;
    }
    
    /// <summary>
    /// Removes a title from a user.
    /// </summary>
    /// <param name="member">The member to target</param>
    /// <param name="title">The title to remove</param>
    /// <returns>Bool representing whether the operation was successful.</returns>
    public static async Task<bool> RemoveTitle(this IGuildUser member, string title) {
            try {

                await using ServerDataContext context = new ServerDataContext();

                // get the database UserId
                int? userId = await member.getUserIdAsync( context);
                
                // if the id is null, the user doesn't exist
                if (userId == null) {
                    return false;
                }

                List<Titles> TitleToDelete = context.Titles.Where(t => t.title == title && t.user_id == userId).ToList();


                if (!await TitleToDelete.OnlyOneAsync($"Duplicate Titles detected for title `{title}`!")) {
                    return false;
                }
                
                Titles titleToDelete = TitleToDelete.First();
                context.Titles.Remove(titleToDelete);
                await context.SaveChangesAsync();

                return true;
                
            }
            catch (Exception ex)
            {
                await Bot.LogAsync(LogSeverity.Error, ex.Message, ex);
                throw;
            }
    }
    
    /// <summary>
    /// mentions the user while applying any titles the user has.
    /// </summary>
    /// <param name="member">The member you want to mention</param>
    /// <returns>A mention of the user.</returns>
    public static async Task<string> ApplyTitle(this IGuildUser member) {

        string[] titles = await member.GetTitles();

        if (titles.Length == 0) {
            return member.Mention;
        }

        string randomTitle = titles[new Random().Next(titles.Length)];
        return randomTitle.Replace("<username>", member.Mention);
    }
    
    /// <summary>
    ///  returns a list of titles the member has
    /// </summary>
    /// <param name="member">The member whose titles you want.</param>
    /// <returns>an array of the user's titles</returns>
    public static async Task<string[]> GetTitles(this IGuildUser member) {
        
        using ServerDataContext context = new ServerDataContext();
        
        // get the database userId of the user
        int? databaseUserId = await member.getUserIdAsync(context);
        
        // get titles attached to them
        List<string> titles = context.Titles
            .Where(t => t.user_id == databaseUserId)
            .Select(t => t.title)
            .ToList();

        return titles.ToArray();
    }
    
    /// <summary>
    /// Gets the member of a guild that has a title.
    /// </summary>
    /// <param name="guild">The guild with the user and title</param>
    /// <param name="title">The title your looking for.</param>
    /// <returns>The member with the title or null if the title cannot be found.</returns>
    public static async Task<IGuildUser?> GetMemberWithTitle(this IGuild guild, string title) {

        ServerDataContext context = new ServerDataContext();

        List<Users> users = context.Titles.Where(t => t.title == title).Select(t => t.user).ToList();

        if (await users.OnlyOneAsync(
                $"{users.Count} users have the title `{title}` on server {guild.Name}(`{guild.Id}`)!")) 
        {
            ulong userId = users.First().discordUser_id;

            return await guild.GetUserAsync(userId);
        }

        return null;
    }
    
    
    /// <summary>
    /// Adds a title to a user
    /// </summary>
    /// <param name="member">The guild member to add the title to.</param>
    /// <param name="title">The title to add</param>
    /// <returns>wether the operaiton was a success.</returns>
    /// <exception cref="UnreachableException"></exception>
    public static async Task<bool> AddTitle(this IGuildUser member, string title) {
        await using ServerDataContext context = new ServerDataContext();

        await member.AddUserAsync();
        
        int databaseUserId = await member.getUserIdAsync(context) ?? throw new UnreachableException("We just added the user to the database, how are they not there?");
        

        if (await member.Guild.GetMemberWithTitle(title) == null) {
            Titles newTitle = new Titles {
                title = title,
                user_id = databaseUserId
            };

            context.Titles.Add(newTitle);
            await context.SaveChangesAsync();
            return true;
        }
        else {
            return false;
        }
    }

    public static void AddServer(this SocketGuild guild) {

        ServerDataContext context = new ServerDataContext();

        int numServers = context.Servers.Count(s => s.server_id == guild.Id);

        if (numServers == 0) {
            Servers newServer = new Servers {
                server_id = guild.Id
            };

            context.Add(newServer);

            context.SaveChanges();
        }
    }
    
    public static async Task<Dictionary<string, string>> GetSettingsAsync(this SocketGuild guild) {
        await using ServerDataContext context = new ServerDataContext();
        
        var settings = new Dictionary<string, string>();

        
        List<Servers> serverResults = context.Servers
            .Where(s => s.server_id == guild.Id).ToList();

        if (await serverResults.OnlyOneAsync($"Found more than one server in the database for server_id == `{guild.Id}`")) {

            Servers server = serverResults.First();
            
            
            // Use reflection to get property names and values dynamically
            PropertyInfo[] properties = typeof(Servers).GetProperties();
            
            
            foreach (PropertyInfo prop in properties) {
                string columnName = prop.Name;

                if (columnName != nameof(Servers.server_id)) {
                    string value = prop.GetValue(server)?.ToString() ?? string.Empty;
                    settings[columnName] = value;
                }
            }
        }

        return settings;
    }
    
    public static  Type? GetSettingDataType(string key) {
        PropertyInfo[] properties = typeof(Servers).GetProperties();

        foreach (PropertyInfo prop in properties) {
            string columnName = prop.Name;

            if (columnName != nameof(Servers.server_id) && columnName == key) {
                return prop.PropertyType;
            }
        }
        
        return null;
    }


    public static bool TryParseType(Type dataType, string value, out object parsedValue) {
        parsedValue = null!;

        if (dataType == typeof(int)) {
            if (int.TryParse(value, out int intValue)) {
                parsedValue = intValue;
                return true;
            }
        }
        else if (dataType == typeof(double)) {
            if (double.TryParse(value, out double doubleValue)) {
                parsedValue = doubleValue;
                return true;
            }
        }
        else if (dataType == typeof(string)) {
            parsedValue = value;
            return true;
        }
        else if (dataType == typeof(bool)) {
            if (int.TryParse(value, out int bitValue) && (bitValue == 0 || bitValue == 1)) {
                parsedValue = bitValue != 0; // Convert to bool
                return true;
            }
        }

        return false;
    }

    public static async Task<bool?> UpdateSettingAsync(this IGuild guild, string key, object value) {
        
        await using ServerDataContext context = new ServerDataContext();
        
        List<Servers> serverResults = context.Servers
            .Where(s => s.server_id == guild.Id).ToList();

        if (!await serverResults.OnlyOneAsync(
                $"Found more than one server in the database for server_id == `{guild.Id}`")) return null;
        
        Servers server = serverResults.First();
            
            
        // Use reflection to get property names and values dynamically
        PropertyInfo[] properties = typeof(Servers).GetProperties();
            
            
        foreach (PropertyInfo prop in properties) {
            string columnName = prop.Name;

            if (columnName != nameof(Servers.server_id) && columnName == key) {
                try {
                    prop.SetValue(server, value);

                    await context.SaveChangesAsync();
                    return true;
                }
                catch (Exception e) {
                    await Bot.LogAsync(LogSeverity.Warning,
                        $"Failed to change setting `{key}` for server {guild.Name}(`{guild.Id}`) to {value}");
                    return null;
                }
            }
        }
            
        // if we make it though the for loop without returning, then the system couldn't find the setting
        return false;

    }
    
    
    /// <summary>
    /// Adds a game to a server
    /// </summary>
    /// <param name="guild">The server to be modified</param>
    /// <param name="title">The title of the game to add</param>
    /// <param name="description">unused. a description to the game.</param>
    /// <returns>False if the game already exists, True if the game has been added, and null for an error.</returns>
    public static async Task<bool> AddGame(this IGuild guild, string title, string description = "") {
        await using ServerDataContext context = new ServerDataContext();
        
        // first we verify the game doesn't already exist.
        int numGames = context.Games.Count(g => g.title == title && g.server_id == guild.Id);
        
        // the game already exists
        if (numGames == 1) {
            return false;
        }
        
        // there is a duplicate freak out there should never be a duplicate.
        else if (numGames > 1) {
            await Bot.LogAsync(LogSeverity.Critical,
                $"Found a duplicate game `{title}` for server {guild.Name}(`{guild.Id}`)");
            return false;
        }

        // create a new game object
        Games newGame = new Games {
            server_id = guild.Id,
            title = title
        };
        
        
        // add the new game object to the database and save the changes.
        await context.Games.AddAsync(newGame);
        await context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Removes a game from a server
    /// </summary>
    /// <param name="guild">The guild with the game</param>
    /// <param name="title">The title of the game</param>
    /// <returns>False if it cant find the game, True on a sucess.</returns>
    public static async Task<bool> RemoveGameAsync(this IGuild guild, string title) {
        await using ServerDataContext context = new ServerDataContext();
        
        Games? game = await guild.GetGame(title, context);

        if (game != null) {
            context.Games.Remove(game);
            await context.SaveChangesAsync();
            return true;
        }
        return false;
        
    }
    
    /// <summary>
    /// Adds a Category to a game on a server.
    /// </summary>
    /// <param name="guild">The guild with the game</param>
    /// <param name="gameTitle">the title of the game you are adding the category to</param>
    /// <param name="categoryName">The name of the category</param>
    /// <param name="unit">The unit of scores within the category.</param>
    /// <param name="higherBetter">Weather higher scores are better in this category</param>
    /// <returns>True if the category has been added and False if it failed.</returns>
    /// <exception cref="InvalidOperationException">Thows this exception if it cant find the game.</exception>
    public static async Task<bool> AddCategoryAsync(this IGuild guild, string gameTitle, string categoryName, string unit, bool higherBetter) {
        
        await using ServerDataContext context = new ServerDataContext();
        
        // get the game
        Games? game = await guild.GetGame(gameTitle, context);

        if (game == null) {
            throw new InvalidOperationException("You should never see this message.");
        }
        
        int numbCategories = context.Categories.Count(c => c.game == game && c.name == categoryName);

        if (numbCategories == 1) {
            return false;
        }
        else if (numbCategories > 1) {
            await Bot.LogAsync(LogSeverity.Critical,
                $"Found a duplicate category `{categoryName} in game {game.title}(`{game.game_id}`)");
            return false;
        }
        else {
            Categories newCategory = new Categories {
                game = game,
                game_id = game.game_id,
                higherBetter = higherBetter,
                name = categoryName,
                unit = unit
            };

            context.Categories.Add(newCategory);
            await context.SaveChangesAsync();
            return true;
        }
    }
    
    /// <summary>
    /// displays a list of categories of a game with the score of a user.
    /// </summary>
    /// <param name="user">The user whose score will be displayed</param>
    /// <param name="guild">the guild with the game.</param>
    /// <param name="gameTitle">The title of the game.</param>
    /// <returns>A list of each category and score. Or null if the game doesn't exist.</returns>
    public static async Task<List<string>?> GetCategoriesWithScores(IUser user, IGuild guild, string gameTitle) {
        var result = new List<string>();

        await using ServerDataContext context = new ServerDataContext();
        
        await guild.AddUserAsync(user, context);

        Games? game = await guild.GetGame(gameTitle, context);

        if (game == null) {
            return null;
        }
        
        List<Categories> categoriesList = await context.Categories.Where(c => c.game == game).ToListAsync();

        Users? dbUser = await guild.GetDbUserAsync(user, context);

        if (dbUser == null) {
            return null;
        }
        IQueryable<Scores> scoresListQueryable = context.Scores.Where(s => s.user == dbUser);

        foreach (Categories category in categoriesList) {
            List<Scores> scoresList = await scoresListQueryable.Where(s => s.category == category).ToListAsync();
                    
                    
            // freak out if there is a duplicate scores
            if (scoresList.Count > 1) {
                await Bot.LogAsync(LogSeverity.Critical,
                    $"Found {scoresList.Count} duplicate score(s) for {user.Username}(`{dbUser.user_id} in category " +
                    $"{category.name}(`{category.category_id}`)");
                return null;
            }
                    
            // if there is a score output it.
            else if (scoresList.Count == 1) {
                Scores score = scoresList.First();
                        
                result.Add($"**{category.name}** {user.Username}: {score.value} {category.unit}");
            }
            
            // if the user doesent have a score here, just output the category name.
            else {
                result.Add($"**{category.name}**");
            }
                    
        }
        
        return result;
    }
    
    /// <summary>
    /// Overload of GetCategoriesWithScores(IUser, IGuild, string)
    ///
    /// displays a list of categories of a game with the score of a user.
    /// </summary>
    /// <param name="member">Member of a guild whose score will be displayed.</param>
    /// <param name="gameTitle">The title of the game.</param>
    /// <returns>A list of each category and score. Or null if the game doesn't exist.</returns>
    public static async Task<List<string>?> GetCategoriesWithScores(this IGuildUser member, string gameTitle) {
        return await GetCategoriesWithScores(member, member.Guild, gameTitle);
    }
    
    
    /// <summary>
    /// Returns the categories associated with a game on a server
    /// </summary>
    /// <param name="guild">The guild with the game.</param>
    /// <param name="gameTitle">The title of the game.</param>
    /// <returns>A list of category names.</returns>
    public static async Task<List<string>?> GetDbCategoriesAsync(this IGuild guild, string gameTitle) {
        var result = new List<string>();

        await using ServerDataContext context = new ServerDataContext();
        

        Games? game = await guild.GetGame(gameTitle, context);

        if (game == null) {
            return null;
        }
        
        List<Categories> categoriesList = await context.Categories.Where(c => c.game == game).ToListAsync();

        foreach (Categories category in categoriesList) {
            result.Add($"**{category.name}**");
        }
        
        return result;
    }
    
    
    /// <summary>
    /// Removes a category from a game on a server
    /// </summary>
    /// <param name="guild">The server where the game is</param>
    /// <param name="gameTitle">The title of the game</param>
    /// <param name="categoryName">The name of the category to be removed.</param>
    /// <returns>True if successful, False if not.</returns>
    public static async Task<bool> RemoveCategoryAsync(this IGuild guild, string gameTitle, string categoryName) {
        await using ServerDataContext context = new ServerDataContext();
        

        List<Categories> categories = await context.Categories
            .Where(c=> c.game.title == gameTitle && c.name == categoryName)
            .ToListAsync();

        if (await categories.OnlyOneAsync(
                $"Found a duplicate category {categoryName} in game {gameTitle} on {guild.Name}(`{guild.Id}`)")) 
        {
            Categories category = categories.First();
            
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Gets whether the output of the flow command should be spoiled
    /// </summary>
    /// <param name="guild"></param>
    /// <returns>the bool value</returns>
    public static async Task<bool?> GetSafeFlow(this IGuild guild) {
        await using ServerDataContext context = new ServerDataContext();

        List<bool> SafeFlowOutput = context.Servers
            .Where(s => s.server_id == guild.Id)
            .Select(s => s.safeFlow).ToList();

        if (await SafeFlowOutput.OnlyOneAsync($"Found more than one server under server_id == {guild.Id}")) {
            return SafeFlowOutput.First();
        }

        return null;
    }

    public static async Task<bool> AddScore(this IGuild guild, IUser discordUser, string gameTitle, string categoryName, double value) {
        ServerDataContext Context = new ServerDataContext();

        await guild.AddUserAsync(discordUser, Context);
        
        Users user = await Context.Users
            .Where(u => u.server_id == guild.Id && u.discordUser_id == discordUser.Id)
            .FirstOrDefaultAsync();

        Categories? category = await Context.Categories
            .Where(c => c.game.title == gameTitle && c.name == categoryName)
            .FirstOrDefaultAsync();

        if (category != null) {
            Scores newScore = new Scores {
                user = user,
                category = category,
                value = value
            };

            await Context.Scores.AddAsync(newScore);

            await Context.SaveChangesAsync();

            return true;
        }

        return false;
    }
    
}