using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GroorineCore.Events;
using GroorineCore.Helpers;

namespace GroorineCore.DataModel
{
	public class ConductorTrack : BindableBase
	{
		private ObservableCollection<MetaEvent> _events;

		private long _length;
		private short _resolution;

		/// <summary>
		/// このトラックに存在する MIDI イベントのリストです。
		/// </summary>
		public ObservableCollection<MetaEvent> Events
		{
			get { return _events; }
			private set
			{
				SetProperty(ref _events, value);

				Length = Events.LastOrDefault()?.Tick ?? 0;
				ResetTempoMap();
			}
		}

		public long Length
		{
			get { return _length; }
			set { SetProperty(ref _length, value); }
		}


		private int? _msec;
		private int _toTickCache;
		public int ToTick(int msec)
		{
			
			if (_msec is int ms && ms == msec)
				return _toTickCache;
			if (msec < 0)
				throw new ArgumentOutOfRangeException(nameof(msec));
			ScoreTempo scoreTempo = TempoMap.FindLast(obj => obj.MilliSeconds <= msec);
			_msec = msec;
			return _toTickCache = scoreTempo.Tick + GetTickLength(msec - scoreTempo.MilliSeconds, scoreTempo.Tempo, _resolution);
		}

		private int? _tick;
		private int _toMilliSecCache;
		public int ToMilliSeconds(int tick)
		{
			if (_tick is int t && t == tick)
				return _toMilliSecCache;
			if (tick < 0)
				throw new ArgumentOutOfRangeException(nameof(tick));
			ScoreTempo scoreTempo = TempoMap.FindLast(obj => obj.Tick <= tick);
			_tick = tick;
			return _toMilliSecCache = scoreTempo.MilliSeconds + GetMilliSeconds(tick - scoreTempo.Tick, scoreTempo.Tempo, _resolution);
		}

		internal ConductorTrack(ObservableCollection<MetaEvent> events, short resolution)
		{
			_resolution = resolution;
			events = events ?? new ObservableCollection<MetaEvent>();
			Events = events;
			Events.CollectionChanged += (sender, args) =>
			{
				Length = Events.LastOrDefault()?.Tick ?? 0;
				ResetTempoMap();
			};
		}

		public static int GetTickLength(int msec, int tempo, int resolution) => (int)(resolution * (msec / 1000.0) * (tempo / 60.0));

		public static int GetMilliSeconds(int tick, int tempo, int resolution) => (int)(tick / (double)resolution / tempo * 60000.0);

		public List<ScoreTempo> TempoMap { get; private set; }


		private void ResetTempoMap()
		{
			var st = new ScoreTempo(0, 0, 120);
			var list = new List<ScoreTempo>();
			list.Add(st);
			foreach (TempoEvent current in (from me in Events
											where me is TempoEvent
											select me as TempoEvent))
			{
				var msec = st.MilliSeconds + GetMilliSeconds((int)(current.Tick - st.Tick), st.Tempo, _resolution);
				st = new ScoreTempo(msec, (int)current.Tick, current.Tempo);
				list.Add(st);
			}
			TempoMap = list;
		}


	}

	/// <summary>
	/// 位置情報とテンポのセットです。
	/// </summary>
	public class ScoreTempo
	{
		/// <summary>
		/// データの時刻です。
		/// </summary>
		public int MilliSeconds { get; }
		/// <summary>
		/// データの時刻です。
		/// </summary>
		public int Tick { get; }
		/// <summary>
		/// テンポの値です。
		/// </summary>
		public int Tempo { get; }
		/// <summary>
		/// ScoreTempo のインスタンスを作成します。
		/// </summary>
		internal ScoreTempo(int msec, int tick, int tempo)
		{
			MilliSeconds = msec;
			Tick = tick;
			Tempo = tempo;
		}
	}
}