using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace SimpleDiscordNet.Modals; 

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

    public static async Task RespondWithEmbedAsync(this SocketModal self, string title, string body, ResponseType type = ResponseType.Info, 
        IMessageComponent[]? components = null, bool ephemeral = false) {
        components ??= Array.Empty<IMessageComponent>();
        ComponentBuilder componentBuilder = ComponentBuilder.FromComponents(components);
        await self.RespondAsync(embed: GetEmbed(title, body, type), components: componentBuilder.Build(), ephemeral: ephemeral);
    }

    public static async Task ModifyWithEmbedAsync(this SocketModal self, string title, string body, ResponseType type = ResponseType.Info) {
        await self.ModifyOriginalResponseAsync(msg => msg.Embed = GetEmbed(title, body, type));
    }

    public static async Task ModifyBodyTextAsync(this SocketModal self, string body) {
        await self.ModifyOriginalResponseAsync(msg => msg.Content = body);
    }

    public static async Task RespondWithUsageAsync(this SocketModal self, string usage) {
        await self.RespondWithEmbedAsync("Usage", usage, ResponseType.Error);
    }

    public static string GetInput(this SocketModal self, string name) {
        if (self.Data.Components.All(s => s.CustomId != name)) {
            throw new ArgumentException("No component with that name exists");
        }
        return self.Data.Components.First(s => s.CustomId == name).Value;
    }

}