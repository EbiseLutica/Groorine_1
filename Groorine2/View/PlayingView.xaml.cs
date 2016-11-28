// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Groorine.View
{
	/// <summary>
	/// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
	/// </summary>
	public sealed partial class PlayingView
	{
		public PlayingView()
		{
			DataContextChanged += (sender, args) => ViewModel = DataContext as MainPageViewModel;
			InitializeComponent();
		}

		public MainPageViewModel ViewModel { get; set; }
	}
}
