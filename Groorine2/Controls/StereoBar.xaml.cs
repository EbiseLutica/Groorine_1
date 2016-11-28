using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Groorine.Annotations;
using static System.Math;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Groorine.Controls
{
	[ContentProperty(Name = nameof(Value))]
	public sealed partial class StereoBar : UserControl, INotifyPropertyChanged
	{
		public StereoBar()
		{
			InitializeComponent();
			SizeChanged += (sender, args) =>
			{
				OnPropertyChanged(nameof(LeftWidth));
				OnPropertyChanged(nameof(RightWidth));
			};
			
		}



		private int MedValue => (MaxValue + MinValue) / 2;

		private double A => (Abs(MaxValue) + Abs(MinValue)) / 2d;

		private double LeftWidth => Max(0, MedValue - Value) / A * (ActualWidth / 2);

		private double RightWidth => Max(0, Value - MedValue) / A * (ActualWidth / 2);




		public int Value
		{
			get { return (int)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(int), typeof(StereoBar), PropertyMetadata.Create(64, PropertyChangedCallback));

		private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			var s = o as StereoBar;
			if (args.NewValue == args.OldValue)
				return;
			s?.OnPropertyChanged(nameof(LeftWidth));
			s?.OnPropertyChanged(nameof(RightWidth));
		}


		public int MaxValue
		{
			get { return (int)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
		
		public static readonly DependencyProperty MaxValueProperty =
			DependencyProperty.Register("MaxValue", typeof(int), typeof(StereoBar), PropertyMetadata.Create(127, PropertyChangedCallback));


		public int MinValue
		{
			get { return (int)GetValue(MinValueProperty); }
			set { SetValue(MinValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinValueProperty =
			DependencyProperty.Register("MinValue", typeof(int), typeof(StereoBar), PropertyMetadata.Create(0, PropertyChangedCallback));


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		
		}

	}
}
