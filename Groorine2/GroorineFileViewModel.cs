using GroorineCore.DataModel;
using GroorineCore.Helpers;

namespace Groorine
{
	public class GroorineFileViewModel : BindableBase
	{
		private GroorineFile _file;
		private string _title;
		private string _copyright;
		private long _length;

		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		public string Copyright
		{
			get { return _copyright; }
			set { SetProperty(ref _copyright, value); }
		}

		public long Length
		{
			get { return _length; }
			set { SetProperty(ref _length, value); }
		}

		public GroorineFile File
		{
			get { return _file; }
			set { SetProperty(ref _file, value); }

		}

		/// <summary>
		/// <see cref="GroorineFileViewModel"/> クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="model"></param>
		public GroorineFileViewModel(GroorineFile model)
		{
			Title = "No Project";
			Copyright = "";
			Length = 0;

			if (model == null) return;
			_file = model;
			Title = model.Title;
			Copyright = model.Copyright;
			Length = model.Length;
		}

	}
}