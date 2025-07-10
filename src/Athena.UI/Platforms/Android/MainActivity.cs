using Android.App;
using Android.Content.PM;
using Android.OS;
using Athena.DataModel.Core;
using Athena.UI;

namespace Athena;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        DefaultDocumentScannerService.InitializeActivity(this);
        base.OnCreate(savedInstanceState);
    }
}
