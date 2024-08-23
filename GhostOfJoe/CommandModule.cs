using System.Data.SQLite;
using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;


namespace GhostOfJoe;

public class CommandModule : InteractionModuleBase<SocketInteractionContext> {
    
    public InteractionService? Commands { get; set; }
    
    
    [SlashCommand("ping", "You there?")]
    public async Task PingAsync() {
        try {
            SocketGuildUser member = (SocketGuildUser)Context.User;
            if (member == null) throw new ArgumentNullException(nameof(member));
        
            await RespondAsync(Program.ApplyTitle(member));
        }
        catch (Exception ex) {
            await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(PingAsync), "", ex));
        }
       
    }
    

    [SlashCommand("grant_title", "Commemorate acts of glory by granting the honored one a glorious (and unique) title!")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task GrantTitleAsync([Summary(description: "The user receiving the title")]SocketGuildUser member, 
        [Summary(description: "The title to be granted")] string title) 
    {

        SocketGuildUser? holder = DataHandler.GetMemberWithTitle(title, member.Guild);
        
        if (holder != null && holder != member) {
            

            await ReplyAsync($"Hark! {holder.Mention} already possesses the title '{title}' and no user " +
                                     $"may possess the title of another");
        }
        else if (holder != null && holder == member) {
            await ReplyAsync(
                $"{member.Mention} already has the title '{title}'. Perhaps they need a different one?");
        }
        else {
            await DataHandler.AddTitle(member, title);
            await ReplyAsync($"{member.Mention} shall hereby be referred to as " +
                             $"\"{title.Replace("<username>", member.Mention)}\"");
        }
        
        await RespondAsync();
    }
    
    
    [SlashCommand("revoke_title", "Strips a disgraced amigo of that once glorious title")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RevokeTitleAsync(
        [Summary("member", "The member to grant the title to")]SocketGuildUser member, 
        [Summary("title","The title to be revoked")] string title) 
    {

        if (DataHandler.RemoveTitle(member, title)) {

            await ReplyAsync($"{member.Mention} has been striped of the title '{title}'");
        }
        else {
            await ReplyAsync("my brain did a bad. Might have to talk to a doctor or something.");
        }
        
        await RespondAsync();
    }
    
    
    [SlashCommand("get_game_list", "Returns a list of games from the high score database")]
    public async Task GetGameListAsync() {
        await DeferAsync();
        try {
            // Retrieve the game list
            List<string> gameList = await DataHandler.GetGames(Context.Guild.Id);



            // Format the game list into a single string
            string gameListString = "";

            foreach (string game in gameList) {
                gameListString += $"\n- {game}";
            }

            // Send the list to the user
            if (gameList.Count > 0) {
                await FollowupAsync($"**Games**:{gameListString}");
            }
            else {
                await FollowupAsync("No games available.");
            }
        }
        catch (Exception ex) {
            await Program.LogAsync(new LogMessage(LogSeverity.Error, nameof(GetGameListAsync),"", ex));
            throw;
        }
    }

    [SlashCommand("add_game", "Adds a command to the high score database")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddGameAsync( string game) {
        await DeferAsync();
        if (DataHandler.AddGame(Context.Guild, game)) {
            await FollowupAsync($"{game} has been added!");
        }
        else {
            await FollowupAsync("Sorry bub, can't do that one. I bet you already tried to add that");
        }
    }


    [SlashCommand("remove_game", "Removes a game from the high score database")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveGameAsync([Summary(description:"game to remove"), Autocomplete(typeof(CompleteGame))] string game) {
        await DeferAsync();
        
        bool success = false;
        try {
            success = DataHandler.RemoveGame(Context.Guild, game);
        }
        catch (Exception ex) {
            await Program.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(RemoveCategoryAsync)}",
                $"Game removal failed: {ex.Message}", ex));
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
            success = DataHandler.AddCategory(Context.Guild.Id, game, category, unit, higher_better);
        }
        catch (InvalidOperationException) {
            await FollowupAsync($"{game}? never heard of it. But {category} certainly sounds interesting. " +
                             $"You will have to tell me about this game.");
        }
        catch (Exception ex) {
            await ReplyAsync("I dont know man... I dont feel so good. Somethings off. My brain did a goof. ");
            await Program.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(AddCategoryAsync)}",
                "Failed to add game category", ex));
        }

        if (success) {

            if (category.ToLower().Contains("motes") && category.ToLower().Contains("bank")) {
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
    
    
    [SlashCommand("removing_category", "Removes a category from a game")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveCategoryAsync(
        [Summary("game", "game with the category you want to remove"), Autocomplete(typeof(CompleteGame))] string game, 
        [Summary("category", "The Category you want to remove from the game."), Autocomplete(typeof(CompleteCategory))] string category) 
    {
        bool success = false;
        try {
            success = DataHandler.RemoveCategory(Context.Guild, game, category);
        }
        catch (Exception ex) {
            await Program.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(RemoveCategoryAsync)}",
                $"Game removal failed: {ex.Message}", ex));
        }
        
        
        if (success) {
            await ReplyAsync("The game is gone. Sure hope I didnt delete anything to impressive.");
        }
        else {
            await ReplyAsync("My dude doesn't like a game so much they try to delete it and it didnt even exist. Or perhaps I am crazy");
        }
        
        await RespondAsync();
    }
    
    
    [SlashCommand("flow", "cite a passage from the Book of Flow")]
    public async Task FlowAsync(string userMessage) {
        
        ulong targetChannelId = Context.Channel.Id;
        
        ITextChannel? channel = Context.Client.GetChannel(targetChannelId) as ITextChannel;



        bool NsfwFlow = await DataHandler.GetSafeFlow(Context.Guild);

        Debug.Assert(channel != null, nameof(channel) + " != null");
        if (NsfwFlow != channel.IsNsfw){
            await ReplyAsync("This command can only be used in NSFW channels.");
            return;
        }
        
        string result = Flow.CiteFlow(userMessage);
        await RespondAsync(result);
    }
    
    
    [SlashCommand("get_global_settings", "get a list of my global settings. Look don't touch.")]
    public async Task GetGlobalSettingsAsync() {
        Debug.Assert(Program.config != null, "Program.config != null");
        await ReplyAsync($"## Current Global Settings:\n{getSettings(Program.config.GlobalSettings)}");
        
        await RespondAsync();
    }
    
    
    [SlashCommand("set_global_setting", "Modify my global settings. Bussies only.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetGlobalSettingAsync(
        [Summary(description:"The name of the setting you want to change"), Autocomplete(typeof(CompleteGlobalSettingKey))]string key, 
        string value) 
    {
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

        await RespondAsync();
    }
    
    private string SetGlobalSetting<T>(string key, T value) {
        if (!Program.AdminServers.Contains(Context.Guild.Id)) {
            return ("Sorry, this Table is not cool enough to have admin powers. Long live the Party Bus");
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

        Dictionary<string, string> targetSettings = DataHandler.GetServerSettings(Context.Guild);

        string output = "";

        foreach (KeyValuePair<string,string> pair in targetSettings) {
            output += $"**{pair.Key}**: {pair.Value}\n";
        }
        
        await ReplyAsync($"## Current Local Settings:\n{output}");
        
        await RespondAsync();
    }
    
    
    [SlashCommand("set_settings", "Change your Table settings.")]
    [Discord.Commands.RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task SetSettingsAsync(string key, string value) {
        try {
            (bool exists, string? dataType) = DataHandler.GetSettingDataType(key);
    
            if (!exists || dataType == null) {
                await ReplyAsync($"The setting '{key}' does not exist.");
                return;
            }

            if (!DataHandler.TryParseType(dataType, value, out var parsedValue)) {
                await ReplyAsync($"Invalid value '{value}' for the setting '{key}' of type '{dataType}'.");
                return;
            }

            if (DataHandler.UpdateSetting(key, parsedValue, Context.Guild)) {
                await ReplyAsync($"Successfully updated the setting '{key}' to '{value}'.");
            } else {
                await ReplyAsync($"Failed to update the setting '{key}'.");
            }
        } catch (Exception ex) {

            await Program.LogAsync(new LogMessage(LogSeverity.Error, 
                nameof(SetSettingsAsync), $"we got an error on Table " +
                                          $"{Context.Guild.Name}({Context.Guild.Id})", ex));
            
            await ReplyAsync($"My brain did an big oopses. i have informed the creator.");
        }
        await RespondAsync();
    }
    
    private string getSettings(Dictionary<string, SettingBase?> settings) {
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

        List<string> games = await DataHandler.GetGames(context.Guild.Id);
        AutocompleteResult[] temp = new AutocompleteResult[games.Count];

        int index = 0;
        
        foreach (string game in games) {
            temp[index] = new AutocompleteResult(game, game);
            index += 1;
        }

        IEnumerable<AutocompleteResult> results = temp;
        
        // Create a collection with suggestions for autocomplete
        

        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}

public class CompleteGlobalSettingKey : AutocompleteHandler {
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        Debug.Assert(Program.config != null, "Program.config != null");
        List<string> GlobalSettingKeys = new List<string>(Program.config.GlobalSettings.Keys);
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
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) 
    {
        IEnumerable<AutocompleteResult> results = new AutocompleteResult[1];

        object? gameValue = autocompleteInteraction.Data.Options.First(x => x.Name == "game").Value;
        
        string? gameTitle = gameValue.ToString();
        
        if (gameTitle != null) {
            
            List<string>? CategoryList = DataHandler.GetCategories(context.Guild, gameTitle);
            
            
            
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
        
        return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25)));
    }
}