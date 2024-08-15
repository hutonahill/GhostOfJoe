using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;


namespace GhostOfJoe;

public class CommandModule : InteractionModuleBase<SocketInteractionContext> {
    
    public InteractionService? Commands { get; set; }
    
    
    [SlashCommand("ping", "You there?")]
    public async Task PingAsync() {
        var member = Context.User as SocketGuildUser;
        if (member == null) throw new ArgumentNullException(nameof(member));
        
        await ReplyAsync(Program.ApplyTitle(member));
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
            DataHandler.AddTitle(member, title);
            await ReplyAsync($"{member.Mention} shall hereby be referred to as " +
                             $"\"{title.Replace("<username>", member.Mention)}\"");
        }
    }
    
    
    [SlashCommand("revoke_title", "Strips a disgraced amigo of that once glorious title")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RevokeTitleAsync(SocketGuildUser member, 
        [Summary(description:"The title to be revoked")] string title) 
    {

        if (DataHandler.RemoveTitle(member, title)) {

            await ReplyAsync($"{member.Mention} has been striped of the title '{title}'");
        }
        else {
            await ReplyAsync("my brain did a bad. Might have to talk to a doctor or something.");
        }
    }
    
    
    [SlashCommand("get_game_list", "Returns a list of games from the high score database")]
    public async Task GetGameListAsync() {
        // Retrieve the game list
        List<string> gameList = DataHandler.GetGames(Context.Guild.Id);
        
        
        
        // Format the game list into a single string
        string gameListString = "";

        foreach (string game in gameList) {
            gameListString += $"\n- {game}";
        }

        // Send the list to the user
        if (gameList.Count > 0) {
            await ReplyAsync($"**Games**:{gameListString}");
        } else {
            await ReplyAsync("No games available.");
        }
    }

    [SlashCommand("add_game", "Adds a command to the high score database")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddGameAsync( string game) {
        if (DataHandler.AddGame(Context.Guild, game)) {
            await ReplyAsync($"{game} has been added!");
        }
        else {
            await ReplyAsync("Sorry bub, can't do that one. I bet you already tried to add that");
        }
    }


    [SlashCommand("remove_game", "Removes a game from the high score database")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveGameAsync([Summary(description:"game to remove"), Autocomplete(typeof(ComplicateGame))] string game) {
        bool success = false;
        try {
            success = DataHandler.RemoveGame(Context.Guild, game);
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
    }
    
    
    [SlashCommand("get_game_categories", "Get the categories for a game in the high score database")]
    public async Task GetGameCategoriesAsync([Summary(description:"game to get the categories of"), Autocomplete(typeof(ComplicateGame))] string game) {
        List<string>? categoryList = DataHandler.GetCategoriesWithScores(Context.User, Context.Guild, game);
        
         if(categoryList != null){   
             string categoryString = "";
        
             foreach (string element in categoryList) {
                 categoryString += element + "\n";
             }
            await ReplyAsync($"**Categories**:\n{categoryString}");
        }
        else {
            await ReplyAsync("We dont got that game bro :/");
        }
         
        await RespondAsync();
    }
    
    
    [SlashCommand("add_category", "Adds a category to a game ")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task AddCategoryAsync([Summary(description:"game to add a category to"), Autocomplete(typeof(ComplicateGame))] string game,
        string category, string unit, bool higher_better) 
    {
        bool success = false;
        try {
            success = DataHandler.AddCategory(Context.Guild.Id, game, category, unit, higher_better);
        }
        catch (InvalidOperationException) {
            await ReplyAsync($"{game}? never heard of it. But {category} certainly sounds interesting. " +
                             $"You will have to tell me about this game.");
        }
        catch (Exception ex) {
            await ReplyAsync("I dont know man... I dont feel so good. Somethings off. My brain did a goof. ");
            await Program.LogAsync(new LogMessage(LogSeverity.Critical, $"{nameof(CommandModule)}.{nameof(AddCategoryAsync)}",
                "Failed to add game category", ex));
        }

        if (success) {

            if (category.ToLower().Contains("motes") && category.ToLower().Contains("bank")) {
                await ReplyAsync(
                    $"Well, well, well. Another realm of competition. {category}, eh? " +
                    $"Drifter would be proud.");
            }
            else {
                await ReplyAsync(
                    $"Well, well, well. Another realm of competition. {category}, eh? " +
                    $"Ive always though motes banked was enough, but to each their own.");
            }
            
            
        }
        else {
            await ReplyAsync("I dont know what went to tell you man, something went wrong");
        }
        
        await RespondAsync();
    }
    
    
    [SlashCommand("removing_category", "Removes a category from a game")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task RemoveCategoryAsync([Summary(description:"game with the category you want to remove"), Autocomplete(typeof(ComplicateGame))] string game, 
        string category) 
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

        Debug.Assert(Program.config != null, "Program.config != null");
        Dictionary<string, SettingBase?> settings = Program.config.GlobalSettings;

        bool NsfwFlow = Program.GetSettingValue<bool>(settings, "NsfwFlow");

        Debug.Assert(channel != null, nameof(channel) + " != null");
        if (NsfwFlow != channel.IsNsfw){
            await ReplyAsync("This command can only be used in NSFW channels.");
            return;
        }
        
        string result = Flow.CiteFlow(userMessage);
        await ReplyAsync(result);
        await RespondAsync();
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
            return ("Sorry, this server is not cool enough to have admin powers. Long live the Party Bus");
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
    
    
    [SlashCommand("set_settings", "Change your server settings.")]
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
                nameof(SetSettingsAsync), $"we got an error on server " +
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

public class ComplicateGame : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {

        List<string> games = DataHandler.GetGames(context.Guild.Id);
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
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, 
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
        
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}