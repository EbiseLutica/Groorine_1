﻿using System.Collections.ObjectModel;
using System.Linq;

namespace GroorineCore
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

				Length = Events.LastOrDefault()?.Tick ?? 0;
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
			Events.CollectionChanged += (sender, args) => Length = Events.LastOrDefault()?.Tick ?? 0;
		}

		/// <summary>
		/// この <see cref="Track"/> の名前を取得または設定します。
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value); }
		}


	}
}