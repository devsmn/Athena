using Athena.Resources.Localization;
using FFImageLoading.Maui;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Rotator;

namespace Athena.UI;

public partial class TutorialView : ContentPage
{
    public TutorialView()
    {
        InitializeComponent();

        List<SfRotatorItem> items = new List<SfRotatorItem> {
            new SfRotatorItem {
                ItemContent = new Label {
                    Text = Localization.TutWelcome,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step1.png",
                    Localization.TutStep1)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step2.png",
                    Localization.TutStep2)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step3.png",
                    Localization.TutStep3)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step4.png",
                    Localization.TutStep4)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step5.png",
                    Localization.TutStep5)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step8.png",
                    Localization.TutStep8)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step6.png",
                    Localization.TutStep6)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step7.png",
                    Localization.TutStep7)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step9.png",
                    Localization.TutStep9)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step10.png",
                    Localization.TutStep10)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step11.png",
                    Localization.TutStep11)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step12.png",
                    Localization.TutStep12)
            },

            new SfRotatorItem {
                ItemContent = SetupImageTextGrid(
                    "tut_step13.png",
                    Localization.TutStep13)
            },
        };


        Button b = new Button();
        b.WidthRequest = 150;
        b.HeightRequest = 50;
        b.Text = Localization.Close;
        b.Clicked += (sender, e) =>
        {
            App.Current.MainPage.Navigation.PopModalAsync();
        };

        SfRotatorItem close = new SfRotatorItem();
        Grid grid = new Grid();
        grid.AddRowDefinition(new RowDefinition { Height = GridLength.Star });
        grid.AddColumnDefinition(new ColumnDefinition { Width = GridLength.Star });
        
        b.HorizontalOptions = LayoutOptions.Center;
        b.VerticalOptions = LayoutOptions.Center;
        grid.Add(b);
        close.ItemContent = grid;
        
        items.Add(close);

        rotator.ItemsSource = items;

    }

    private Grid SetupImageTextGrid(string image, string text)
    {
        Grid grid = new Grid
        {
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = {
                new ColumnDefinition { Width = GridLength.Star }
            },
        };

        grid.Add(
            new Border
            {
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5),
                },
                Content = new CachedImage
                {

                    Source = image,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            });

        Grid textGrid = new Grid();
        textGrid.Margin = new Thickness(5);
        textGrid.AddColumnDefinition(new ColumnDefinition(GridLength.Auto));
        textGrid.AddColumnDefinition(new ColumnDefinition(GridLength.Star));

        textGrid.Add(
            new BoxView {
                BackgroundColor = Color.FromArgb("#0b122e"),
                WidthRequest = 3
            });
        textGrid.Add(
            new Label {
                Margin = new Thickness(5),
                Text = text,
                HorizontalTextAlignment = TextAlignment.Center
            }, 1, 0);

        grid.Add(
            textGrid, 0, 1);

        return grid;
    }
}