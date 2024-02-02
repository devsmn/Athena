using Syncfusion.Maui.TabView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public class CustomTabItem : SfTabItem
    {
        /// <summary>
        /// Identifies the <see cref="Page"/> bindable property.
        /// </summary>
        /// <value>
        /// The identifier for <see cref="Page"/> bindable property.
        /// </value>
        public static readonly BindableProperty PageProperty =
            BindableProperty.Create(nameof(Page), typeof(ContentPage), typeof(CustomTabItem), null, propertyChanged: OnPagePropertyChanged);


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTabItem"/> class.
        /// </summary>
        public CustomTabItem()
        {
        }

        /// <summary>
        /// Gets or sets the content page.
        /// </summary>
        /// <value>
        /// Specifies the content page of the custom tab item. The default value is null.
        /// </value>
        public ContentPage Page
        {
            get => (ContentPage)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="PageProperty"/> is set for the control.
        /// </summary>
        /// <param name="bindable">The bindable.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private static void OnPagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var page = bindable as CustomTabItem;

            if (page != null)
            {
                page.Content = (newValue as ContentPage).Content;
            }
        }
    }
}
