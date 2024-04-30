using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Buttons;
using SimpleDiscordNet.Commands;
using SimpleDiscordNet.DMs;
using SimpleDiscordNet.MessageReceived;
using SimpleDiscordNet.Modals;

namespace SimpleDiscordNet; 

public class SimpleDiscordBot {

    /// <summary>
    /// The socket client allowing you to interact with Discord.
    /// </summary>
    public readonly DiscordSocketClient Client;

    /// <summary>
    /// Function that is fired whenever a new message is logged.
    /// </summary>
    public event Func<LogMessage, Task> Log;

    /// <summary>
    /// Creates a new instance of the SimpleDiscordBot class.
    /// </summary>
    /// <param name="token">The Discord bot token to use to authenticate.</param>
    public SimpleDiscordBot(string token) {
        Log += VoidLog;
        Client = new DiscordSocketClient();

        Login(token);
        
        Info("Bot Initializer", "Loading handlers...");
        ModalHandler.LoadModalHandlers();
        SlashCommandHandler.LoadCommands();
        DmHandler.LoadDmHandlers();
        MessageHandler.LoadMessageHandlers();
        ButtonHandler.LoadButtonHandlers();
        Info("Bot Initializer", "Loaded handlers!");

        Client.Log += LogFunc;
        Client.SlashCommandExecuted += SlashCommandExecuted;
        Client.ModalSubmitted += ModalSubmitted;
        Client.MessageReceived += MessageReceived;
        Client.ButtonExecuted += ButtonExecuted;
    }

    /// <summary>
    /// Waits for the bot to stop, also registers ctrl + c binding
    /// </summary>
    public async Task WaitAsync() {
        Debug("Bot", "Loading waiter");
        CancellationTokenSource source = new();
        Console.CancelKeyPress += async (sender, args) => {
            Info("Bot", "Stop requested");
            await Client.StopAsync();
            source.Cancel();
        };
        try {
            Debug("Bot", "Started Waiting");
            await Task.Delay(-1, source.Token);
            Info("Bot", "Wait Ended Unexpectedly");
        }
        catch (TaskCanceledException) {
            Info("Bot", "Bot stopped");
        }
    }

    /// <summary>
    /// Waits for the bot to stop, also registers ctrl + c binding
    /// </summary>
    public void Wait() {
        WaitAsync().Wait();
    }

    private Task VoidLog(LogMessage msg) => Task.CompletedTask;

    private Task LogFunc(LogMessage msg) {
        return Log(msg);
    }

    private async void Login(string token) {
        await Client.LoginAsync(TokenType.Bot, token);
    }

    /// <summary>
    /// Run the bot
    /// </summary>
    public async Task StartBot() {
        await Client.StartAsync();
    }

    /// <summary>
    /// Send an update to Discord for all slash commands
    /// </summary>
    public void UpdateCommands() {
        SlashCommandHandler.UpdateCommands(this);
    }

    /// <summary>
    /// Send an update to Discord for the specified slash command
    /// </summary>
    /// <param name="cmd">The command to send an update for</param>
    public void UpdateCommand(string cmd) {
        SlashCommandHandler.UpdateCommand(this, cmd);
    }
    
    private Task SlashCommandExecuted(SocketSlashCommand arg) {
        return SlashCommandHandler.ExecuteCommand(this, arg);
    }
    
    private Task ModalSubmitted(SocketModal arg) {
        return ModalHandler.Submit(arg, this);
    }
    
    private Task ButtonExecuted(SocketMessageComponent arg) {
        return ButtonHandler.Submit(arg, this);
    }
    
    private Task MessageReceived(SocketMessage msg) {
        return msg.Channel is IDMChannel ? DmHandler.Run(msg, this) : MessageHandler.Run(msg, this);
    }

    internal void Debug(string src, object msg) {
        Log(new LogMessage(LogSeverity.Debug, src, msg.ToString()));
    }

    internal void Info(string src, object msg) {
        Log(new LogMessage(LogSeverity.Info, src, msg.ToString()));
    }

    internal void Error(string src, object msg) {
        if (msg is Exception exception) {
            Log(new LogMessage(LogSeverity.Error, src, exception.ToString(), exception));
        }
        else {
            Log(new LogMessage(LogSeverity.Error, src, msg.ToString()));
        }
    }
    
}