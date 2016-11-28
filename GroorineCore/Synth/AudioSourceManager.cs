using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GroorineCore.Api;
using GroorineCore.Helpers;

namespace GroorineCore.Synth
{

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
					byte ch;
					if (!byte.TryParse(fileName, out ch))
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
					byte ch;
					if (!byte.TryParse(fileName, out ch))
						continue;


					Drumset?.Add(new Instrument(ch, await ReadAudioSourceFileAsync(f)));
				}

			}


			for (byte i = 0; i < Instruments.Length; i++)
			{
				if (Instruments[i] == null)
				{
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

}
