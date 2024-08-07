using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GhostOfJoe;

public static class Flow {
    public static string CiteFlow(string userMessage) {
        List<List<string>> flow = ImportFlow();

        Match match = Regex.Match(userMessage, @"^\d+:\d+$");
        if (match.Success) {
            string[] parts = userMessage.Split(':');
            int chapter = int.Parse(parts[0]);
            int verse = int.Parse(parts[1]);
            return GetVerse(flow, chapter, verse);
        }

        match = Regex.Match(userMessage, @"^\d+:\d+-\d+$");
        if (match.Success) {
            string[] parts = userMessage.Split(new[] { ':', '-' });
            int chapter = int.Parse(parts[0]);
            int startVerse = int.Parse(parts[1]);
            int endVerse = int.Parse(parts[2]);
            return GetVerses(flow, chapter, startVerse, chapter, endVerse);
        }

        match = Regex.Match(userMessage, @"^\d+:\d+\s*-\s*\d+:\d+$");
        if (match.Success)
        {
            string[] parts = userMessage.Split(new[] { ':', '-' });
            int chapter1 = int.Parse(parts[0]);
            int verse1 = int.Parse(parts[1]);
            int chapter2 = int.Parse(parts[2]);
            int verse2 = int.Parse(parts[3]);
            return GetVerses(flow, chapter1, verse1, chapter2, verse2);
        }

        return Program.GrabError("syntaxError");
    }
    
    public static List<List<string>> ImportFlow()
    {
        var json = File.ReadAllText("bookOfFlow.json");
        return JsonConvert.DeserializeObject<List<List<string>>>(json);
    }
    
    
    private static string GetVerse(List<List<string>> flow, int chapter, int verse) {
        if (chapter <= flow.Count && verse <= flow[chapter - 1].Count) {
            var verseContent = flow[chapter - 1][verse - 1];
            return $"*Flow {chapter}:{verse}* - {verseContent}";
        }

        return Program.GrabError("noFlow");
    }

    private static string GetVerses(List<List<string>> flow, int chapter1, int verse1, int chapter2, int verse2) {
        var output = new List<string>();

        for (var chapter = chapter1; chapter <= chapter2; chapter++) {
            output.Add($"**== Chapter {chapter} ==**");
            var startVerse = chapter == chapter1 ? verse1 : 1;
            var endVerse = chapter == chapter2 ? verse2 : flow[chapter - 1].Count;

            for (var verse = startVerse; verse <= endVerse; verse++) {
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