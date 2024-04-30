
using SimpleDiscordNet;

Console.WriteLine("Hello, World!");

// TODO: REMOVE BOT TOKEN BEFORE COMMIT
SimpleDiscordBot bot = new("");
bot.Log += log => {
    Console.WriteLine(log.ToString());
    return Task.CompletedTask;
};

await bot.StartBot();

bot.Client.Ready += () => {
    //bot.UpdateCommand("lol");
    return Task.CompletedTask;
};

bot.Wait();
Console.WriteLine("Bye!");