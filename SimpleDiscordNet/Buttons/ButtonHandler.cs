using System.Reflection;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet.Buttons; 

internal static class ButtonHandler {
    internal static readonly Dictionary<string, Func<SocketMessageComponent, DiscordSocketClient, Task>> ButtonHandlers = new();

    internal static void LoadButtonHandlers() {
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsClass)
            .SelectMany(x => x.GetMethods())
            .Where(x => x.GetCustomAttributes(typeof(ButtonListenerAttribute), false).FirstOrDefault() != null);

        foreach (MethodInfo method in methods) {
            object? obj = Activator.CreateInstance(method.DeclaringType!);
            ButtonHandlers.Add(method.GetCustomAttribute<ButtonListenerAttribute>()!.ButtonId, (cmd, client) => (Task) method.Invoke(obj, new object[] {cmd, client})!);
        }
    }
    
    internal static async Task Submit(SocketMessageComponent button, SimpleDiscordBot bot) {
        string buttonId = button.Data.CustomId;
        bot.Debug("Button Handler", "Attempting to submit button: " + buttonId);

        if (!ButtonHandlers.ContainsKey(buttonId)) {
            bot.Error("Button Handler", "Button handler not found: " + buttonId);
            await button.RespondWithEmbedAsync("Button not found", "The button you tried to invoke does not exist.", ResponseType.Error);
            return;
        }
        
        try {
            await ButtonHandlers[buttonId](button, bot.Client);
        }
        catch (Exception e) {
            bot.Error("Button Handler", e);
            bot.Error("Button Handler", "Button press " + buttonId + " failed.");
            
            // Dump info
            bot.Debug("Button Handler", "Button ID: " + buttonId);
            bot.Debug("Button Handler", $"Button user: {button.User.Username}#{button.User.Discriminator}");

            if (e is not TimeoutException) {
                try {
                    await button.RespondAsync("Sorry but I couldn't execute that command ¯\\_(ツ)_/¯");
                    return;
                }
                catch (Exception) {
                    // Ignore
                }

                try {
                    await button.ModifyOriginalResponseAsync(props => {
                        props.Content = "Sorry but I couldn't execute that command ¯\\_(ツ)_/¯";
                    });
                    return;
                }
                catch (Exception) {
                    // Ignore
                }
            }

            try {
                await button.Channel.SendMessageAsync("Sorry but it appears I took to long to respond");
                return;
            }
            catch (Exception) {
                // We can't do anything about it so ignore it
                bot.Info("Button Handler", "Failed to notify user of error");
            }
        }
    }
}