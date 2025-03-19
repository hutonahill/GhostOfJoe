using System.Diagnostics;


namespace GhostOfJoe;

public class Jokes {
    private static readonly List<string> JokeList = new List<string>();
    
    public static async Task ImportJokes() {
        Debug.Assert(Bot.config != null, "Program.config != null");
        string userKey = await Flow.GetUserKey();

        string raw = await Flow.GetRawPrivatePaste(Bot.config.jokePasteKey, userKey);

        JokeList.AddRange(raw.Split("/n"));
    }

    public static string GetJoke() {
        Random random = new Random();

        return JokeList[random.Next(0, JokeList.Count - 1)];
    }
    
    
}