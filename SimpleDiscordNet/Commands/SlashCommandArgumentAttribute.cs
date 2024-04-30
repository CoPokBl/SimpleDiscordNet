using Discord;

namespace SimpleDiscordNet.Commands; 

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class SlashCommandArgumentAttribute : Attribute {
    
    public SlashCommandArgumentAttribute(string name, string description, bool required, ApplicationCommandOptionType type) {
        Name = name;
        Description = description;
        Required = required;
        Type = type;
    }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Required { get; set; }
    public ApplicationCommandOptionType Type { get; set; }
    
}