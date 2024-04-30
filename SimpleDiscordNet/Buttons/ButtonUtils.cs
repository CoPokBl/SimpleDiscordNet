using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet.Buttons; 

public static class ModalUtils {

    private static Embed GetEmbed(string title, string body, ResponseType type) {
        Color color = type switch {
            ResponseType.Success => Color.Green,
            ResponseType.Error => Color.Red,
            ResponseType.Info => Color.Blue,
            _ => Color.Blue
        };
        
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(body)
            .WithColor(color)
            .Build();
    }

    public static async Task RespondWithEmbedAsync(this SocketMessageComponent self, string title, string body, ResponseType type = ResponseType.Info, 
        IMessageComponent[]? components = null, bool ephemeral = false) {
        components ??= Array.Empty<IMessageComponent>();
        ComponentBuilder componentBuilder = ComponentBuilder.FromComponents(components);
        await self.RespondAsync(embed: GetEmbed(title, body, type), components: componentBuilder.Build(), ephemeral: ephemeral);
    }

    public static async Task ModifyWithEmbedAsync(this SocketMessageComponent self, string title, string body, ResponseType type = ResponseType.Info) {
        await self.ModifyOriginalResponseAsync(msg => msg.Embed = GetEmbed(title, body, type));
    }

    public static async Task ModifyBodyTextAsync(this SocketMessageComponent self, string body) {
        await self.ModifyOriginalResponseAsync(msg => msg.Content = body);
    }

    public static async Task RespondWithUsageAsync(this SocketMessageComponent self, string usage) {
        await self.RespondWithEmbedAsync("Usage", usage, ResponseType.Error);
    }
    
}