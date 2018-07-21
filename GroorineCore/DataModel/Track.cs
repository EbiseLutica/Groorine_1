using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Groorine.Events;
using Groorine.Helpers;

namespace Groorine.DataModel
{
	/// <summary>
	/// Groorine プロジェクト内の、MIDI イベントが含まれたトラックを表現します。
	/// </summary>
	public class Track : BindableBase
	{
		private ObservableCollection<MidiEvent> _events;
		private string _name;

		private long _length;

		/// <summary>
		/// このトラックに存在する MIDI イベントのリストです。
		/// </summary>
		public ObservableCollection<MidiEvent> Events
		{
			get { return _events; }
			private set
			{
				SetProperty(ref _events, value);

				SetLength();
			}
		}

		public long Length
		{
			get { return _length; }
			set { SetProperty(ref _length, value); }
		}


		public Track(ObservableCollection<MidiEvent> events)
		{
			events = events ?? new ObservableCollection<MidiEvent>();
			Events = events;
			Events.CollectionChanged += (s, e) => SetLength();
		}
		
		private void SetLength()
		{
			MidiEvent lod = Events.LastOrDefault();
			if (lod == null)
				return;
			Length = lod.Tick;
			if (lod is NoteEvent)
			{
				var ne = lod as NoteEvent;
				Length = ne.Tick + ne.Gate;
			}
		}

		/// <summary>
		/// この <see cref="Track"/> の名前を取得または設定します。
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value); }
		}

		public IEnumerable<MidiEvent> GetDataBetweenTicks(int startTick, int endTick) => from me in Events
																						 where startTick <= me.Tick && me.Tick <= endTick
																						 select me;


	}
}