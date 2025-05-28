using Syncfusion.Maui.Popup;

namespace Athena.UI;

public partial class DefaultContentPage : ContentPage
{
    private bool _initialized;

	public DefaultContentPage()
	{
		InitializeComponent();

        NavigatedTo += OnNavigatedTo;
    }

    private async void OnNavigatedTo(object sender, NavigatedToEventArgs e)
    {
        if (!_initialized && BindingContext is ContextViewModel vm)
            await vm.InitializeAsync();

        _initialized = true;
    }

    public void ShowInfoPopup(string caption, string text)
    {
        SfPopup popup = new();
        popup.HeaderTitle = caption;
        popup.AcceptButtonText = "Ok";
        popup.ShowFooter = true;
        popup.ShowCloseButton = true;
        popup.AutoSizeMode = PopupAutoSizeMode.Height;

        DataTemplate template = new(() =>
        {
            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Label label = new();
            label.Text = text;
            label.Margin = new Thickness(5);
            label.HorizontalOptions= LayoutOptions.Fill;
            label.VerticalOptions = LayoutOptions.Fill;
            label.LineBreakMode = LineBreakMode.WordWrap;

            grid.Add(label);

            return grid;
        });

        popup.ContentTemplate = template;
        popup.Show();
    }
}
