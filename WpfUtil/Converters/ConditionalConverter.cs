using System;
using System.Windows;
using System.Windows.Data;

namespace WpfUtil.Converters {
    public class ConditionalToBooleanConverter : DependencyObject,  IValueConverter {

        public DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ConditionalToBooleanConverter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public object Value {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }



        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return object.Equals(value, Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException("Converter supports one way binding only");
        }
    }
}
