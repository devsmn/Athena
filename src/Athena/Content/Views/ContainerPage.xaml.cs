namespace Athena.UI
{


    public partial class ContainerPage : TabbedPage
    {
        public ContainerPage()
        {
            InitializeComponent();
        }

        private void NavigationPage_OnPopped(object sender, NavigationEventArgs e)
        {
            if (e.Page.BindingContext is not ContextViewModel vm)
                return;

            vm.Dispose();
        }
    }
}