using Groorine.Events;
using Groorine.Helpers;

namespace Groorine.DataModel
{


	public class Tone : BindableBase
	{
		private EnvelopeFlag _envFlag;
		private double _tick;
		private int _sampleTick;
		private double _startTick;
		private double _envVolume;
		private int _freq;

		public byte NoteNum { get; }

		public EnvelopeFlag EnvFlag
		{
			get { return _envFlag; }
			set { SetProperty(ref _envFlag, value); }
		}
		public byte Velocity { get; }
		public double Tick
		{
			get { return _tick; }
			set { SetProperty(ref _tick, value); }
		}

		public double StartTick
		{
			get { return _startTick; }
			set { SetProperty(ref _startTick, value); }
		}

		public double RealTick => Tick - StartTick;

		public int SampleTick
		{
			get { return _sampleTick; }
			set { SetProperty(ref _sampleTick, value); }
		}

		public double EnvVolume
		{
			get { return _envVolume; }
			set { SetProperty(ref _envVolume, value); }
		}

		public int Frequency
		{
			get => _freq;
			set => SetProperty(ref _freq, value);
		}

		public long Gate { get; }

		public int Channel { get; }
		/// <summary>
		/// 対応する <see cref="NoteEvent"/> を指定して、 <see cref="Tone"/> クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="e">対応する <see cref="NoteEvent"/></param>
		public Tone(NoteEvent e)
		{
			Velocity = e.Velocity;
			Tick = 0;
			NoteNum = e.Note;
			Channel = e.Channel;
			EnvFlag = EnvelopeFlag.Attack;
			Gate = e.Gate;
			EnvVolume = 1;
		}

		/// <summary>
		/// 指定した <see cref="Tone"/> のコピーとして、新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="t">コピー対象の <see cref="Tone"/>。</param>
		public Tone(Tone t)
		{
			Velocity = t.Velocity;
			Tick = t.Tick;
			NoteNum = t.NoteNum;
			Channel = t.Channel;
			EnvFlag = t.EnvFlag;
			Gate = t.Gate;
			EnvVolume = t.EnvVolume;
		}

	}
}