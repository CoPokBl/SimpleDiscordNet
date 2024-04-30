namespace SimpleDiscordNet.Buttons; 

/// <summary>
/// The function that this attribute decorates will be called when the button that is specified in the constructor is clicked.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ButtonListenerAttribute : Attribute {
    
    /// <summary>
    /// Creates a new instance of the ButtonListenerAttribute class.
    /// </summary>
    /// <param name="buttonId">The button to listen to click events for.</param>
    public ButtonListenerAttribute(string buttonId) {
        ButtonId = buttonId;
    }
    
    /// <summary>
    /// The ID of the button that is being listened for.
    /// </summary>
    public string ButtonId { get; }
    
}