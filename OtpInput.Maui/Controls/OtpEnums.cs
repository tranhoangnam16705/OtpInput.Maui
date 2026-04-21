namespace OtpInput.Maui.Controls;

public enum OtpInputType
{
    Number,
    Text,
    Password
}

public enum OtpStylingMode
{
    Outlined,
    Filled,
    Underlined
}

public enum OtpInputState
{
    Normal,
    Error,
    Success
}

public class OtpCompletedEventArgs : EventArgs
{
    public string Value { get; }
    public OtpCompletedEventArgs(string value) => Value = value;
}
