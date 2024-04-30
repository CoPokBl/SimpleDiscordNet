namespace SimpleDiscordNet.Modals; 

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ModalListenerAttribute : Attribute {
    
    public ModalListenerAttribute(string modalId) {
        ModalId = modalId;
    }
    
    public string ModalId { get; }
    
}