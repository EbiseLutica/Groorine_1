using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Groorine2.Controls;
using Groorine2.View;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace Groorine2
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage
    {
		private bool _isPaddingAdded;
		// Declare the top level nav items
		private readonly List<NavMenuItem> _navlist = new List<NavMenuItem>(
			new[]
			{
				new NavMenuItem
				{
					Symbol = Symbol.MusicInfo,
					Label = "Playlist",
					DestPage = typeof(PlaylistView)
				},
				new NavMenuItem
				{
					Symbol = Symbol.List,
					Label = "MIDI Event",
					DestPage = typeof(MidiEventView)
				}
			});

		public static MainPage Current;

	public MainPageViewModel ViewModel { get; set; }

	/// <summary>
	/// Initializes a new instance of the AppShell, sets the static 'Current' reference,
	/// adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
	/// provide the nav menu list with the data to display.
	/// </summary>
	public MainPage()
		{
			DataContextChanged += (sender, args) => ViewModel = DataContext as MainPageViewModel;
			
			DataContext = new MainPageViewModel();


			InitializeComponent();

			Loaded += (sender, args) =>
			{
				Current = this;

				CheckTogglePaneButtonSizeChanged();

				var titleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
				titleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
			};

			RootSplitView.RegisterPropertyChangedCallback(
				SplitView.DisplayModeProperty,
				(s, a) =>
				{
					// Ensure that we update the reported size of the TogglePaneButton when the SplitView's
					// DisplayMode changes.
					CheckTogglePaneButtonSizeChanged();
				});

			SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;


			NavMenuList.ItemsSource = _navlist;
			NavMenuList.SelectedIndex = 0;
		}

		public Frame AppFrame => frame;

	    /// <summary>
		/// Invoked when window title bar visibility changes, such as after loading or in tablet mode
		/// Ensures correct padding at window top, between title bar and app content
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void TitleBar_IsVisibleChanged(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar sender, object args)
		{
			if (!_isPaddingAdded && sender.IsVisible)
			{
				//add extra padding between window title bar and app content
				double extraPadding = (double)Application.Current.Resources["DesktopWindowTopPadding"];
				_isPaddingAdded = true;

				Thickness margin = NavMenuList.Margin;
				NavMenuList.Margin = new Thickness(margin.Left, margin.Top + extraPadding, margin.Right, margin.Bottom);
				margin = frame.Margin;
				frame.Margin = new Thickness(margin.Left, margin.Top + extraPadding, margin.Right, margin.Bottom);
				margin = TogglePaneButton.Margin;
				TogglePaneButton.Margin = new Thickness(margin.Left, margin.Top + extraPadding, margin.Right, margin.Bottom);
			}
		}

		/// <summary>
		/// Default keyboard focus movement for any unhandled keyboarding
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AppShell_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			FocusNavigationDirection direction = FocusNavigationDirection.None;
			switch (e.Key)
			{
				case Windows.System.VirtualKey.Left:
				case Windows.System.VirtualKey.GamepadDPadLeft:
				case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
				case Windows.System.VirtualKey.NavigationLeft:
					direction = FocusNavigationDirection.Left;
					break;
				case Windows.System.VirtualKey.Right:
				case Windows.System.VirtualKey.GamepadDPadRight:
				case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
				case Windows.System.VirtualKey.NavigationRight:
					direction = FocusNavigationDirection.Right;
					break;

				case Windows.System.VirtualKey.Up:
				case Windows.System.VirtualKey.GamepadDPadUp:
				case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
				case Windows.System.VirtualKey.NavigationUp:
					direction = FocusNavigationDirection.Up;
					break;

				case Windows.System.VirtualKey.Down:
				case Windows.System.VirtualKey.GamepadDPadDown:
				case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
				case Windows.System.VirtualKey.NavigationDown:
					direction = FocusNavigationDirection.Down;
					break;
			}

			if (direction != FocusNavigationDirection.None)
			{
				var control = FocusManager.FindNextFocusableElement(direction) as Control;
				if (control != null)
				{
					control.Focus(FocusState.Programmatic);
					e.Handled = true;
				}
			}
		}

		#region BackRequested Handlers

		private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
		{
			bool handled = e.Handled;
			BackRequested(ref handled);
			e.Handled = handled;
		}

		private void BackRequested(ref bool handled)
		{
			// Get a hold of the current frame so that we can inspect the app back stack.

			if (AppFrame == null)
				return;

			// Check to see if this is the top-most page on the app back stack.
			if (AppFrame.CanGoBack && !handled)
			{
				// If not, set the event to handled and go back to the previous page in the app.
				handled = true;
				AppFrame.GoBack();
			}
		}

		#endregion

		#region Navigation

		/// <summary>
		/// Navigate to the Page for the selected <paramref name="listViewItem"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="listViewItem"></param>
		private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
		{
			var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

			if (item?.DestPage != null &&
				item.DestPage != AppFrame.CurrentSourcePageType)
			{
				AppFrame.Navigate(item.DestPage, item.Arguments);
			}
		}

		/// <summary>
		/// Ensures the nav menu reflects reality when navigation is triggered outside of
		/// the nav menu buttons.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.Back)
			{
				var item = (from p in _navlist where p.DestPage == e.SourcePageType select p).SingleOrDefault();
				if (item == null && AppFrame.BackStackDepth > 0)
				{
					// In cases where a page drills into sub-pages then we'll highlight the most recent
					// navigation menu item that appears in the BackStack
					foreach (var entry in AppFrame.BackStack.Reverse())
					{
						item = (from p in _navlist where p.DestPage == entry.SourcePageType select p).SingleOrDefault();
						if (item != null)
							break;
					}
				}

				var container = (ListViewItem)NavMenuList.ContainerFromItem(item);

				// While updating the selection state of the item prevent it from taking keyboard focus.  If a
				// user is invoking the back button via the keyboard causing the selected nav menu item to change
				// then focus will remain on the back button.
				if (container != null) container.IsTabStop = false;
				NavMenuList.SetSelectedItem(container);
				if (container != null) container.IsTabStop = true;
			}
		}

		private void OnNavigatedToPage(object sender, NavigationEventArgs e)
		{
			// After a successful navigation set keyboard focus to the loaded page
			var page = e.Content as Page;
			if (page != null && e.Content != null)
			{
				var control = page;
				control.Loaded += Page_Loaded;
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			((Page)sender).Focus(FocusState.Programmatic);
			((Page)sender).Loaded -= Page_Loaded;
		}

		#endregion

		public Rect TogglePaneButtonRect
		{
			get;
			private set;
		}

		/// <summary>
		/// An event to notify listeners when the hamburger button may occlude other content in the app.
		/// The custom "PageHeader" user control is using this.
		/// </summary>
		public event TypedEventHandler<MainPage, Rect> TogglePaneButtonRectChanged;

		/// <summary>
		/// Public method to allow pages to open SplitView's pane.
		/// Used for custom app shortcuts like navigating left from page's left-most item
		/// </summary>
		public void OpenNavePane()
		{
			TogglePaneButton.IsChecked = true;
		}

		/// <summary>
		/// Hides divider when nav pane is closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RootSplitView_PaneClosed(SplitView sender, object args)
		{
		}

		/// <summary>
		/// Callback when the SplitView's Pane is toggled closed.  When the Pane is not visible
		/// then the floating hamburger may be occluding other content in the app unless it is aware.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TogglePaneButton_Unchecked(object sender, RoutedEventArgs e)
		{
			CheckTogglePaneButtonSizeChanged();
		}

		/// <summary>
		/// Callback when the SplitView's Pane is toggled opened.
		/// Restores divider's visibility and ensures that margins around the floating hamburger are correctly set.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
		{
			CheckTogglePaneButtonSizeChanged();
		}

		/// <summary>
		/// Check for the conditions where the navigation pane does not occupy the space under the floating
		/// hamburger button and trigger the event.
		/// </summary>
		private void CheckTogglePaneButtonSizeChanged()
		{
			if (RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
				RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
			{
				var transform = TogglePaneButton.TransformToVisual(this);
				var rect = transform.TransformBounds(new Rect(0, 0, TogglePaneButton.ActualWidth, TogglePaneButton.ActualHeight));
				TogglePaneButtonRect = rect;
			}
			else
			{
				TogglePaneButtonRect = new Rect();
			}

			var handler = TogglePaneButtonRectChanged;
			// handler(this, this.TogglePaneButtonRect);
			handler?.DynamicInvoke(this, TogglePaneButtonRect);
		}

		/// <summary>
		/// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
		/// using the associated Label of each item.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (!args.InRecycleQueue && args.Item is NavMenuItem)
			{
				args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
			}
			else
			{
				args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
			}
		}
		
    }
}
