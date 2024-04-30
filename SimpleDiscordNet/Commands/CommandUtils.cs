using Discord;
using Discord.WebSocket;

namespace SimpleDiscordNet.Commands; 

public static class CommandUtils {
    
    public static T? GetArgument<T>(this SocketSlashCommand self, string name) {
        IEnumerable<SocketSlashCommandDataOption> args = self.Data.Options.Where(option => option.Name == name);
        if (!args.Any()) {
            return default;
        }
        return (T) self.Data.Options.Single(option => option.Name == name).Value;
    }
    
    public static Embed GetEmbed(string title, string body, ResponseType type, EmbedFooterBuilder? footer = null) {
        Color color = type switch {
            ResponseType.Success => Color.Green,
            ResponseType.Error => Color.Red,
            ResponseType.Info => Color.Blue,
            _ => Color.Blue
        };

        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(body)
            .WithColor(color);

        if (footer != null) {
            builder.WithFooter(footer);
        }

        return builder.Build();
    }

    public static async Task RespondWithEmbedAsync(this SocketSlashCommand self, string title, string body, ResponseType type = ResponseType.Info, 
        IMessageComponent[]? components = null, bool ephemeral = false, EmbedFooterBuilder? footer = null) {
        components ??= Array.Empty<IMessageComponent>();
        ComponentBuilder componentBuilder = ComponentBuilder.FromComponents(components);
        await self.RespondAsync(embed: GetEmbed(title, body, type, footer), components: componentBuilder.Build(), ephemeral: ephemeral);
    }
    
    public static async Task RespondWithEmbedAsyncModal(this SocketModal self, string title, string body, ResponseType type = ResponseType.Info, 
        ComponentBuilder? components = null) {
        components ??= new ComponentBuilder();
        await self.RespondAsync(embed: GetEmbed(title, body, type), components: components.Build());
    }
    
    public static async Task ModifyWithEmbedAsync(this SocketSlashCommand self, string title, string body, ResponseType type = ResponseType.Info) {
        await self.ModifyOriginalResponseAsync(msg => msg.Embed = GetEmbed(title, body, type));
    }

    public static async Task ModifyBodyTextAsync(this SocketSlashCommand self, string body) {
        await self.ModifyOriginalResponseAsync(msg => msg.Content = body);
    }

    public static async Task RespondWithUsageAsync(this SocketSlashCommand self, string usage) {
        await self.RespondWithEmbedAsync("Usage", usage, ResponseType.Error);
    }
    
}