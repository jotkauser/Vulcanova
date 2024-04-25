using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Vulcanova.Core.Layout.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class HighlightValueView
{
    public static readonly BindableProperty NameProperty =
        BindableProperty.Create(nameof(Name), typeof(string), typeof(HighlightValueView));

    public string Name
    {
        get => (string) GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
        
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(HighlightValueView));

    public string Value
    {
        get => (string) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty ValueAdornmentProperty =
        BindableProperty.Create(nameof(ValueAdornment), typeof(View), typeof(HighlightValueView));

    public View ValueAdornment
    {
        get => (View)GetValue(ValueAdornmentProperty);
        set => SetValue(ValueAdornmentProperty, value);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        var valueAdornment = ValueAdornment;
        if (valueAdornment != null)
        {
            SetInheritedBindingContext(valueAdornment, BindingContext);
        }
    }
    
    public HighlightValueView()
    {
        InitializeComponent();
    }
}