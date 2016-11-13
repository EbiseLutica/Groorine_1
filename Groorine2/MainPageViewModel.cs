using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using GroorineCore;
using GroorineCore.Api;
using GroorineCore.Helpers;
using GroorineCore.Synth;
using FileAccessMode = GroorineCore.Api.FileAccessMode;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Win8.Wave.WaveOutputs;

namespace Groorine
{
	// We are initializing a COM interface for use within the namespace
	// This interface allows access to memory at the byte level which we need to populate audio data that is generated
	[ComImport]
	[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

	unsafe interface IMemoryBufferByteAccess
	{
		void GetBuffer(out byte* buffer, out uint capacity);
	}

	public class MainPageViewModel : BindableBase
	{
		private GroorineFileViewModel _currentFile;

		private bool _canStop;

		private bool _isPlaying;
		private int _masterVolume;

		private ObservableCollection<StorageFile> _musicFiles;
		private long _musicPosition;

		private int _resolution;

		private StorageFile _selectedMusic;
		private bool _canPlay;

		private Player _player;
		private AudioGraph graph;
		private AudioDeviceOutputNode deviceOutputNode;
		private AudioFrameInputNode frameInputNode;

		private IWavePlayer _nativePlayer;
		private BufferedWaveProvider _bwp;



		private short[] _buffer;
		private bool _isInitialized;

		public MainPageViewModel()
		{
			_musicFiles = new ObservableCollection<StorageFile>();
			CurrentFile = new GroorineFileViewModel(null);

			//_player = new Player();
			Initialize();

		}

		public bool IsPlaying
		{
			get { return _isPlaying; }
			set
			{
				SetProperty(ref _isPlaying, value);
				// ReSharper disable once ExplicitCallerInfoArgument
				OnPropertyChanged(nameof(IsNotPlaying));
			}
		}

		public bool CanStop
		{
			get { return _canStop; }
			set { SetProperty(ref _canStop, value); }
		}

		public bool CanPlay
		{
			get { return _canPlay; }
			set { SetProperty(ref _canPlay, value); }
		}

		public bool IsNotPlaying => !_isPlaying;

		public bool IsInitialized
		{
			get { return _isInitialized; }
			set { SetProperty(ref _isInitialized, value); }
		}

		public int Resolution
		{
			get { return _resolution; }
			set { SetProperty(ref _resolution, value); }
		}

		public int MasterVolume
		{
			get { return _masterVolume; }
			set
			{
				if (value < 0) value = 0;
				if (value > 127) value = 127;
				SetProperty(ref _masterVolume, value);
			}
		}

		public ObservableCollection<StorageFile> MusicFiles
		{
			get { return _musicFiles; }
			private set { SetProperty(ref _musicFiles, value); }
		}

		public GroorineFileViewModel CurrentFile
		{
			get { return _currentFile; }
			set { SetProperty(ref _currentFile, value); }
		}

		public StorageFile SelectedMusic
		{
			get { return _selectedMusic; }
			set { SetProperty(ref _selectedMusic, value); }
		}

		public long MusicPosition
		{
			get { return _musicPosition; }
			set
			{
				if (CurrentFile == null)
					return;
				if (value >= CurrentFile.Length)
					value = CurrentFile.Length - 1;
				if (value < 0)
					value = 0;
				SetProperty(ref _musicPosition, value);
			}
		}

		public void Play()
		{
			IsPlaying = true;
			CanStop = true;
			_player.Play();
		}



		public void Pause()
		{
			IsPlaying = false;
			_player.Pause();
		}

		public void Stop()
		{
			IsPlaying = false;
			CanStop = false;
			_player.Stop();
		}

		public async void Load()
		{
			if (SelectedMusic == null) return;
			Stop();
			_player.Load(SmfParser.Parse(await GetFileAsStreamAsync(SelectedMusic)));
			CurrentFile = new GroorineFileViewModel(_player.CurrentFile);

			CanPlay = true;
			Play();
		}

		public async void ImportFromMidi()
		{
			var filePicker = new FileOpenPicker {ViewMode = PickerViewMode.List};


			filePicker.FileTypeFilter.Add(".mid");
			filePicker.FileTypeFilter.Add(".midi");
			filePicker.FileTypeFilter.Add(".smf");

			filePicker.CommitButtonText = "Import";

			var file = await filePicker.PickSingleFileAsync();

			if (file == null)
				return;

			if (file.ContentType != "audio/mid" &&
				file.ContentType != "audio/midi")
			{
				await
					new MessageDialog("Please select a valid standard midi file.", "Selected file is not a midi file!").ShowAsync();
				return;
			}


			IStorageItem rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			var dir = rootDir as StorageFolder;
			if (dir == null)
			{
				await new MessageDialog("Please restart this app!", "Music Folder is not found!").ShowAsync();
				return;
			}
			MusicFiles.Add(await file.CopyAsync(dir));

		}

		public async void ExportToAudio()
		{
			await new MessageDialog("Groorine is still in development ><", "That feature is not implement yet!").ShowAsync();
		}


		private async void Initialize()
		{
			IStorageItem rootDir = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Music");
			var dir = rootDir as StorageFolder;
			if (dir == null)
			{
				dir = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Music");

				var file = await Package.Current.InstalledLocation.TryGetItemAsync("Hello, Groorine.mid") as StorageFile;
				file?.CopyAsync(dir);
			}
			IAsyncOperation<IReadOnlyList<StorageFile>> asyncOperation = dir?.GetFilesAsync();
			if (asyncOperation == null) return;
			IReadOnlyList<StorageFile> files = await asyncOperation;
			if (files != null)
				MusicFiles = new ObservableCollection<StorageFile>(files);

			MasterVolume = 100;

			await (await Package.Current.InstalledLocation.GetFolderAsync("Presets\\Inst")).GetFilesAsync();

			await AudioSourceManager.InitializeAsync(new FileSystem(), "GroorineCore");


			
			var settings = new AudioGraphSettings(AudioRenderCategory.Media);

			settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.ClosestToDesired;
			settings.DesiredSamplesPerQuantum = 44100;

			var result = await AudioGraph.CreateAsync(settings);

			if (result.Status != AudioGraphCreationStatus.Success)
			{
				await new MessageDialog("Can't create AudioGraph! Application will stop...").ShowAsync();
				Application.Current.Exit();
			}

			graph = result.Graph;
			
			var deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();
			if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
			{
				await new MessageDialog("Can't create DeviceOutputNode! Application will stop...").ShowAsync();
				Application.Current.Exit();
			}
			deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
		
			var nodeEncodingProperties = graph.EncodingProperties;
			nodeEncodingProperties.ChannelCount = 2;
			nodeEncodingProperties.SampleRate = 44100;

			
			frameInputNode = graph.CreateFrameInputNode(nodeEncodingProperties);
			frameInputNode.AddOutgoingConnection(deviceOutputNode);
			
			frameInputNode.Stop();
			_player = new Player((int)nodeEncodingProperties.SampleRate);
			
			frameInputNode.QuantumStarted += (sender, args) =>
			{
				uint numSamplesNeeded = (uint) args.RequiredSamples;

				if (numSamplesNeeded != 0)
				{
					AudioFrame audioData = GenerateAudioData(numSamplesNeeded);
					frameInputNode.AddFrame(audioData);
				}
			};

			graph.Start();
			frameInputNode.Start();
		

			/*
			_player = new Player();

			_buffer = _player.CreateBuffer(50);

			_bwp = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
			_nativePlayer = new WasapiOutRT(AudioClientShareMode.Shared, 50);
			_nativePlayer.Init(() => _bwp);
			_nativePlayer.Play();
			*/
			IsInitialized = true;
			/*
			while (true)
			{
				_player.GetBuffer(_buffer);

				var b = ToByte(_buffer);
				_bwp.AddSamples(b, 0, b.Length);
				while (_bwp.BufferedBytes > _buffer.Length * 2)
					await Task.Delay(1);
			}
			*/

		}

		private static byte[] ToByte(short[] a)
		{
			var size = a.Length * sizeof(short);
			var b = new byte[size];
			unsafe
			{

				fixed (short* psrc = &a[0])
				{
					using (var strmSrc = new UnmanagedMemoryStream((byte*)psrc, size))
						strmSrc.Read(b, 0, size);
				}
			}
			return b;
		}

		private unsafe AudioFrame GenerateAudioData(uint samples)
		{
			uint bufferSize = samples * sizeof(float) * 2;
			var frame = new AudioFrame(bufferSize);
			_buffer = new short[samples * 2];
			using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
			using (IMemoryBufferReference reference = buffer.CreateReference())
			{
				byte* dataInBytes;
				uint capacityInBytes;
				float* dataInFloat;
				((IMemoryBufferByteAccess) reference).GetBuffer(out dataInBytes, out capacityInBytes);
				dataInFloat = (float*) dataInBytes;
				_player.GetBuffer(_buffer);

				for (var i = 0; i < _buffer.Length; i++)
				{
					dataInFloat[i] = _buffer[i] / 32767f;
				}

			}

			return frame;
		}

		public async Task<Stream> GetFileAsStreamAsync(StorageFile file) => (await file.OpenReadAsync()).AsStream();
	}

	class FileSystem : IFileSystem
	{
		public IFolder BaseFolder => new Folder(Package.Current.InstalledLocation);
		public IFolder LocalFolder => new Folder(ApplicationData.Current.LocalFolder);
	}

	class Folder : IFolder
	{
		public string Name { get; }
		public string Path { get; }

		private readonly StorageFolder _folder;

		public Folder(StorageFolder sf)
		{
			if (sf == null)
				throw new ArgumentNullException(nameof(sf));
			_folder = sf;
			Name = sf.Name;
			Path = sf.Path;
		}

		public async Task<IFile> GetFileAsync(string name) => new File(await _folder.GetFileAsync(name));

		public async Task<IList<IFile>> GetFilesAsync() => (await _folder.GetFilesAsync()).Select(sf => (IFile)new File(sf)).ToList();

		public async Task<IFolder> GetFolderAsync(string name) => new Folder(await _folder.GetFolderAsync(name));

		public async Task<IList<IFolder>> GetFoldersAsync()
			=> (await _folder.GetFoldersAsync()).Select(sf => (IFolder) new Folder(sf)).ToList();
	}

	class File : IFile
	{
		public string Name { get; }
		public string Path { get; }

		private readonly StorageFile _file;

		public File(StorageFile file)
		{
			if (file == null)
				throw new ArgumentNullException(nameof(file));
			_file = file;
			Path = file.Path;
			Name = System.IO.Path.GetFileName(Path);
		}

		public async Task<Stream> OpenAsync(FileAccessMode fileAccess)
		{
			switch (fileAccess)
			{
				case FileAccessMode.Read:
					return (await _file.OpenAsync(Windows.Storage.FileAccessMode.Read)).AsStream();
				case FileAccessMode.ReadAndWrite:
					return (await _file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite)).AsStream();
				default:
					throw new ArgumentOutOfRangeException(nameof(fileAccess), fileAccess, null);
			}
		}


	}

}