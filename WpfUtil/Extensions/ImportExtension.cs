using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows;

namespace WpfUtil.Extensions {
    public class ImportExtension : MarkupExtension {

        public Uri Uri { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return Application.LoadComponent(Uri);
        }
    }
}
