using Discord.WebSocket;

namespace SimpleDiscordNet.MessageReceived; 

public class MessageHandlerInstance {
    public Func<SocketMessage, DiscordSocketClient, Task> Function;
    public bool IgnoreBots;
}