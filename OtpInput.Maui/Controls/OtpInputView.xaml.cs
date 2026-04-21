using System.Text;
using Microsoft.Maui.Controls.Shapes;

namespace OtpInput.Maui.Controls;

public partial class OtpInputView : ContentView
{
    // Zero-width space lets us detect "backspace on already-empty box" via TextChanged.
    const string ZWSP = "\u200B";
    const char EmptySlot = '\0';

    readonly List<Entry> _entries = new();
    readonly List<Border> _borders = new();
    readonly List<BoxView> _underlines = new();
    readonly List<char> _realValues = new();
    bool _suppress;
    bool _isBuilt;

    public OtpInputView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    #region Bindable Properties

    public static readonly BindableProperty LengthProperty = BindableProperty.Create(
        nameof(Length), typeof(int), typeof(OtpInputView), 6,
        propertyChanged: (b, _, _) => ((OtpInputView)b).BuildBoxes());

    public int Length
    {
        get => (int)GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(string), typeof(OtpInputView), string.Empty,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, n) => ((OtpInputView)b).OnValuePropertyChanged((string)n));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty InputTypeProperty = BindableProperty.Create(
        nameof(InputType), typeof(OtpInputType), typeof(OtpInputView), OtpInputType.Number,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyInputType());

    public OtpInputType InputType
    {
        get => (OtpInputType)GetValue(InputTypeProperty);
        set => SetValue(InputTypeProperty, value);
    }

    public static readonly BindableProperty StylingModeProperty = BindableProperty.Create(
        nameof(StylingMode), typeof(OtpStylingMode), typeof(OtpInputView), OtpStylingMode.Outlined,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public OtpStylingMode StylingMode
    {
        get => (OtpStylingMode)GetValue(StylingModeProperty);
        set => SetValue(StylingModeProperty, value);
    }

    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State), typeof(OtpInputState), typeof(OtpInputView), OtpInputState.Normal,
        propertyChanged: (b, _, n) => ((OtpInputView)b).OnStateChanged((OtpInputState)n));

    public OtpInputState State
    {
        get => (OtpInputState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(
        nameof(Spacing), typeof(double), typeof(OtpInputView), 10d,
        propertyChanged: (b, _, n) => ((OtpInputView)b).BoxesLayout.Spacing = (double)n);

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly BindableProperty BoxSizeProperty = BindableProperty.Create(
        nameof(BoxSize), typeof(double), typeof(OtpInputView), 48d,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public double BoxSize
    {
        get => (double)GetValue(BoxSizeProperty);
        set => SetValue(BoxSizeProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(double), typeof(OtpInputView), 22d,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(double), typeof(OtpInputView), 10d,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(OtpInputView), "_",
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyPlaceholder());

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty NormalBorderColorProperty = BindableProperty.Create(
        nameof(NormalBorderColor), typeof(Color), typeof(OtpInputView), Color.FromArgb("#BDBDBD"),
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public Color NormalBorderColor
    {
        get => (Color)GetValue(NormalBorderColorProperty);
        set => SetValue(NormalBorderColorProperty, value);
    }

    public static readonly BindableProperty FocusBorderColorProperty = BindableProperty.Create(
        nameof(FocusBorderColor), typeof(Color), typeof(OtpInputView), Color.FromArgb("#512BD4"),
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public Color FocusBorderColor
    {
        get => (Color)GetValue(FocusBorderColorProperty);
        set => SetValue(FocusBorderColorProperty, value);
    }

    public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create(
        nameof(ErrorColor), typeof(Color), typeof(OtpInputView), Color.FromArgb("#E53935"),
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public Color ErrorColor
    {
        get => (Color)GetValue(ErrorColorProperty);
        set => SetValue(ErrorColorProperty, value);
    }

    public static readonly BindableProperty SuccessColorProperty = BindableProperty.Create(
        nameof(SuccessColor), typeof(Color), typeof(OtpInputView), Color.FromArgb("#43A047"),
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public Color SuccessColor
    {
        get => (Color)GetValue(SuccessColorProperty);
        set => SetValue(SuccessColorProperty, value);
    }

    public static readonly BindableProperty BoxBackgroundColorProperty = BindableProperty.Create(
        nameof(BoxBackgroundColor), typeof(Color), typeof(OtpInputView), Colors.Transparent,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public Color BoxBackgroundColor
    {
        get => (Color)GetValue(BoxBackgroundColorProperty);
        set => SetValue(BoxBackgroundColorProperty, value);
    }

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(OtpInputView), Colors.Black,
        propertyChanged: (b, _, _) => ((OtpInputView)b).ApplyStyling());

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty AutoFocusProperty = BindableProperty.Create(
        nameof(AutoFocus), typeof(bool), typeof(OtpInputView), true);

    public bool AutoFocus
    {
        get => (bool)GetValue(AutoFocusProperty);
        set => SetValue(AutoFocusProperty, value);
    }

    #endregion

    public event EventHandler<OtpCompletedEventArgs>? Completed;

    public void FocusFirst() => FocusIndex(0);

    public void Clear()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            _realValues[i] = EmptySlot;
            SetTextSilently(_entries[i], ZWSP);
        }
        UpdateValueFromSlots();
        FocusIndex(0);
    }

    #region Build / lifecycle

    void OnLoaded(object? sender, EventArgs e)
    {
        if (!_isBuilt) BuildBoxes();
        if (AutoFocus)
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(250), () => FocusIndex(0));
    }

    void BuildBoxes()
    {
        if (BoxesLayout == null) return;

        foreach (var entry in _entries)
        {
            entry.TextChanged -= OnEntryTextChanged;
            entry.Focused -= OnEntryFocused;
            entry.Unfocused -= OnEntryUnfocused;
            entry.HandlerChanged -= OnEntryHandlerChanged;
        }

        BoxesLayout.Children.Clear();
        _entries.Clear();
        _borders.Clear();
        _underlines.Clear();
        _realValues.Clear();

        int len = Math.Max(1, Length);

        for (int i = 0; i < len; i++)
        {
            var entry = new Entry
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.Transparent,
                MaxLength = 64, // allow paste
                Text = ZWSP,
            };
            entry.TextChanged += OnEntryTextChanged;
            entry.Focused += OnEntryFocused;
            entry.Unfocused += OnEntryUnfocused;
            entry.HandlerChanged += OnEntryHandlerChanged;

            var underline = new BoxView
            {
                HeightRequest = 2,
                VerticalOptions = LayoutOptions.End,
                IsVisible = false,
            };

            var inner = new Grid();
            inner.Children.Add(entry);
            inner.Children.Add(underline);

            var border = new Border
            {
                Content = inner,
                Padding = 0,
                StrokeThickness = 1.5,
                StrokeShape = new RoundRectangle { CornerRadius = (float)CornerRadius },
            };

            _entries.Add(entry);
            _borders.Add(border);
            _underlines.Add(underline);
            _realValues.Add(EmptySlot);

            BoxesLayout.Children.Add(border);
        }

        _isBuilt = true;

        ApplyInputType();
        ApplyPlaceholder();
        ApplyStyling();
        OnValuePropertyChanged(Value ?? string.Empty);
    }

    #endregion

    #region Styling

    void ApplyStyling()
    {
        if (!_isBuilt) return;

        for (int i = 0; i < _entries.Count; i++)
        {
            var border = _borders[i];
            var entry = _entries[i];
            var underline = _underlines[i];
            bool isFocused = entry.IsFocused;

            border.WidthRequest = BoxSize;
            border.HeightRequest = BoxSize;
            border.StrokeShape = new RoundRectangle { CornerRadius = (float)CornerRadius };
            entry.FontSize = FontSize;
            entry.TextColor = TextColor;
            entry.PlaceholderColor = NormalBorderColor;

            var activeColor = GetStateColor(isFocused);

            switch (StylingMode)
            {
                case OtpStylingMode.Outlined:
                    border.Stroke = activeColor;
                    border.StrokeThickness = isFocused ? 2 : 1.5;
                    border.BackgroundColor = BoxBackgroundColor;
                    underline.IsVisible = false;
                    break;

                case OtpStylingMode.Filled:
                    border.Stroke = isFocused ? activeColor : Colors.Transparent;
                    border.StrokeThickness = isFocused ? 2 : 0;
                    border.BackgroundColor = BoxBackgroundColor == Colors.Transparent
                        ? Color.FromArgb("#F1F1F5")
                        : BoxBackgroundColor;
                    underline.IsVisible = false;
                    break;

                case OtpStylingMode.Underlined:
                    border.Stroke = Colors.Transparent;
                    border.StrokeThickness = 0;
                    border.BackgroundColor = Colors.Transparent;
                    underline.IsVisible = true;
                    underline.Color = activeColor;
                    underline.HeightRequest = isFocused ? 3 : 2;
                    break;
            }
        }
    }

    Color GetStateColor(bool isFocused)
    {
        return State switch
        {
            OtpInputState.Error => ErrorColor,
            OtpInputState.Success => SuccessColor,
            _ => isFocused ? FocusBorderColor : NormalBorderColor,
        };
    }

    void ApplyInputType()
    {
        if (!_isBuilt) return;
        foreach (var e in _entries)
        {
            switch (InputType)
            {
                case OtpInputType.Number:
                    e.Keyboard = Keyboard.Numeric;
                    e.IsPassword = false;
                    break;
                case OtpInputType.Text:
                    e.Keyboard = Keyboard.Default;
                    e.IsPassword = false;
                    break;
                case OtpInputType.Password:
                    e.Keyboard = Keyboard.Numeric;
                    // We mask manually so the ZWSP sentinel is not rendered as a bullet.
                    e.IsPassword = false;
                    break;
            }
        }
        // Refresh rendered chars to match new input type.
        for (int i = 0; i < _entries.Count; i++)
            RenderSlot(i);
    }

    void ApplyPlaceholder()
    {
        if (!_isBuilt) return;
        var raw = Placeholder ?? string.Empty;
        for (int i = 0; i < _entries.Count; i++)
        {
            string ph = i < raw.Length ? raw[i].ToString() : raw.Length > 0 ? raw[^1].ToString() : string.Empty;
            _entries[i].Placeholder = ph;
        }
    }

    #endregion

    #region State & animations

    void OnStateChanged(OtpInputState state)
    {
        ApplyStyling();
        if (state == OtpInputState.Error)
            _ = ShakeAsync();
    }

    async Task ShakeAsync()
    {
        if (!_isBuilt) return;
        const uint duration = 50;
        for (int cycle = 0; cycle < 2; cycle++)
        {
            await BoxesLayout.TranslateToAsync(-8, 0, duration);
            await BoxesLayout.TranslateToAsync(8, 0, duration);
        }
        await BoxesLayout.TranslateToAsync(0, 0, duration);
    }

    async Task AnimateFocusAsync(VisualElement target)
    {
        await target.ScaleToAsync(1.06, 90, Easing.CubicOut);
        await target.ScaleToAsync(1.0, 90, Easing.CubicIn);
    }

    #endregion

    #region Focus

    void FocusIndex(int index)
    {
        if (index < 0 || index >= _entries.Count) return;
        var entry = _entries[index];
        entry.Focus();
    }

    void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            int idx = _entries.IndexOf(entry);
            if (idx >= 0)
            {
                _ = AnimateFocusAsync(_borders[idx]);
                ApplyStyling();
            }
        }
    }

    void OnEntryUnfocused(object? sender, FocusEventArgs e) => ApplyStyling();

    static void OnEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is not Entry entry || entry.Handler?.PlatformView is not { } platformView) return;

#if ANDROID
        if (platformView is Android.Widget.EditText editText)
        {
            // Remove the material underline and default tint; keep only our Border/BoxView chrome.
            editText.Background = null;
            editText.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(
                Android.Graphics.Color.Transparent);
            editText.SetPadding(0, 0, 0, 0);
        }
#elif IOS || MACCATALYST
        if (platformView is UIKit.UITextField textField)
        {
            // Drop the rounded/line border so only our Border/BoxView chrome shows.
            textField.BorderStyle = UIKit.UITextBorderStyle.None;
            textField.Layer.BorderWidth = 0;
            textField.Layer.MasksToBounds = true;
        }
#endif
    }

    #endregion

    #region Input handling

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppress || sender is not Entry entry) return;
        int index = _entries.IndexOf(entry);
        if (index < 0) return;

        string newText = e.NewTextValue ?? string.Empty;
        string oldText = e.OldTextValue ?? string.Empty;

        // Strip all ZWSP characters to get the pure user input.
        string user = newText.Replace(ZWSP, string.Empty);

        // All native-text mutations are deferred to the next UI pump.
        // Writing to Entry.Text synchronously from inside TextChanged crashes Android
        // ("end should be < than charSequence length") because the underlying EditText
        // is still in the middle of applying the user's keystroke when we re-enter it.

        if (user.Length > 1)
        {
            Dispatcher.Dispatch(() => HandlePaste(index, user));
            return;
        }

        if (user.Length == 1)
        {
            char c = user[0];
            if (!IsCharValid(c))
            {
                Dispatcher.Dispatch(() => RenderSlot(index));
                return;
            }

            _realValues[index] = c;
            Dispatcher.Dispatch(() =>
            {
                RenderSlot(index);
                UpdateValueFromSlots();

                if (index < _entries.Count - 1)
                    FocusIndex(index + 1);
                else
                    entry.Unfocus();

                RaiseCompletedIfFull();
            });
            return;
        }

        // user.Length == 0 ⇒ user deleted the ZWSP sentinel (backspace on already-empty box).
        if (newText.Length == 0 && oldText.Length <= 1)
        {
            bool hadValue = _realValues[index] != EmptySlot;
            _realValues[index] = EmptySlot;
            bool goBack = !hadValue && index > 0;
            if (goBack) _realValues[index - 1] = EmptySlot;

            Dispatcher.Dispatch(() =>
            {
                RenderSlot(index);
                if (goBack)
                {
                    RenderSlot(index - 1);
                    FocusIndex(index - 1);
                }
                UpdateValueFromSlots();
            });
        }
    }

    void HandlePaste(int startIndex, string pasted)
    {
        int i = startIndex;
        foreach (var raw in pasted)
        {
            if (i >= _entries.Count) break;
            if (!IsCharValid(raw)) continue;
            _realValues[i] = raw;
            RenderSlot(i);
            i++;
        }
        // Clear leftover boxes that follow the paste? Leave existing content alone.
        // Repaint the start index in case nothing got pasted.
        RenderSlot(startIndex);

        UpdateValueFromSlots();

        int nextFocus = Math.Min(i, _entries.Count - 1);
        FocusIndex(nextFocus);
        if (i >= _entries.Count) _entries[^1].Unfocus();

        RaiseCompletedIfFull();
    }

    bool IsCharValid(char c) => InputType switch
    {
        OtpInputType.Number => char.IsDigit(c),
        OtpInputType.Text => char.IsLetterOrDigit(c),
        OtpInputType.Password => char.IsDigit(c),
        _ => true
    };

    void RenderSlot(int i)
    {
        if (i < 0 || i >= _entries.Count) return;
        var entry = _entries[i];
        char c = _realValues[i];
        string display;
        if (c == EmptySlot)
            display = ZWSP;
        else if (InputType == OtpInputType.Password)
            display = ZWSP + "\u2022"; // bullet
        else
            display = ZWSP + c;

        SetTextSilently(entry, display);
    }

    void SetTextSilently(Entry entry, string value)
    {
        // No-op writes still round-trip through the native EditText on Android and can
        // nudge the Spannable into an invalid selection state. Skip when unchanged.
        if (string.Equals(entry.Text, value, StringComparison.Ordinal)) return;

        _suppress = true;
        try
        {
            entry.Text = value;
            var len = value?.Length ?? 0;
            if (entry.CursorPosition > len) entry.CursorPosition = len;
        }
        finally { _suppress = false; }
    }

    void UpdateValueFromSlots()
    {
        var sb = new StringBuilder(_realValues.Count);
        foreach (var c in _realValues)
        {
            if (c == EmptySlot) break;
            sb.Append(c);
        }
        var combined = sb.ToString();
        _suppress = true;
        try { Value = combined; }
        finally { _suppress = false; }
    }

    void OnValuePropertyChanged(string newValue)
    {
        if (_suppress || !_isBuilt) return;

        newValue ??= string.Empty;
        for (int i = 0; i < _entries.Count; i++)
        {
            char c = i < newValue.Length ? newValue[i] : EmptySlot;
            if (c != EmptySlot && !IsCharValid(c)) c = EmptySlot;
            _realValues[i] = c;
            RenderSlot(i);
        }
        RaiseCompletedIfFull();
    }

    void RaiseCompletedIfFull()
    {
        foreach (var c in _realValues)
            if (c == EmptySlot) return;

        Completed?.Invoke(this, new OtpCompletedEventArgs(new string(_realValues.ToArray())));
    }

    #endregion
}
