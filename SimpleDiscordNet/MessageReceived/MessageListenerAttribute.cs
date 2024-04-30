namespace SimpleDiscordNet.MessageReceived; 

[AttributeUsage(AttributeTargets.Method)]
public class MessageListenerAttribute : Attribute {
    public MessageListenerAttribute(bool ignoreBots = true) {
        IgnoreBots = ignoreBots;
    }
    
    public bool IgnoreBots { get; }
}