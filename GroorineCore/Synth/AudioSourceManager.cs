using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GroorineCore.Api;
using GroorineCore.DataModel;
using GroorineCore.Helpers;

namespace GroorineCore.Synth
{
	public class InstrumentList : List<IInstrument>
	{
		public IEnumerable<IInstrument> FindInstrumentsByNote(byte noteNo) => this.Where(i => i.KeyRange.Contains(noteNo));
		public IEnumerable<IInstrument> FindInstrumentsByVelocity(byte velocity) => this.Where(i => i.VelocityRange.Contains(velocity));
		public IEnumerable<IInstrument> FindInstruments(byte noteNo, byte velocity) => this.Where(i => i.KeyRange.Contains(noteNo) && i.VelocityRange.Contains(velocity));
	}

	public class AudioSourceManager
	{
		private static AudioSourceManager _instance;
		
		public IInstrument[] Instruments { get; } = new IInstrument[128];
		public InstrumentList Drumset { get; } = new InstrumentList();

		private AudioSourceManager() { }

		private void AddInstrument(byte ch, IInstrument inst)
		{
			if (inst == null)
				return;
			if (ch >= 128)
				throw new ArgumentOutOfRangeException(nameof(ch));
			Instruments[ch] = inst;
		}
		

		private async Task InternalInitializeAsync(IFileSystem fs, string resPath)
		{
			IFolder root = await fs.BaseFolder.GetFolderAsync(Path.Combine(resPath, "Presets"));
			if (root == null)
				return;
			IList<IFile> files = await (await root.GetFolderAsync("Inst")).GetFilesAsync();
			if (files != null)
			{

				foreach (IFile f in files)
				{
					var fileName = Path.GetFileNameWithoutExtension(f.Path);
					if (!byte.TryParse(fileName, out var ch))
						continue;
					
					AddInstrument(ch, new Instrument(ch, await ReadAudioSourceFileAsync(f)));
				}
			}

			files = await (await root.GetFolderAsync("Drum")).GetFilesAsync();
			if (files != null)
			{
				foreach (IFile f in files)
				{
					var fileName = Path.GetFileNameWithoutExtension(f.Path);
					if (!byte.TryParse(fileName, out var ch))
						continue;


					Drumset?.Add(new Instrument(ch, await ReadAudioSourceFileAsync(f)));
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

		private static async Task<IAudioSource> ReadAudioSourceFileAsync(IFile f)
		{

			Stream s = await f.OpenAsync(FileAccessMode.Read);
			if (s == null)
				return null;

			// 現状拡張子のみで判断しているけどもっと良い方法ないかな
			switch (Path.GetExtension(f.Path).ToLower().Remove(0, 1))
			{
				case "mssf":    // Music Sheet Sound File
					return FileUtility.LoadMssf(s);

				case "gsef":    // Groorine Sound Effect File
					throw new NotImplementedException("GSEF ファイルはまだサポートされていません。");

				case "wav":
				case "wave":// Wave
					return new AudioSourceWav(s);

				default:
					throw new InvalidOperationException("サポートされていないファイルです。");
			}
		}

		public static async Task<AudioSourceManager> InitializeAsync(IFileSystem fileSystem, string resPath = "")
		{
			if (fileSystem == null)
				throw new ArgumentNullException(nameof(fileSystem));
			_instance = new AudioSourceManager();
			await _instance.InternalInitializeAsync(fileSystem, resPath);
			return _instance;
		}

		public static AudioSourceManager GetInstance()
		{
			if (_instance == null)
				throw new InvalidOperationException("初期化されていません。");
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
