using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using PCLExt.FileStorage;
using System.IO;

namespace GroorineCore
{
	public class InstrumentList : List<IInstrument>
	{
		public IEnumerable<IInstrument> FindInstrumentsByNote(byte noteNo) => from i in this
																			   where i.KeyRange.Contains(noteNo)
																			   select i;
		public IEnumerable<IInstrument> FindInstrumentsByVelocity(byte velocity) => from i in this
																					where i.VelocityRange.Contains(velocity)
																					select i;
		public IEnumerable<IInstrument> FindInstruments(byte noteNo, byte velocity) => from i in this
																					   where i.VelocityRange.Contains(velocity) && i.KeyRange.Contains(noteNo)
																					   select i;
	}

	public class AudioSourceManager
	{
		private static AudioSourceManager _instance;

		//public InstrumentList[] Instruments { get; } = new InstrumentList[128];
		public IInstrument[] Instruments { get; } = new IInstrument[128];
		public InstrumentList Drumset { get; } = new InstrumentList();
		

		private AudioSourceManager() { }

		private void AddInstrument(byte ch, IInstrument inst)
		{
			if (inst == null)
				throw new ArgumentNullException(nameof(inst));
			if (ch >= 128)
				throw new ArgumentOutOfRangeException(nameof(ch));
			/*if (Instruments[ch] == null)
				Instruments[ch] = new InstrumentList();
			if (Instruments[ch].Contains(inst))
				return;
			Instruments[ch].Add(inst);*/
			Instruments[ch] = inst;
		}
		

		private async Task InternalInitialize()
		{
			IFolder root = await FileSystem.Current.BaseStorage.GetFolderAsync("Presets");
			if (root == null)
				return;
			IList<IFile> files = await (await root.GetFolderAsync("Inst")).GetFilesAsync();
			if (files == null)
				return;

			foreach (IFile f in files)
			{
				var fileName = Path.GetFileNameWithoutExtension(f.Path);
				if (!byte.TryParse(fileName, out var ch))
					continue;
				
				
				// 現状拡張子のみで判断しているのでなんとかしたい
				switch (Path.GetExtension(f.Path).ToLower())
				{
					case "mssf":    // Music Sheet Sound File
						Stream s = await f.OpenAsync(FileAccess.Read);
						AddInstrument(ch, new Instrument(FileUtility.LoadMssf(s)));
						break;
					case "gsef":    // Groorine Sound Effect File
						throw new NotImplementedException("GSEF ファイルはまだサポートされていません。");
						break;
					case "wav":     // Wave
					case "wave":
						throw new NotImplementedException("Wave ファイルはまだサポートされていません。");
						break;
				}
			}

			for (byte i = 0; i < Instruments.Length; i++)
			{
				if (Instruments[i] == null)
				{
					//Instruments[i] = new InstrumentList();
					AddInstrument(i,  new Instrument(new AudioSourceSine()));
				}
			}

		}

		public static async Task<AudioSourceManager> GetInstance()
		{
			if (_instance == null)
			{
				_instance = new AudioSourceManager();
				await _instance.InternalInitialize();
			}
			return _instance;
		}
	}

	public interface IInstrument
	{
		Range<byte> KeyRange { get; }
		Range<byte> VelocityRange { get; }
		IAudioSource Source { get; }
		double Pan { get; }

	}

	public class Instrument : IInstrument
	{
		private double _pan;

		public Range<byte> KeyRange { get; }
		public Range<byte> VelocityRange { get; }
		public IAudioSource Source { get; }
		public double Pan
		{
			get { return _pan; }
			set
			{
				if (_pan > 100 || _pan < -100)
					throw new ArgumentOutOfRangeException(nameof(Pan));
				_pan = value;
				
			}
		}

		internal Instrument(Range<byte> key, Range<byte> vel, IAudioSource src, double pan = 0)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (vel == null)
				throw new ArgumentNullException(nameof(vel));
			if (!key.IsBetween(0, 127))
				throw new ArgumentOutOfRangeException(nameof(key));
			if (!vel.IsBetween(0, 127))
				throw new ArgumentOutOfRangeException(nameof(vel));
			if (src == null)
				throw new ArgumentNullException(nameof(src));
			KeyRange = key;
			VelocityRange = vel;
			Source = src;
			Pan = pan;
		}

		internal Instrument(IAudioSource src, double pan = 0)
			: this(new Range<byte>(0, 127), new Range<byte>(0, 127), src, pan) { }

		internal Instrument(byte noteNo, IAudioSource src, double pan = 0)
			: this(new Range<byte>(noteNo, noteNo), new Range<byte>(0, 127), src, pan) { }

		

	}

}
