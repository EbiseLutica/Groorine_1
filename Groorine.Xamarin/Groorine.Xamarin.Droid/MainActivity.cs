using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4;
using Android.Support.V7.App;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.Widget;
using Android.Support.V4.Widget;
using Android.Content.Res;

namespace Groorine.Xamarin.Droid
{
	[Activity (Label = "Groorine", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme")]
	public class MainActivity : AppCompatActivity
	{
		ActionBarDrawerToggle _drawerToggle;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			V7Toolbar toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);
			var drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			var drawerList = FindViewById<ListView>(Resource.Id.left_drawer);
			
			drawerList.Adapter = new DrawerAdapter(this,
				new DrawerObject("Playlist", Resource.Drawable.ic_playlist_play_white_36dp),
				new DrawerObject("MIDI Event", Resource.Drawable.ic_music_note_white_36dp)
				);

			_drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, Resource.String.app_name, Resource.String.app_name)
			{
				DrawerIndicatorEnabled = true
			};
			drawerLayout.SetDrawerListener(_drawerToggle);

			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);

			SupportActionBar.Title = GetString(Resource.String.no_project);

			drawerList.ItemClick += (sender, e) =>
			{
				var fragment = new PlaylistFragment();

				FragmentManager.BeginTransaction()
					.Replace(Resource.Id.content_frame, fragment)
					.Commit();
				drawerList.SetItemChecked(e.Position, true);
				drawerLayout.CloseDrawer(drawerList);
			};

			FragmentManager.BeginTransaction()
				.Replace(Resource.Id.content_frame, new PlaylistFragment())
				.Commit();
		}


		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.menu_play:
					Toast.MakeText(this, Resource.String.play, ToastLength.Short).Show();
					break;
				case Resource.Id.menu_stop:
					Toast.MakeText(this, Resource.String.stop, ToastLength.Short).Show();
					break;

			}
			return _drawerToggle.OnOptionsItemSelected(item) || base.OnOptionsItemSelected(item);
		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			_drawerToggle.SyncState();
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			_drawerToggle.OnConfigurationChanged(newConfig);
		}


	}
}


