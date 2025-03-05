
using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GhostOfJoe.Models;

namespace GhostOfJoe;

public class CommandModule : InteractionModuleBase<SocketInteractionContext> {
    
    public InteractionService? Commands { get; set; }
    
    
    [SlashCommand("ping", "You there?")]
    public async Task PingAsync() {
        try {
            SocketGuildUser member = (SocketGuildUser)Context.User;
            if (member == null) throw new ArgumentNullException(nameof(member));
        
            await RespondAsync(await member.ApplyTitle());
        }
        catch (Exception ex) {
            await Bot.LogAsync(LogSeverity.Error, ex.Message, ex);
        }
       
    }
    

    [SlashCommand("grant_title", "Commemorate acts of glory by granting the honored one a glorious (and unique) title!")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task GrantTitleAsync(
        [Summary(description: "The user receiving the title"), Autocomplete(typeof(CompleteGuildMember))] SocketGuildUser member, 
        [Summary(description: "The title to be granted")] string title) 
    {
        IGuildUser? holder = await member.Guild.GetMemberWithTitle(title);
        
        if (holder != null && holder != member) {
            
            await RespondAsync($"Hark! {holder.Mention} already possesses the title '{title}' and no user " +
                               $"may possess the title of another!");
        }
        else if (holder != null && holder == member) {
            await RespondAsync(
                $"{member.Mention} already has the title '{title}'. Perhaps they need a different one?");
        }
        else {
            bool success = await member.AddTitle(title);

            if (success) {
                await RespondAsync($"{member.Mention} shall hereby be referred to as " +
                                   $"\"{title.Replace("<username>", member.Mention)}\"");
            }
            else {
                await RespondAsync($"I dont know what to tell you man. ");
            }
        }
    }
    
    
    [SlashCommand("revoke_title", "Strips a disgraced amigo of that once glorious title")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RevokeTitleAsync(
        [Summary("member", "The member to grant the title to")]SocketGuildUser member, 
        [Summary("title","The title to be revoked")] string title) 
    {

        if (await member.RemoveTitle(title)) {

            await ReplyAsync($"{member.Mention} has been striped of the title '{title}'");
        }
        else {
            await ReplyAsync("my brain did a bad. Might have to talk to a doctor or something.");
        }
        
        await RespondAsync();
    }
    
    
    [SlashCommand("get_game_list", "Returns a list of games from the high score database")]
    public async Task GetGameListAsync() {
        try {
            // Retrieve the game list
            List<string> gameList = await Context.Guild.GetGames();



            // Format the game list into a single string
            string gameListString = "";

            foreach (string game in gameList) {
                gameListString += $"\n- {game}";
            }

            // Send the list to the user
            if (gameList.Count > 0) {
                await RespondAsync($"**Games**:{gameListString}");
            }
            else {
                await RespondAsync("No games available.");
            }
        }
        catch (Exception ex) {
            await Bot.LogAsync(LogSeverity.Error, "", ex);
            throw;
        }
    }

    [SlashCommand("add_game", "Adds a command to the high score database")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddGameAsync(string game) {
        bool success = await Context.Guild.AddGame(game);
        if (success) {
            await RespondAsync($"{game} has been added!");
        }
        else {
            await RespondAsync("Sorry bub, can't do that one. I bet you already tried to add that");
        }
    }


    [SlashCommand("remove_game", "Removes a game_title from the high score database")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveGameAsync([Summary(description:"title of the game to remove"), Autocomplete(typeof(CompleteGame))] string game_title) {
        await DeferAsync();
        
        bool success = false;
        try {
            success = await Context.Guild.RemoveGameAsync(game_title);
        }
        catch (Exception ex) {
            await Bot.LogAsync(LogSeverity.Critical,
                $"Game removal failed: {ex.Message}", ex);
        }
        
        
        if (success) {
            await FollowupAsync("The game is gone. Sure hope I didnt delete anything to impressive.");
        }
        else {
            await FollowupAsync("My dude doesn't like a game so much they try to delete it and it didnt even exist. Or perhaps I am crazy");
        }
    }
    
    
    [SlashCommand("get_game_categories", "Get the categories for a game in the high score database")]
    public async Task GetGameCategoriesAsync([Summary(description:"game to get the categories of"), Autocomplete(typeof(CompleteGame))] string game) {
        await DeferAsync();
        List<string>? categoryList = await DataHandler.GetCategoriesWithScores(Context.User, Context.Guild, game);
        
         if(categoryList != null){   
             string categoryString = "";
        
             foreach (string element in categoryList) {
                 categoryString += element + "\n";
             }
            await FollowupAsync($"**Categories**:\n{categoryString}");
        }
        else {
            await FollowupAsync("We dont got that game bro :/");
        }
    }
    
    
    [SlashCommand("add_category", "Adds a category to a game ")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddCategoryAsync([Summary(description:"game to add a category to"), Autocomplete(typeof(CompleteGame))] string game,
        string category, string unit, bool higher_better) 
    {
        await DeferAsync();
        bool success = false;
        try {
            success = await Context.Guild.AddCategoryAsync(game, category, unit, higher_better);
        }
        catch (InvalidOperationException) {
            await FollowupAsync($"{game}? never heard of it. But {category} certainly sounds interesting. " +
                             $"You will have to tell me about this game.");
        }
        catch (Exception ex) {
            await ReplyAsync("I dont know man... I dont feel so good. Somethings off. My brain did a goof. ");
            await Bot.LogAsync(LogSeverity.Critical, "Failed to add game category", ex);
        }

        if (success) {

            if (category.ToLower().Contains("mote") && category.ToLower().Contains("bank")) {
                await FollowupAsync(
                    $"Well, well, well. Another realm of competition. {category}, eh? " +
                    $"Drifter would be proud.");
            }
            else {
                await FollowupAsync(
                    $"Well, well, well. Another realm of competition. {category}, eh? " +
                    $"Ive always though motes banked was enough, but to each their own.");
            }
        }
        else {
            await ReplyAsync("I dont know what went to tell you man, something went wrong");
        }
    }
    
    
    [SlashCommand("remove_category", "Removes a category from a game")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveCategoryAsync(
        [Summary("game_title", "the title of the game with the category you want to remove"), Autocomplete(typeof(CompleteGame))] string gameTitle, 
        [Summary("category", "The Category you want to remove from the game."), Autocomplete(typeof(CompleteCategory))] string category) 
    {
        bool success = false;
        try {
            success = await Context.Guild.RemoveCategoryAsync(gameTitle, category);
        }
        catch (Exception ex) {
            await Bot.LogAsync(LogSeverity.Critical, $"Game removal failed: {ex.Message}", ex);
        }
        
        
        if (success) {
            await ReplyAsync("The game is gone. Sure hope I didnt delete anything to impressive.");
        }
        else {
            await ReplyAsync("My dude doesn't like a game so much they try to delete it and it didnt even exist. Or perhaps I am crazy");
        }
        
        await RespondAsync();
    }

    [SlashCommand("add_score", "Adds your score to a category.")]
    public async Task AddScore(
        [Summary("game_title", "Title of the game you want to add your score to"), Autocomplete(typeof(CompleteGame))]
        string gameTitle,
        [Summary("category", "The Category you want to add your score to."), Autocomplete(typeof(CompleteCategory))]
        string category, [Summary("value", "The value of the score your recording")]double value) {
        
        bool success = await Context.Guild.AddScore(Context.User, gameTitle, category, value);

        if (success == true) {
            await RespondAsync("Boom. Done.");
        }
        else {
            await RespondAsync("Um. not sure that one exists. Make sure I know about that game and category.");
        }
    }
    
    [SlashCommand("flow", "cite a passage from the Book of Flow")]
    public async Task FlowAsync(string userMessage) {
        
        ulong targetChannelId = Context.Channel.Id;
        
        ITextChannel? channel = Context.Client.GetChannel(targetChannelId) as ITextChannel;



        bool? NsfwFlow = await Context.Guild.GetSafeFlow();
        
        Debug.Assert(NsfwFlow != null, "NsfwFlow != null");
        Debug.Assert(channel != null, nameof(channel) + " != null");
        if (NsfwFlow != channel.IsNsfw){
            await ReplyAsync("This command can only be used in NSFW channels.");
            return;
        }
        
        string result = Flow.CiteFlow(userMessage);
        await RespondAsync(result);
    }
    
    
    [SlashCommand("get_global_settings", "get a list of my global settings. Look don't touch.")]
    [RequireUserPermission((GuildPermission.Administrator))]
    public async Task GetGlobalSettingsAsync() {
        Debug.Assert(Bot.config != null, "Program.config != null");
        await RespondAsync($"Only the bot owner may modify these settings, but server admins may see them." +
                           $"\n## Current Global Settings:\n{getGlobalSettings(Bot.config.GlobalSettings)}", ephemeral: true);
        
    }
    
    
    [SlashCommand("set_global_setting", "Modify my global settings. Bussies only.")]
    [Discord.Commands.RequireOwner]
    public async Task SetGlobalSettingAsync(
        [Summary(description:"The name of the setting you want to change"), Autocomplete(typeof(CompleteGlobalSettingKey))]string key, 
        string value) 
    {
        Debug.Assert(Bot.config != null, "Program.config != null");
        if (Bot.config.AdminServers.Contains(Context.Guild.Id)) {
            Debug.Assert(Bot.config != null, "Program.config != null");
            Dictionary<string, ISetting?> settings = Bot.config.GlobalSettings;

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

        await RespondAsync();
    }
    
    private string SetGlobalSetting<T>(string key, T value) {
        Debug.Assert(Bot.config != null, "Program.config != null");
        if (!Bot.config.AdminServers.Contains(Context.Guild.Id)) {
            return ("Sorry, this Server is not cool enough to have admin powers. Long live the Party Bus");
        }

        Debug.Assert(Bot.config != null, "Program.config != null");
        if (Bot.config.GlobalSettings.ContainsKey(key)) {
            try {
                Bot.SetSettingValue(Bot.config.GlobalSettings, key, value);
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

        Dictionary<string, string> targetSettings = await Context.Guild.GetSettingsAsync();

        string output = "";

        foreach (KeyValuePair<string,string> pair in targetSettings) {
            output += $"**{pair.Key}**: {pair.Value}\n";
        }
        
        await ReplyAsync($"## Current Local Settings:\n{output}");
        
        await RespondAsync();
    }
    
    
    [SlashCommand("set_settings", "Change your Server settings.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetSettingsAsync(string key, string value) {
        try {
            Type? dataType = DataHandler.GetSettingDataType(key);
    
            if (dataType == null) {
                await RespondAsync($"The setting '{key}' does not exist.");
                return;
            }

            if (!DataHandler.TryParseType(dataType, value, out object parsedValue)) {
                await RespondAsync($"Invalid value '{value}' for the setting '{key}' of type '{dataType.FullName}'.");
                return;
            }
            
            if (await Context.Guild.UpdateSettingAsync(key, parsedValue) ?? false) {
                await RespondAsync($"Successfully updated the setting '{key}' to '{value}'.");
                return;
            } else {
                await RespondAsync($"Failed to update the setting '{key}'.");
                return;
            }
        } catch (Exception ex) {
            await Bot.LogAsync(LogSeverity.Error, $"We got an error on Server " +
                                                  $"{Context.Guild.Name}(`{Context.Guild.Id}`) for a command executed by " +
                                                  $"{Context.User.Username}(`{Context.User.Id}`) ", ex);
            
            await RespondAsync($"My brain did an big oopses. I have informed the creator.");
        }
    }
    
    /// <summary>
    /// converts settings into a string
    /// </summary>
    /// <param name="settings">the settings to convert</param>
    /// <returns></returns>
    private static string getGlobalSettings(Dictionary<string, ISetting?> settings) {
        List<string> settingList = new List<string>();
        
        foreach (string key in settings.Keys) {
            settingList.Add($"- **{key}** \n {settings[key]}");
        }

        return string.Join("\n", settingList);
    }
}

public class CompleteGame : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {

        List<string> games = await context.Guild.GetGames();
        AutocompleteResult[] temp = new AutocompleteResult[games.Count];

        int index = 0;
        
        foreach (string game in games) {
            temp[index] = new AutocompleteResult(game, game);
            index ++;
        }

        IEnumerable<AutocompleteResult> results = temp;
        
        // Create a collection with suggestions for autocomplete
        

        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}

public class CompleteGuildMember : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {

        IReadOnlyCollection<IGuildUser> guildUsers = await context.Guild.GetUsersAsync();

        AutocompleteResult[] temp = new AutocompleteResult[guildUsers.Count];

        int index = 0;
        foreach (IGuildUser guildUser in guildUsers) {
            temp[index] = new AutocompleteResult(guildUser.Username, guildUser);
            index++;
        }

        IEnumerable<AutocompleteResult> results = temp;


        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}

public class CompleteGlobalSettingKey : AutocompleteHandler {
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        Debug.Assert(Bot.config != null, "Program.config != null");
        List<string> GlobalSettingKeys = new List<string>(Bot.config.GlobalSettings.Keys);
        AutocompleteResult[] temp = new AutocompleteResult[GlobalSettingKeys.Count];

        int index = 0;

        foreach (string key in GlobalSettingKeys) {
            temp[index] = new AutocompleteResult(key, key);
            index++;
        }
        
        IEnumerable<AutocompleteResult> results = temp;
        
        return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25)));
    }
}

public class CompleteCategory : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) 
    {
        IEnumerable<AutocompleteResult> results = new AutocompleteResult[1];

        object? gameValue = autocompleteInteraction.Data.Options.First(x => x.Name == "game").Value;
        
        string? gameTitle = gameValue.ToString();
        
        if (gameTitle != null) {
            
            List<string>? CategoryList = await context.Guild.GetDbCategoriesAsync(gameTitle);
            
            
            
            if (CategoryList != null) {
                AutocompleteResult[] temp = new AutocompleteResult[CategoryList.Count];
                int index = 0;
                foreach (string category in CategoryList) {
                    temp[index] = new AutocompleteResult(category, category);
                    index++;
                }
                
                results = temp;
            }
        }
        
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}