using GroorineCore;
using GroorineCore.DataModel;
using GroorineCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groorine.WPF
{
	internal class MainWindowViewModel : BindableBase
	{

		public MainWindowViewModel()
		{

		}
	}


	internal class ChannelViewModel : BindableBase
	{
		private int _volume;
		private int _expression;
		private double _envVolume;

		private int _freq;
		private int _pan;
		private int _pitchBend;

		public double EnvVolume
		{
			get => _envVolume;
			set
			{
				SetProperty(ref _envVolume, value);
				OnPropertyChanged(nameof(Output));
			}
		}

		public int Volume
		{
			get => _volume;
			set
			{
				SetProperty(ref _volume, value);
				OnPropertyChanged(nameof(Output));
			}
		}

		public int Expression
		{
			get => _expression;
			set
			{
				SetProperty(ref _expression, value);
				OnPropertyChanged(nameof(Output));
			}
		}

		public int Output => (int)((Volume + Expression) / 255d * EnvVolume);

		public int PitchBend
		{
			get => _pitchBend;
			set => SetProperty(ref _pitchBend, value);
		}
		public int Panpot
		{
			get => _pan;
			set => SetProperty(ref _pan, value);
		}
		public int Frequency
		{
			get => _freq;
			set => SetProperty(ref _freq, value);
		}

		public int ChannelNo { get; }

		public ChannelViewModel(int chNo, Player.Track track)
		{
			ChannelNo = chNo;
			Update(track.Channel);
		}

		public void Update(IChannel ch)
		{
			Volume = ch?.Volume ?? 0;
			Expression = ch?.Expression ?? 0;
			Panpot = ch?.Panpot ?? 64;
			PitchBend = ch?.Pitchbend ?? 0;

			Tone sample = Player.Track.Tones.FirstOrDefault(t => t?.Channel == ChannelNo);
			if (sample == null)
			{
				Frequency = 0;
				EnvVolume = 0;
				return;
			}
			Frequency = sample.Frequency;
			EnvVolume = sample.EnvVolume;
		}
	}


}
