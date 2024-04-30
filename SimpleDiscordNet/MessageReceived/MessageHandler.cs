using System.Reflection;
using Discord.WebSocket;

namespace SimpleDiscordNet.MessageReceived; 

public static class MessageHandler {

    internal static readonly List<MessageHandlerInstance> MessageHandlers = new();

    internal static void LoadMessageHandlers() {
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsClass)
            .SelectMany(x => x.GetMethods())
            .Where(x => x.GetCustomAttributes(typeof(MessageListenerAttribute), false).FirstOrDefault() != null);

        foreach (MethodInfo method in methods) {
            object? obj = Activator.CreateInstance(method.DeclaringType!);
            MessageListenerAttribute attribute = method.GetCustomAttribute<MessageListenerAttribute>()!;
            MessageHandlerInstance instance = new() {
                Function = (cmd, client) => (Task) method.Invoke(obj, new object[] {cmd, client})!,
                IgnoreBots = attribute.IgnoreBots
            };
            MessageHandlers.Add(instance);
        }
    }
    
    internal static async Task Run(SocketMessage msg, SimpleDiscordBot bot) {
        if (msg.Author.Id == bot.Client.CurrentUser.Id) {
            return;
        }
        if (MessageHandlers.Count == 0) {
            return;
        }
        bot.Debug("Message Handler", "Message Handler");

        try {
            foreach (MessageHandlerInstance handler in MessageHandlers.Where(handler => !handler.IgnoreBots || !msg.Author.IsBot)) {
                await handler.Function(msg, bot.Client);
            }
        }
        catch (Exception e) {
            bot.Error("Message Handler", e);
            bot.Error("Message Handler", "Message handler failed");
            
            // Dump info
            bot.Debug("Message Handler", $"User: {msg.Author.Username}#{msg.Author.Discriminator}");
            bot.Debug("Message Handler", $"Message: {msg.Content}");
            bot.Debug("Message Handler", $"Channel: {msg.Channel.Name}");
        }
    }
    
}