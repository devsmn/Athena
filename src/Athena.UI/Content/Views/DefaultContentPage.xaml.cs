using Syncfusion.Maui.Popup;

namespace Athena.UI;

public partial class DefaultContentPage : ContentPage
{
    protected bool Initialized;

    public TaskCompletionSource DoneTcs { get; private set; }
    public TaskCompletionSource<object> DoneWithResult { get; private set; }

    public DefaultContentPage()
    {
        DoneTcs = new();
        DoneWithResult = new();
        InitializeComponent();
        NavigatedTo += OnNavigatedTo;
    }

    protected virtual async void OnNavigatedTo(object sender, NavigatedToEventArgs e)
    {
        if (!Initialized && BindingContext is ContextViewModel vm)
        {
            _ = vm.InitializeAsync();
            Initialized = true;
        }
    }

    public void ShowInfoPopup(string caption, string text)
    {
        SfPopup popup = new()
        {
            HeaderTitle = caption, AcceptButtonText = "Ok", ShowFooter = true, ShowCloseButton = true,
            AutoSizeMode = PopupAutoSizeMode.Height
        };

        DataTemplate template = new(() =>
        {
            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Label label = new()
            {
                Text = text,
                Margin = new Thickness(5),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                LineBreakMode = LineBreakMode.WordWrap
            };

            grid.Add(label);
            return grid;
        });

        popup.ContentTemplate = template;
        popup.Show();
    }
}
