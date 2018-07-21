using System.Collections.ObjectModel;
using System.Linq;
using Groorine.Events;
using Groorine.Helpers;

namespace Groorine.DataModel
{
	/// <summary>
	/// MIDI ファイルのデータ構造を表現します。
	/// </summary>
	public class MidiFile : BindableBase
	{
		private ObservableCollection<Track> _tracks;
		private short _resolution;
		private string _title;
		private string _copyright;
		private long _length;
		private ConductorTrack _conductor;
		private long? _loopStart;

		/// <summary>
		/// このプロジェクトに含まれるトラックを取得します。
		/// </summary>
		public ObservableCollection<Track> Tracks
		{
			get { return _tracks; }
			private set { SetProperty(ref _tracks, value); }
		}

		/// <summary>
		/// このプロジェクトの制御イベントが含まれるトラックですを取得します。
		/// </summary>
		public ConductorTrack Conductor
		{
			get { return _conductor; }
			private set { SetProperty(ref _conductor, value); }
		}


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
		/// このプロジェクトのループ開始地点を取得または設定します。存在しない場合の値は <see cref="null"/> です。
		/// </summary>
		public long? LoopStart
		{
			get { return _loopStart; }
			set { SetProperty(ref _loopStart, value); }
		}

		/// <summary>
		/// このプロジェクトの著作権情報を取得または設定します。
		/// </summary>
		public string Copyright
		{

			get { return _copyright; }
			set { SetProperty(ref _copyright, value); }
		}

		/// <summary>
		/// このプロジェクトの長さを Tick 単位で取得します。
		/// </summary>
		public long Length
		{

			get { return _length; }
			set { SetProperty(ref _length, value); }
		}

		/// <summary>
		/// <see cref="MidiFile"/> の新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="ct"></param>
		/// <param name="tracks"></param>
		/// <param name="resolution"></param>
		/// <param name="title"></param>
		/// <param name="copyright"></param>
		public MidiFile(ConductorTrack ct, ObservableCollection<Track> tracks, short resolution, string title, string copyright, long? loopStart = null)
		{
			Tracks = tracks;

			foreach (Track t in tracks)
				foreach (NoteEvent ne in t.Events.OfType<NoteEvent>().Where(e => e.Channel == 9 && e.Gate < resolution))
					ne.Gate = resolution;
			Resolution = resolution;
			Title = title;
			Copyright = copyright;
			Conductor = ct;
			LoopStart = loopStart;
			
			if (Tracks?.Count > 0)
				Length = Tracks.Max(mt => mt.Length);
		}
		
	}
}