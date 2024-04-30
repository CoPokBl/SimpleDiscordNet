using System.Reflection;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet.Modals; 

internal static class ModalHandler {
    internal static readonly Dictionary<string, Func<SocketModal, DiscordSocketClient, Task>> ModalHandlers = new();

    internal static void LoadModalHandlers() {
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsClass)
            .SelectMany(x => x.GetMethods())
            .Where(x => x.GetCustomAttributes(typeof(ModalListenerAttribute), false).FirstOrDefault() != null);

        foreach (MethodInfo method in methods) {
            object? obj = Activator.CreateInstance(method.DeclaringType!);
            ModalHandlers.Add(method.GetCustomAttribute<ModalListenerAttribute>()!.ModalId, (cmd, client) => (Task) method.Invoke(obj, new object[] {cmd, client})!);
        }
    }
    
    internal static async Task Submit(SocketModal modal, SimpleDiscordBot bot) {
        string modalId = modal.Data.CustomId;
        bot.Debug("Modal Handler", "Attempting to submit modal: " + modalId);

        if (!ModalHandlers.ContainsKey(modalId)) {
            bot.Error("Modal Handler", "Modal handler not found: " + modalId);
            await modal.RespondWithEmbedAsync("Modal not found", "The modal you tried to invoke does not exist.", ResponseType.Error);
            return;
        }
        
        try {
            await ModalHandlers[modalId](modal, bot.Client);
        }
        catch (Exception e) {
            bot.Error("Modal Handler", e);
            bot.Error("Modal Handler", "Submission of modal " + modalId + " failed.");
            
            // Dump info
            bot.Debug("Modal Handler", "Modal ID: " + modalId);
            bot.Debug("Modal Handler", $"Modal executor: {modal.User.Username}#{modal.User.Discriminator}");

            if (e is not TimeoutException) {
                try {
                    await modal.RespondAsync("Sorry but I couldn't execute that command ¯\\_(ツ)_/¯");
                    return;
                }
                catch (Exception) {
                    // Ignore
                }

                try {
                    await modal.ModifyOriginalResponseAsync(props => {
                        props.Content = "Sorry but I couldn't execute that command ¯\\_(ツ)_/¯";
                    });
                    return;
                }
                catch (Exception) {
                    // Ignore
                }
            }

            try {
                await modal.Channel.SendMessageAsync("Sorry but it appears I took to long to respond");
                return;
            }
            catch (Exception) {
                // We can't do anything about it so ignore it
                bot.Info("Modal Handler", "Failed to notify user of error");
            }
        }
    }
}