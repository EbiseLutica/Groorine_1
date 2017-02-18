using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Groorine.Xamarin.Droid
{

	class DrawerObject
	{
		public int ImageResourceId { get; set; }
		public string Text { get; set; }
		public DrawerObject(string text, int imageResId)
		{
			ImageResourceId = imageResId;
			Text = text;
		}
	}

	class DrawerAdapter : BaseAdapter
	{

		Context _context;
		IList<DrawerObject> _objects;

		public DrawerAdapter(Context context, params DrawerObject[] objects)
			: this(context, (IList<DrawerObject>)objects) { }
		public DrawerAdapter(Context context, IList<DrawerObject> objects)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_objects = objects ?? throw new ArgumentNullException(nameof(objects));
		}

		public override Java.Lang.Object GetItem(int position) => position;

		public override long GetItemId(int position) => position;

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			DrawerAdapterViewHolder holder = null;

			if (view != null)
				holder = view.Tag as DrawerAdapterViewHolder;

			if (holder == null)
			{
				holder = new DrawerAdapterViewHolder();
				var inflater = _context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
				//replace with your item and your holder items
				//comment back in
				view = inflater.Inflate(Resource.Layout.DrawerItem, parent, false);
				holder.Image = view.FindViewById<ImageView>(Resource.Id.imageView);
				holder.Text = view.FindViewById<TextView>(Resource.Id.textView);

				view.Tag = holder;
			}


			//fill in your items
			//holder.Title.Text = "new text here";
			holder.Text.Text = _objects[position].Text;
			holder.Image.SetImageResource(_objects[position].ImageResourceId);
			return view;
		}

		//Fill in cound here, currently 0
		public override int Count => _objects.Count;

	}

	class DrawerAdapterViewHolder : Java.Lang.Object
	{
		//Your adapter views to re-use
		//public TextView Title { get; set; }
		public ImageView Image { get; set; }
		public TextView Text { get; set; }
	}
}