using Discord.WebSocket;

namespace SimpleDiscordNet; 

public static class GeneralUtils {
    
    public static string GetValue(this IEnumerable<SocketMessageComponentData> components, string key) {
        string? readOnlyCollection = components.First(x => x.CustomId == key).Value;
        if (readOnlyCollection == null) {
            throw new Exception("No values found");
        }
        return readOnlyCollection;
    }
    
}