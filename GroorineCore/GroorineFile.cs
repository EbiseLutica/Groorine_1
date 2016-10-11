using System.Collections.ObjectModel;
using System.Linq;

namespace GroorineCore
{
	/// <summary>
	/// Groorine プロジェクトファイルのデータ構造を表現します。
	/// </summary>
	public class GroorineFile : BindableBase
	{
		private ObservableCollection<Track> _tracks;
		/// <summary>
		/// このプロジェクトに含まれるトラックです。
		/// </summary>
		public ObservableCollection<Track> Tracks
		{
			get { return _tracks; }
			private set { SetProperty(ref _tracks, value); }
		}

		private short _resolution;
		private string _title;
		private string _copyright;
		private long _length;

		/// <summary>
		/// このプロジェクトの解像度を取得します。
		/// </summary>
		public short Resolution
		{
			get {return _resolution;}
			private set { SetProperty(ref _resolution, value); }
		}

		/// <summary>
		/// このプロジェクトのタイトルを取得または設定します。
		/// </summary>
		public string Title
		{

			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		/// <summary>
		/// このプロジェクトの著作権情報を取得または設定します。
		/// </summary>
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

		/// <summary>
		/// <see cref="GroorineFile"/> の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="tracks"></param>
		/// <param name="resolution"></param>
		/// <param name="title"></param>
		/// <param name="copyright"></param>
		public GroorineFile(ObservableCollection<Track> tracks, short resolution, string title, string copyright)
		{
			Tracks = tracks;
			Resolution = resolution;
			Title = title;
			Copyright = copyright;

			if (Tracks?.Count > 0)
				Length = Tracks.Max(mt => mt.Length);
		}
		
	}
}