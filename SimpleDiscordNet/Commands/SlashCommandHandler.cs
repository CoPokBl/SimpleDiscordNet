using System.Reflection;
using Discord;
using Discord.WebSocket;

namespace SimpleDiscordNet.Commands; 

internal static class SlashCommandHandler {
    private static SlashCommand[]? _commands;

    internal static void LoadCommands() {
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies() // Returns all currenlty loaded assemblies
            .SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
            .Where(x => x.IsClass) // only yields classes
            .SelectMany(x => x.GetMethods()) // returns all methods defined in those classes
            .Where(x => x.GetCustomAttributes(typeof(SlashCommandAttribute), false).FirstOrDefault() != null); // returns only methods that have the InvokeAttribute

        _commands = (from method in methods
            let obj = Activator.CreateInstance(method.DeclaringType!)
            let cmdAttribute = method.GetCustomAttributes<SlashCommandAttribute>().First()
            let argumentAttributes = method.GetCustomAttributes<SlashCommandArgumentAttribute>().ToArray()
            let args = argumentAttributes
                .Select(aa => new SlashCommandArgument(aa.Name, aa.Description, aa.Required, aa.Type)).ToArray()
            select new SlashCommand {
            Name = cmdAttribute.Name, 
            Description = cmdAttribute.Description, 
            Arguments = args, 
            Function = (cmd, client) => (Task) method.Invoke(obj, new object[] {cmd, client})!,
            RequiredPermissions = cmdAttribute.RequiredPerms
        }).ToArray();
    }

    internal static async Task ExecuteCommand(SimpleDiscordBot bot, SocketSlashCommand cmdArgs) {
        bot.Debug("Slash Command Handler", "Attempting to invoke command: " + cmdArgs.CommandName);

        if (_commands!.All(cmd => cmd.Name != cmdArgs.CommandName)) {
            bot.Error("Slash Command Handler", "Command not found: " + cmdArgs.CommandName);
            await cmdArgs.RespondWithEmbedAsync("Command not found", "The command you tried to invoke does not exist.", ResponseType.Error);
            return;
        }
        
        try {
            await _commands!.Single(cmd => cmd.Name == cmdArgs.CommandName).Function(cmdArgs, bot.Client);
        }
        catch (Exception e) {
            bot.Error("Slash Command Handler", e);
            bot.Error("Slash Command Handler", "Execution of command " + cmdArgs.CommandName + " failed.");
            
            // Dump info
            bot.Debug("Slash Command Handler", "Command name: " + cmdArgs.CommandName);
            bot.Debug("Slash Command Handler", $"Command executor: {cmdArgs.User.Username}#{cmdArgs.User.Discriminator}");
            string argsString = cmdArgs.Data.Options.Aggregate("\n", (current, option) => current + $"{option.Name} ({option.Type.ToString()}) = {option.Value}\n");
            bot.Debug("Slash Command Handler", $"Arguments: {argsString}");

            if (e is not TimeoutException) {
                try {
                    await cmdArgs.RespondAsync("Sorry but I couldn't execute that command ¯\\_(ツ)_/¯");
                    return;
                }
                catch (Exception) {
                    // Ignore
                }

                try {
                    await cmdArgs.ModifyOriginalResponseAsync(props => {
                        props.Content = "Sorry but I couldn't execute that command ¯\\_(ツ)_/¯";
                    });
                    return;
                }
                catch (Exception) {
                    // Ignore
                }
            }

            try {
                await cmdArgs.Channel.SendMessageAsync("Sorry but it appears I took to long to respond");
                return;
            }
            catch (Exception) {
                // We can't do anything about it so ignore it
                bot.Info("Slash Command Handler", "Failed to notify user of error");
            }
        }
    }
    
    internal static async void UpdateCommands(SimpleDiscordBot bot) {
        DateTime startTime = DateTime.Now;
        bot.Info("Slash Command Handler", "Commencing command update");
        ApplicationCommandProperties[] commands = _commands!.Select(cmd => cmd.Build()).Cast<ApplicationCommandProperties>().ToArray();
        bot.Info("Slash Command Handler", "Sending bulk update request...");
        await bot.Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
        bot.Info("Slash Command Handler", "Command update completed in " + (DateTime.Now - startTime).TotalSeconds + " seconds");
    }

    internal static void UpdateCommand(SimpleDiscordBot bot, string cmdName) {
        SlashCommand cmd = _commands!.Single(slashCmd => slashCmd.Name == cmdName);
        bot.Debug("Slash Command Handler", "Sending command as global");
        bot.Client.CreateGlobalApplicationCommandAsync(cmd.Build()).Wait();
        bot.Info("Slash Command Handler", "Created command: " + cmd.Name);
    }

}

public enum ResponseType {
    Success,
    Error,
    Info
}