using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public static class AppExtensions
    {
        public static async Task PushAndIndicate(this Application? application, Page page, ContextViewModel viewModel)
        {
            viewModel.ShowActivityIndicator = true;
            await Task.Delay(4000);
            await application.MainPage.Navigation.PushAsync(page);
            viewModel.ShowActivityIndicator = false;
        }
    }

}
