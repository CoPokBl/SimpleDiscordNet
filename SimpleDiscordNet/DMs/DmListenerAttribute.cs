namespace SimpleDiscordNet.DMs; 

[AttributeUsage(AttributeTargets.Method)]
public class DmListenerAttribute : Attribute {
    public DmListenerAttribute() { }
}