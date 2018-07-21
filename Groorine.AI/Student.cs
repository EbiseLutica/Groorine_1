using Groorine.DataModel;
using Groorine.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Groorine.AI
{
	public class Student
	{
		private Dictionary<Key, List<Unit>>[] units;

		private List<int> bpmList = new List<int>();

		internal Unit AddUnit(Key key, int ch, Unit unit)
		{
			// null check
			if (units == null) units = new Dictionary<Key, List<Unit>>[16];
			if (units[ch] == null) units[ch] = new Dictionary<Key, List<Unit>>();
			if (!units[ch].ContainsKey(key) || units[ch][key] == null) units[ch][key] = new List<Unit>();

			units[ch][key].Add(unit);
			return unit;
		}

		public void Clear()
		{
			units = null;
			bpmList.Clear();
			bpmList.Add(120);
		}

		public void Learn(MidiFile mf)
		{
			bpmList.AddRange(mf.Conductor.TempoMap.Select(t => t.Tempo));
			// 全イベントのデータを複製し、チャンネルごとに分ける
			var dataSource = mf.Tracks
				// ノートイベントを抽出
				.Select(t => t.Events.OfType<NoteEvent>())
				// 一次元化する
				.SelectMany(t => t)
				// tick順にソート
				.OrderBy(n => n.Tick)
				// すべてNoteEventExに変換
				.Select(n => new NoteEventEx { Channel = n.Channel, Gate = n.Gate, Note = n.Note, Tick = n.Tick, Velocity = n.Velocity })
				// チャンネルでグループ化
				.GroupBy(n => n.Channel)
				// グループをチャンネル順にソート
				.OrderBy(g => g.Key)
				// 各チャンネルのデータをTickでグループ化(和音のグループに)
				.Select(g => g.GroupBy(n => n.Tick).ToArray())
				// すべて配列化
				.ToArray();

			int count = 0;
			// 学習処理
			foreach (var events in dataSource)
			{
				Unit prevUnit = default;
				for (int i = 0; i < events.Length - 1; i++)
				{
					var current = events[i];
					var next = events[i + 1];

					var nextOne = next.FirstOrDefault();

					Key key = default;
					foreach (var note in current)
					{
						// 処理しやすくするため、次のノートへの待ち時間データを付加する
						note.Wait = nextOne != default ? nextOne.Tick - note.Gate - note.Tick : 0;
						key = key.Note < note.Note ? new Key(note) : key;
					}

					//候補追加
					if (prevUnit != default)
						prevUnit.Candidates.Add(key);

					// 登録
					prevUnit = AddUnit(key, count, new Unit { Notes = current.ToArray() });
				}
				count++;
			}
		}

		public MidiFile Generate(int size)
		{
			var ct = new ConductorTrack(new ObservableCollection<MetaEvent>(new List<TempoEvent>() { new TempoEvent { Tempo = bpmList.Random() } }), 480);
			var tracks = new ObservableCollection<Track>();
			var drumIsAvailable = Extension.Random(10) < 5;
			var channels = Extension.Random(1, 17);
			if (units == null)
				return null;
			for (int ch = 0; ch < channels; ch++)
			{
				var events = new ObservableCollection<MidiEvent>();
				if (units[ch] == null)
					continue;
				var unit = units[ch].ToList().Random().Value.Random();

				if (unit == default)
					continue;

				var currentTick = 0L;
				for (int i = 0; i < size; i++)
				{
					if (unit.Notes.Length == 0)
						break;
					foreach (var note in unit.Notes)
					{
						events.Add(new NoteEvent { Note = note.Note, Gate = note.Gate, Velocity = note.Velocity, Channel = note.Channel, Tick = currentTick });
					}
					// 候補がなくなったら終了
					if (unit.Candidates.Count == 0)
						break;

					unit = units[ch][unit.Candidates.Random()].Random();
					currentTick += unit.Notes[0].Gate + unit.Notes[0].Wait;
				}

				tracks.Add(new Track(events));
			}

			// チャンネル数がドラムパートに届かなかった場合50%でドラムが入る
			if (channels < 9 && drumIsAvailable && units[9] != null && units[9].Count > 0)
			{
				var events = new ObservableCollection<MidiEvent>();
				var unit = units[9].ToList().Random().Value?.Random();

				var currentTick = 0L;

				for (int i = 0; i < size; i++)
				{
					if (unit.Notes.Length == 0)
						continue;
					foreach (var note in unit.Notes)
					{
						events.Add(new NoteEvent { Note = note.Note, Gate = note.Gate, Velocity = note.Velocity, Channel = note.Channel, Tick = currentTick });
					}
					// 候補がなくなったら終了
					if (unit.Candidates.Count == 0)
						break;

					unit = units[9][unit.Candidates.Random()].Random();
					currentTick += unit.Notes[0].Gate + unit.Notes[0].Wait;
				}

				tracks.Add(new Track(events));
			}


			return new MidiFile(ct, tracks, 480, $"AI Generated Song {DateTime.Now.ToString()}", "");
		}

	}

	public static class Extension
	{
		private static Random rnd = new Random();

		public static int Random(int min, int max) => rnd.Next(min, max);

		public static int Random(int max) => rnd.Next(max);

		public static T Random<T>(this IList<T> list) => list.Count == 0 ? default : list[rnd.Next(list.Count)];
	}

}
