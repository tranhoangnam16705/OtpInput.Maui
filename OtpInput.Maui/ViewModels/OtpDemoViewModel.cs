using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OtpInput.Maui.Controls;

namespace OtpInput.Maui.ViewModels;

public class OtpDemoViewModel : INotifyPropertyChanged
{
    string _otp = string.Empty;
    string _status = "Enter the 6-digit code sent to your device.";
    OtpInputState _state = OtpInputState.Normal;

    public OtpDemoViewModel()
    {
        VerifyCommand = new Command(Verify, () => Otp.Length == 6);
        ResetCommand = new Command(Reset);
    }

    public string Otp
    {
        get => _otp;
        set
        {
            if (SetField(ref _otp, value ?? string.Empty))
            {
                if (State != OtpInputState.Normal) State = OtpInputState.Normal;
                (VerifyCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public OtpInputState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public ICommand VerifyCommand { get; }
    public ICommand ResetCommand { get; }

    void Verify()
    {
        // Toy validation: "123456" passes.
        if (Otp == "123456")
        {
            State = OtpInputState.Success;
            Status = "Verified successfully.";
        }
        else
        {
            State = OtpInputState.Error;
            Status = "Incorrect code. Please try again.";
        }
    }

    void Reset()
    {
        Otp = string.Empty;
        State = OtpInputState.Normal;
        Status = "Enter the 6-digit code sent to your device.";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
