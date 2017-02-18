using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Groorine.Xamarin.Droid
{
	public class PlaylistFragment : Fragment
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
			

		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			var listView = view.FindViewById<ListView>(Resource.Id.listView1);

			if (listView != null)
			{
				listView.Adapter = new ArrayAdapter<string>(Context, Resource.Layout.PlaylistTextView, new[] { "A.mid", "B.mid", "C.mid" });
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			return inflater.Inflate(Resource.Layout.Playlist, container, false);
		}
	}
}