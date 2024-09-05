using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord;
using Newtonsoft.Json;

namespace GhostOfJoe;

public static class Flow {
    private static List<List<string>>? flow = new List<List<string>>();
    
    private static async Task<string> GetRawPrivatePaste(string pasteKey, string userKey) {
        using var client = new HttpClient();
        Debug.Assert(Program.config != null, "Program.config != null");
        var values = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_dev_key", Program.config.pasteApiKey),
            new KeyValuePair<string, string>("api_user_key", userKey),
            new KeyValuePair<string, string>("api_paste_key", pasteKey),
            new KeyValuePair<string, string>("api_option", "show_paste")
        });

        var response = await client.PostAsync("https://pastebin.com/api/api_raw.php", values);

        if (response.IsSuccessStatusCode){
            return await response.Content.ReadAsStringAsync();
        }else{
            throw new Exception($"Failed to get paste: {response.StatusCode}");
        }
    }
    
    private static async Task<string> GetUserKey() {
        using HttpClient client = new HttpClient();

        Debug.Assert(Program.config != null, "Program.config != null");
        FormUrlEncodedContent values = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("api_dev_key", Program.config.pasteApiKey),
            new KeyValuePair<string, string>("api_user_name", Program.config.pasteBinUsername),
            new KeyValuePair<string, string>("api_user_password", Program.config.pasteBinPassword)
        });

        var response = await client.PostAsync("https://pastebin.com/api/api_login.php", values);

        if (response.IsSuccessStatusCode){
            string userKey = await response.Content.ReadAsStringAsync();
            return userKey; // The user key to be used in further requests
        }else{
            throw new Exception($"Failed to log in: {response.StatusCode}");
        }
        
    }
    
    public static async Task ImportFlow() {
        Debug.Assert(Program.config != null, "Program.config != null");
        string userKey = await GetUserKey();

        string json = await GetRawPrivatePaste(Program.config.flowPasteKey, userKey);
        
        flow = JsonConvert.DeserializeObject<List<List<string>>>(json);

        if (flow == null) {
            await Program.LogAsync(LogSeverity.Critical,
                $"Can't get flow. Check your pastebin.");
        }
    }
    
    public static string CiteFlow(string userMessage) {

        Match match = Regex.Match(userMessage, @"^\d+:\d+$");
        if (match.Success) {
            string[] parts = userMessage.Split(':');
            int chapter = int.Parse(parts[0]);
            int verse = int.Parse(parts[1]);
            return GetVerse(chapter, verse);
        }

        match = Regex.Match(userMessage, @"^\d+:\d+-\d+$");
        if (match.Success) {
            string[] parts = userMessage.Split(new[] { ':', '-' });
            int chapter = int.Parse(parts[0]);
            int startVerse = int.Parse(parts[1]);
            int endVerse = int.Parse(parts[2]);
            return GetVerses(chapter, startVerse, chapter, endVerse);
        }

        match = Regex.Match(userMessage, @"^\d+:\d+\s*-\s*\d+:\d+$");
        if (match.Success)
        {
            string[] parts = userMessage.Split(new[] { ':', '-' });
            int chapter1 = int.Parse(parts[0]);
            int verse1 = int.Parse(parts[1]);
            int chapter2 = int.Parse(parts[2]);
            int verse2 = int.Parse(parts[3]);
            return GetVerses(chapter1, verse1, chapter2, verse2);
        }

        return Program.GrabError("syntaxError");
    }
    
    private static string GetVerse(int chapter, int verse) {
        Debug.Assert(flow != null, nameof(flow) + " != null");
        if (chapter <= flow.Count && verse <= flow[chapter - 1].Count) {
            var verseContent = flow[chapter - 1][verse - 1];
            return $"*Flow {chapter}:{verse}* - {verseContent}";
        }

        return Program.GrabError("noFlow");
    }

    private static string GetVerses(int chapter1, int verse1, int chapter2, int verse2) {
        var output = new List<string>();

        for (var chapter = chapter1; chapter <= chapter2; chapter++) {
            output.Add($"**== Chapter {chapter} ==**");
            var startVerse = chapter == chapter1 ? verse1 : 1;
            Debug.Assert(flow != null, nameof(flow) + " != null");
            var endVerse = chapter == chapter2 ? verse2 : flow[chapter - 1].Count;

            for (var verse = startVerse; verse <= endVerse; verse++) {
                Debug.Assert(flow != null, nameof(flow) + " != null");
                if (chapter <= flow.Count && verse <= flow[chapter - 1].Count) {
                    output.Add($"*{verse}* {flow[chapter - 1][verse - 1]}\n");
                }
                else {
                    return Program.GrabError("noFlow");
                }
            }

            output.Add("");
        }

        return string.Join('\n', output);
    }
}