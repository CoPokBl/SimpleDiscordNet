using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.DMs;
using SimpleDiscordNet.MessageReceived;
using SimpleDiscordNet.Modals;

namespace Testing; 

public class Commands {

    [SlashCommand("lol", "Does the lols")]
    public Task Lol(SocketSlashCommand cmd, DiscordSocketClient client) {
        ModalBuilder builder = new ModalBuilder("HI", "test").AddTextInput("Name", "name");
        return cmd.RespondWithModalAsync(builder.Build());
    }

    [ModalListener("test")]
    public Task TestModal(SocketModal modal, DiscordSocketClient client) {
        string name = modal.GetInput("name");
        return modal.RespondWithEmbedAsync("HI!", $"Hello {name}", ResponseType.Success);
    }

    [DmListener]
    [MessageListener(false)]
    public Task MessageListen(SocketMessage msg, DiscordSocketClient client) {
        return msg.Channel.SendMessageAsync("HI!");
    }

}