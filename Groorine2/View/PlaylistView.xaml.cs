// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Groorine
{
	/// <summary>
	/// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
	/// </summary>
	public sealed partial class PlaylistView
	{
		public PlaylistView()
		{
			DataContextChanged += (sender, args) => ViewModel = DataContext as MainPageViewModel;
			InitializeComponent();
		}

		public MainPageViewModel ViewModel { get; set; }


		private void UIElement_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var senderElement = sender as FrameworkElement;
			var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
			flyoutBase.ShowAt(senderElement);
		}
	}
}
