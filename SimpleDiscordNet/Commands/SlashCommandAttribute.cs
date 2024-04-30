using Discord;

namespace SimpleDiscordNet.Commands; 

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class SlashCommandAttribute : Attribute {
    
    public SlashCommandAttribute(string name, string description) {
        Name = name;
        Description = description;
    }
    
    public string Name { get; }
    public string Description { get; }
    public GuildPermissions? RequiredPerms { get; }
    
}