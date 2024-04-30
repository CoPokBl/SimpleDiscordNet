using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace SimpleDiscordNet.Commands; 

internal class SlashCommand {
    
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public SlashCommandArgument[] Arguments { get; set; } = null!;
    public bool TestingCommand { get; set; }
    public GuildPermission? RequiredPermissions { get; set; }
    public Func<SocketSlashCommand, DiscordSocketClient, Task> Function { get; set; } = null!;

    public SlashCommand(
        string name, 
        string description, 
        SlashCommandArgument[] args,
        Func<SocketSlashCommand, DiscordSocketClient, Task> function,
        GuildPermission? perms = null, 
        bool testing = false) {
        Name = name;
        Description = description;
        TestingCommand = testing;
        RequiredPermissions = perms;
        Arguments = args;
        Function = function;
    }

    public SlashCommand() { }

    public SlashCommandProperties Build() {
        
        // Validate cmd
        string cmdAndOptionNameRegex = @"^[\w-]{1,32}$";
        if (!Regex.IsMatch(Name, cmdAndOptionNameRegex)) {
            throw new ArgumentException($"Command name '{Name}' is invalid. Command names must be 1-32 characters long and can only contain alphanumeric characters, underscores, and dashes.");
        }
        foreach (SlashCommandArgument arg in Arguments) {
            if (!Regex.IsMatch(arg.Name, cmdAndOptionNameRegex)) {
                throw new ArgumentException($"Command argument name '{arg.Name}' is invalid. Command argument names must be 1-32 characters long and can only contain alphanumeric characters, underscores, and dashes.");
            }
        }

        if (Description.Length > 100) {
            throw new ArgumentException($"Command description '{Description}' is too long. Command descriptions must be 100 characters or less.");
        }
        foreach (SlashCommandArgument arg in Arguments) {
            if (arg.Description.Length > 100) {
                throw new ArgumentException($"Command argument description '{arg.Description}' is too long. Command argument descriptions must be 100 characters or less.");
            }
        }

        SlashCommandBuilder builder = new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .WithDefaultMemberPermissions(RequiredPermissions);
        
        foreach (SlashCommandArgument arg in Arguments) {
            builder.AddOption(arg.Name, arg.Type, arg.Description, arg.Required);
        }
        
        return builder.Build();
    }

}