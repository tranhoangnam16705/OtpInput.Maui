using OtpInput.Maui.Controls;

namespace OtpInput.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void OnOtpCompleted(object? sender, OtpCompletedEventArgs e)
        {
            // UI-only hook; business logic lives in the ViewModel's Verify command.
            SemanticScreenReader.Announce($"Code {e.Value} entered.");
        }
    }
}
