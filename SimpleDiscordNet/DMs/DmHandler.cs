using System.Reflection;
using Discord.WebSocket;

namespace SimpleDiscordNet.DMs; 

public static class DmHandler {

    internal static readonly List<Func<SocketMessage, DiscordSocketClient, Task>> DmHandlers = new();

    internal static void LoadDmHandlers() {
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsClass)
            .SelectMany(x => x.GetMethods())
            .Where(x => x.GetCustomAttributes(typeof(DmListenerAttribute), false).FirstOrDefault() != null);

        foreach (MethodInfo method in methods) {
            object? obj = Activator.CreateInstance(method.DeclaringType!);
            DmHandlers.Add((cmd, client) => (Task) method.Invoke(obj, new object[] {cmd, client})!);
        }
    }
    
    internal static async Task Run(SocketMessage msg, SimpleDiscordBot bot) {
        if (msg.Author.Id == bot.Client.CurrentUser.Id) {
            return;
        }
        if (DmHandlers.Count == 0) {
            return;
        }
        bot.Debug("DM Handler", "DM Handler");

        try {
            foreach (Func<SocketMessage, DiscordSocketClient, Task> handler in DmHandlers) {
                await handler(msg, bot.Client);
            }
        }
        catch (Exception e) {
            bot.Error("DM Handler", e);
            bot.Error("DM Handler", "DM handler failed");
            
            // Dump info
            bot.Debug("DM Handler", $"User: {msg.Author.Username}#{msg.Author.Discriminator}");

            try {
                await msg.Channel.SendMessageAsync("Sorry, something went wrong while processing your message. Please try again later.");
                return;
            }
            catch (Exception) {
                // We can't do anything about it so ignore it
                bot.Info("DM Handler", "Failed to notify user of error");
            }
        }
    }
    
}