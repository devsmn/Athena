namespace Athena.UI;

public class ContextContentPage : ContentPage
{
    public TaskCompletionSource DoneTcs { get; private set; }

	public ContextContentPage()
    {
        DoneTcs = new();
    }
}
