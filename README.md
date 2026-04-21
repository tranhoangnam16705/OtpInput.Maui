# OtpInputView

A reusable, MVVM-friendly OTP (One-Time Password) input control for .NET MAUI, inspired by Syncfusion `SfOtpInput`. Works on Android and iOS out of the box.

## Features

- Configurable digit count (`Length`)
- Auto-advance focus on type, auto-step-back on backspace (including on an already-empty box)
- Multi-character paste distribution across boxes
- Three styling modes: **Outlined**, **Filled**, **Underlined**
- Three input types: **Number**, **Text**, **Password** (masked)
- Three visual states: **Normal**, **Error**, **Success**
- Built-in focus scale animation and error shake animation
- Auto-focus first box on page load
- Two-way `Value` binding
- `Completed` event when all slots are filled
- Native `Entry` underline/border stripped on Android and iOS — only the control's own chrome shows

## Quick start

### 1. Declare the namespace

```xml
xmlns:controls="clr-namespace:OtpInput.Maui.Controls"
```

### 2. Use in XAML

```xml
<controls:OtpInputView
    Length="6"
    Value="{Binding Otp}"
    State="{Binding State}"
    InputType="Number"
    StylingMode="Outlined"
    AutoFocus="True"
    BoxSize="52"
    FontSize="24"
    CornerRadius="12"
    Spacing="10"
    Placeholder="------"
    Completed="OnOtpCompleted" />
```

### 3. Minimal ViewModel

```csharp
public class LoginViewModel : INotifyPropertyChanged
{
    string _otp = "";
    OtpInputState _state = OtpInputState.Normal;

    public string Otp
    {
        get => _otp;
        set { _otp = value; OnPropertyChanged(); }
    }

    public OtpInputState State
    {
        get => _state;
        set { _state = value; OnPropertyChanged(); }
    }

    // ... INotifyPropertyChanged boilerplate
}
```

## Bindable properties

| Property              | Type              | Default        | Description                                              |
| --------------------- | ----------------- | -------------- | -------------------------------------------------------- |
| `Length`              | `int`             | `6`            | Number of input boxes.                                   |
| `Value`               | `string`          | `""`           | Combined OTP string. `TwoWay` by default.                |
| `InputType`           | `OtpInputType`    | `Number`       | `Number` / `Text` / `Password`.                          |
| `StylingMode`         | `OtpStylingMode`  | `Outlined`     | `Outlined` / `Filled` / `Underlined`.                    |
| `State`               | `OtpInputState`   | `Normal`       | `Normal` / `Error` / `Success`. Drives color + shake.    |
| `Placeholder`         | `string`          | `"_"`          | Per-box placeholder. String is distributed char-by-char. |
| `Spacing`             | `double`          | `10`           | Gap between boxes.                                       |
| `BoxSize`             | `double`          | `48`           | Width & height of each box (dp).                         |
| `FontSize`            | `double`          | `22`           | Font size of the displayed digit.                        |
| `CornerRadius`        | `double`          | `10`           | Corner radius of each box.                               |
| `NormalBorderColor`   | `Color`           | `#BDBDBD`      | Border color in `Normal` state, unfocused.               |
| `FocusBorderColor`    | `Color`           | `#512BD4`      | Border color when a box is focused.                      |
| `ErrorColor`          | `Color`           | `#E53935`      | Border color in `Error` state.                           |
| `SuccessColor`        | `Color`           | `#43A047`      | Border color in `Success` state.                         |
| `BoxBackgroundColor`  | `Color`           | `Transparent`  | Fill color of each box. Filled mode falls back to `#F1F1F5` when transparent. |
| `TextColor`           | `Color`           | `Black`        | Color of the displayed digit.                            |
| `AutoFocus`           | `bool`            | `true`         | Focus the first box when the control loads.              |

## Events

| Event       | Args                      | When it fires                                      |
| ----------- | ------------------------- | -------------------------------------------------- |
| `Completed` | `OtpCompletedEventArgs`   | Every slot is filled. `Args.Value` holds the OTP.  |

## Public methods

| Method         | Purpose                                               |
| -------------- | ----------------------------------------------------- |
| `FocusFirst()` | Programmatically focus the first box.                 |
| `Clear()`      | Reset all boxes, clear `Value`, focus the first box.  |

## Enums

```csharp
public enum OtpInputType   { Number, Text, Password }
public enum OtpStylingMode { Outlined, Filled, Underlined }
public enum OtpInputState  { Normal, Error, Success }
```

### Input type behavior

| Type       | Accepted chars      | Keyboard | Masking |
| ---------- | ------------------- | -------- | ------- |
| `Number`   | `0-9`               | Numeric  | No      |
| `Text`     | letters + digits    | Default  | No      |
| `Password` | `0-9`               | Numeric  | Yes (`•`) |

## Full MVVM example

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:controls="clr-namespace:OtpInput.Maui.Controls"
    xmlns:vm="clr-namespace:OtpInput.Maui.ViewModels"
    x:Class="OtpInput.Maui.MainPage">

    <ContentPage.BindingContext>
        <vm:OtpDemoViewModel />
    </ContentPage.BindingContext>

    <VerticalStackLayout Padding="24" Spacing="24">
        <Label Text="{Binding Status}" HorizontalTextAlignment="Center" />

        <controls:OtpInputView
            Length="6"
            Value="{Binding Otp}"
            State="{Binding State}"
            InputType="Number"
            StylingMode="Outlined"
            Completed="OnOtpCompleted" />

        <Button Text="Verify" Command="{Binding VerifyCommand}" />
        <Button Text="Reset"  Command="{Binding ResetCommand}" />
    </VerticalStackLayout>
</ContentPage>
```

```csharp
void OnOtpCompleted(object? sender, OtpCompletedEventArgs e)
{
    // Optional UI-only hook. Business logic belongs in the ViewModel.
    SemanticScreenReader.Announce($"Code {e.Value} entered.");
}
```

## Styling cookbook

### Outlined (default)
```xml
<controls:OtpInputView StylingMode="Outlined" CornerRadius="12" />
```

### Filled
```xml
<controls:OtpInputView
    StylingMode="Filled"
    BoxBackgroundColor="#F1F1F5"
    CornerRadius="10" />
```

### Underlined
```xml
<controls:OtpInputView
    StylingMode="Underlined"
    BoxSize="40"
    FontSize="20" />
```

### Error + shake
Set `State="Error"`. The control automatically:
- Switches all border/underline colors to `ErrorColor`.
- Plays a horizontal shake animation on the row of boxes.

```csharp
vm.State = OtpInputState.Error; // triggers shake
```

## Platform notes

### Android — `IllegalArgumentException` safety
Writing `Entry.Text` synchronously from inside `TextChanged` re-enters the underlying `EditText` while it is still applying the current keystroke, which triggers `java.lang.IllegalArgumentException: end should be < than charSequence length` in `Spannable.setSpan`. The control sidesteps this by deferring every slot mutation through `Dispatcher.Dispatch`, and by skipping no-op `Text` writes.

### Stripping native Entry chrome
Each `Entry`'s `HandlerChanged` hook removes the default platform chrome so only the control's own `Border`/`BoxView` is visible:

- **Android**: `EditText.Background = null`, `BackgroundTintList = Transparent`, zero padding.
- **iOS**: `UITextField.BorderStyle = None`, `Layer.BorderWidth = 0`.

### Backspace-on-empty detection
Each box is pre-seeded with a zero-width space (`\u200B`). When the user presses backspace on a visually-empty box, the ZWSP gets deleted and `TextChanged` fires with an empty string — that is the signal to step focus back. In `Password` mode, the ZWSP is **not** exposed through `Entry.IsPassword`; the control paints `•` manually so the sentinel does not render as a bullet.

## File layout

```
Controls/
├── OtpEnums.cs            # OtpInputType, OtpStylingMode, OtpInputState, OtpCompletedEventArgs
├── OtpInputView.xaml      # ContentView host (HorizontalStackLayout)
├── OtpInputView.xaml.cs   # Bindable properties, input logic, styling, animations
└── README.md              # this file
```

## Supported TFMs

- `net10.0-android`
- `net10.0-ios`

## Roadmap ideas

- SMS auto-fill (iOS `OneTimeCode`, Android SMS Retriever API)
- Resend countdown + `ResendCommand`
- `IsEnabled` / `IsReadOnly` binding
- `CompletedCommand` / `ValueChangedCommand` for pure-XAML MVVM
- `ErrorMessage` / `HelperText` label beneath the row
- Haptic feedback on digit entry, error, and completion
- RTL (`FlowDirection`) support
- Digit grouping separator (e.g. `123-456`)
- Custom char validator (`Func<char, bool>`)
- `VisualStateManager` integration in place of imperative styling
