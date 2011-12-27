using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Xaml;

namespace WpfUtil.Extensions {
    public class RootExtension : MarkupExtension {

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var root = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            return root == null ? null : root.RootObject;
        }
    }
}
