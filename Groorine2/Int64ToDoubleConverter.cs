using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Groorine
{
	public class Int64ToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language) => (double) (long) value;

		public object ConvertBack(object value, Type targetType, object parameter, string language) => (long) (double) value;
	}

	public class Int32ToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language) => (double)(int)value;

		public object ConvertBack(object value, Type targetType, object parameter, string language) => (int)(double)value;
	}

}