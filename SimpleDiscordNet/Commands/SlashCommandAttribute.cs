using Discord;

namespace SimpleDiscordNet.Commands; 

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class SlashCommandAttribute : Attribute {
    
    public SlashCommandAttribute(string name, string description) {
        Name = name;
        Description = description;
    }
    
    public SlashCommandAttribute(string name, string description, GuildPermission perms) {
        Name = name;
        Description = description;
        RequiredPerms = perms;
    }
    
    public string Name { get; }
    public string Description { get; }
    public GuildPermission? RequiredPerms { get; }

}