using System.Linq;
using Groorine.Api;
using Groorine.DataModel;
using Groorine.Events;
using Groorine.Helpers;
using Groorine.Synth;
using System;
using static System.Math;
using static Groorine.Helpers.MathHelper;


namespace Groorine
{

	public delegate void BufferCallbackEventHandler(object sender, short[] buffer);

	/// <summary>
	/// Groorine プロジェクトファイルを読み込み、再生する機能を提供します。
	/// </summary>
	public class Player : BindableBase
	{

		private long _time;
		private bool _isPlaying;
		private int _preTick;
		private bool _isPausing;

		/// <summary>
		/// 現在の <see cref="Player"/> のサンプリング周波数を取得します。
		/// </summary>
		public int SampleRate { get; }
		/// <summary>
		/// 読み込まれた Groorine プロジェクトファイルを取得します。
		/// </summary>
		public MidiFile CurrentFile { get; private set; }



		/// <summary>
		/// ループ回数を取得します。
		/// 0の場合はループせず、-1の場合は無限ループします。
		/// </summary>
		public int LoopCount { get; private set; }
		public int? FadeOutTick { get; private set; }

		public int FadeOutTime { get; private set; } = 2000;

		/// <summary>
		/// サンプル周波数、バッファサイズを指定してインスタンスを初期化します。
		/// </summary>
		/// <param name="latency">再生にかかる時間。単位はミリ秒。</param>
		/// <param name="sampleRate">再生時のサンプリング周波数。</param>
		public Player(int sampleRate = 44100)
		{
			SampleRate = sampleRate;

			Tracks = new Track[Constants.MaxChannelCount];
			for (var i = 0; i < Tracks.Length; i++)
				Tracks[i] = new Track();
			OnPropertyChanged(nameof(Tracks));
		}

		public int Tick { get; set; }

		double GetTime(int index) => index / (SampleRate * 0.001 * 2);

		public short[] GetBuffer(short[] buf)
		{
			if (buf == null)
				return null; // ぬるぬる！！！！

			if (!IsPlaying)
			{
				for (var i = 0; i < buf.Length; i++)
					buf[i] = 0;
				return buf;
			}

			double firstTime = Time;
			
			for (var i = 0; i < buf.Length; i += 2)
			{
				double realTime = firstTime + GetTime(i);
				Time = (long)(realTime + 0.5);
				Tick = (int)CurrentFile.Conductor.ToTick(realTime);

				buf[i] = buf[i + 1] = 0;

				if (CurrentFile != null)
				{
					if (Tick >= CurrentFile.Length)
					{
						if (CurrentFile.LoopStart is long)
						{
							long loop = CurrentFile.LoopStart.Value;
							Tick = _preTick = (int)loop;
							realTime = CurrentFile.Conductor.ToMilliSeconds(Tick);
							
							firstTime = realTime - GetTime(i);

							_preTick = Tick = (int)CurrentFile.Conductor.ToTick(realTime);

							_preTick--;
							if (LoopCount > 0)
								LoopCount--;
							if (LoopCount == 0 && FadeOutTick == null)
								FadeOutTick = FadeOutTime;
							ToneInit();
						}
						else
						{
							Stop();

							return buf;
						}
					}

					
					if (Tick != _preTick)
					{
						foreach (DataModel.Track t in CurrentFile.Tracks)
						{
							if (Tick > t.Length)
								continue;
							foreach (MidiEvent me in t.Events)
							{
								if (_preTick < me.Tick && me.Tick <= Tick)
									Tracks[me.Channel]?.SendEvent(me, Tick);
							}
						}
					}

					for (var ti = 0; ti < Track.Tones.Length; ti++)
					{
						Tone t = Track.Tones[ti];

						if (t == null)
							continue;


						t.Tick = Tick;
						if (t.RealTick >= t.Gate - 1)
						{
							Track.Tones[ti] = null;
							continue;
						}

						ValueTuple<short, short> sample = Tracks[t.Channel].Process(ti, SampleRate);

						buf[i] += sample.Item1;
						buf[i + 1] += sample.Item2;

					}


				}


				if (FadeOutTick is int)
				{
					int fot = FadeOutTick.Value;
					buf[i] = (short)(buf[i] * Linear(fot, 0, FadeOutTime, 0, 1));
					buf[i + 1] = (short)(buf[i + 1] * Linear(fot, 0, FadeOutTime, 0, 1));
					FadeOutTick--;
					if (fot <= 0)
						Stop();
				}


				_preTick = Tick;
			

			}
			return buf;
		}


		public short[] CreateBuffer(int latency) => new short[(int)(latency * SampleRate * 0.001) * 2];

		/// <summary>
		/// 指定したプロジェクトファイルを読み込み、再生の準備をします。現在再生中の場合は停止します。
		/// </summary>
		/// <param name="gf"></param>
		public void Load(MidiFile gf)
		{
			if (gf == null)
				return;
			Stop();
			CurrentFile = gf;
			OnPropertyChanged(nameof(MaxTime));

		}


		/// <summary>
		/// プレイヤーの現在位置をミリ秒で取得します。
		/// </summary>
		public long Time
		{
			get { return _time; }
			set { SetProperty(ref _time, value); }
		}

		public long MaxTime => (long)(CurrentFile?.Conductor.ToMilliSeconds((int) CurrentFile.Length) ?? 0);

		public bool IsPausing
		{
			get { return _isPausing; }
			set { SetProperty(ref _isPausing, value); }
		}

		/// <summary>
		/// プレイヤーが再生中であるかどうかを取得します。
		/// </summary>
		public bool IsPlaying
		{
			get { return _isPlaying; }
			private set { SetProperty(ref _isPlaying, value); }
		}

		public Track[] Tracks { get; private set; }

		/// <summary>
		/// 読み込まれたプロジェクトファイルの再生を開始します。
		/// </summary>
		public void Play(int loopCount = -1, int fadeOutTime = 2000)
		{
			if (!IsPausing && !IsPlaying)
			{
				Time = Tick = 0;
				_preTick = -1;
				ToneInit();
				TrackReset();
			}
			IsPlaying = true;
			IsPausing = false;
			LoopCount = loopCount;
			FadeOutTime = (int)(fadeOutTime * SampleRate * 0.001);

		}

		public void TrackReset()
		{
			foreach (Track t in Tracks)
			{
				t.ProgramChange = 0;
				t.Channel = new Channel();
				t.Rpns.Initialize();
			}
		}

		/// <summary>
		/// 再生中の <see cref="Player"/> を停止し、 <see cref="Time"/> を初期化します。
		/// </summary>
		public void Stop()
		{
			IsPlaying = false;
			Time = 0;
			ToneInit();
			_preTick = -1;
			FadeOutTick = null;
		}

		void ToneInit()
		{

			for (var i = 0; i < Track.Tones.Length; i++)
				Track.Tones[i] = null;
		}



		/// <summary>
		/// 再生中の <see cref="Player"/> を一時停止します。 <see cref="Play()"/> を呼び出すと続きから再生します。
		/// </summary>
		public void Pause()
		{
			IsPausing = true;
			IsPlaying = false;
		}


		public class Track
		{
			/// <summary>
			/// このトラックが使用する音色を取得または設定します。
			/// </summary>
			public int ProgramChange { get; set; }

			public IInstrument GetCurrentInstrument() => (AudioSourceManager.GetInstance()).Instruments[ProgramChange];

			public InstrumentList GetDrumsets() => (AudioSourceManager.GetInstance()).Drumset;


			/// <summary>
			/// このトラックの MIDI チャンネル情報を取得または設定します。
			/// </summary>
			public IChannel Channel { get; set; }

			public static Tone[] Tones { get; }

			public static AudioTimer[] AudioTimers { get; }

			static readonly float[] FreqTable;

			public readonly short[] Rpns;
			

			static Track()
			{
				Tones = new Tone[Constants.MaxToneCount];
				FreqTable = Enumerable.Range(0, 128).Select(GetFreq).ToArray();
				AudioTimers = new AudioTimer[Constants.MaxToneCount];
				for (var i = 0; i < AudioTimers.Length; i++)
					AudioTimers[i] = new AudioTimer();
			}

			public Track()
			{
				Channel = new Channel();
				Rpns = new short[4];
			}

			private static int _tonePtr { get; set; }

			public void SendEvent(MidiEvent me, int tick)
			{
				if (me is NoteEvent)
				{



					var value = Tones.TakeWhile(t => !(t == null || t.RealTick >= t.Gate || (t.Channel == me.Channel && t.NoteNum == (me as NoteEvent).Note))).Count();


					if (value != Tones.Length)
						_tonePtr = value;
					else
					{
						ValueTuple<Tone, int> value2 = Tones.Select((t, i) => new ValueTuple<Tone, int>(t, i)).Where(ti => ti.Item1 != null).OrderBy((ti) => ti.Item1.StartTick).FirstOrDefault();
						if (value2.Item1 != null)
							_tonePtr = value2.Item2;
					}
					Tones[_tonePtr] = new Tone(me as NoteEvent) { StartTick = tick, Tick = tick };
					AudioTimers[_tonePtr].Reset();

					_tonePtr = (_tonePtr + 1) % Tones.Length;

				}
				else if (me is ControlEvent)
				{
					var c = (ControlEvent)me;
					var cc = c.ControlNo;
					switch (cc)
					{
						case ControlChangeType.Volume:
							Channel.Volume = c.Data;
							break;
						case ControlChangeType.Panpot:
							Channel.Panpot = c.Data;
							break;
						case ControlChangeType.Expression:
							Channel.Expression = c.Data;
							break;
						case ControlChangeType.DataMsb:
							Rpns[2] = c.Data;
							switch (Rpns[1])
							{
								case 0:
									Channel.BendRange = Rpns[2];
									break;
								case 2:
									Channel.NoteShift = (short)(Rpns[2] - 64);
									break;
							}
							break;
						case ControlChangeType.DataLsb:
							Rpns[3] = c.Data;
							if (Rpns[1] == 1)
								Channel.Tweak = (short)((Rpns[2] << 7) + Rpns[3] - 8192);
							break;
						case ControlChangeType.Rpnlsb:
							Rpns[1] = c.Data;
							break;
						case ControlChangeType.Rpnmsb:
							Rpns[0] = c.Data;
							break;
						case ControlChangeType.HoldPedal:
							break;
						case ControlChangeType.Reverb:
							break;
						case ControlChangeType.Chorus:
							break;
						case ControlChangeType.Delay:
							break;
						case ControlChangeType.AllSoundOff:
							break;
						case ControlChangeType.ResetAllController:
							break;
						case ControlChangeType.AllNoteOff:
							break;
						case ControlChangeType.Mono:
							break;
						case ControlChangeType.Poly:
							break;
						case ControlChangeType.BankMsb:
							break;
						case ControlChangeType.BankLsb:
							break;
						case ControlChangeType.Modulation:
							break;
						default:
							break;
					}
				}
				else if (me is PitchEvent)
				{
					var p = (PitchEvent)me;
					Channel.Pitchbend = p.Bend;
				}
				else if (me is ProgramEvent)
				{
					var pg = (ProgramEvent)me;
					ProgramChange = pg.ProgramNo;
				}
			}

			public ValueTuple<short, short> Process(int index, int sampleRate)
			{
				Tone t = Tones[index];
				AudioTimer at = AudioTimers[index];
				ValueTuple<short, short> output = new ValueTuple<short, short>(0, 0);
				if (t == null)
					return output;
				IInstrument il;
				il = t.Channel == 9 ? GetDrumsets().FindInstruments(t.NoteNum, t.Velocity).FirstOrDefault() : GetCurrentInstrument();


				if (il == null)
					return output;

				var panrt = Channel.Panpot * 0.0078f;
				var a = 4f / Tones.Length;
				if (t.Channel == 9) a *= 2;

				var kake = (Channel.Volume * 0.0078f) * (Channel.Expression * 0.0078f) * (t.Velocity * 0.0078f) * a;

				var freq = FreqTable[t.NoteNum] * Channel.FreqExts;
				t.Frequency = (int)freq;
				if (t.Channel == 9)
					freq = 441 * Channel.FreqExts;
				at.SetCycle(freq, sampleRate);

				output = il.Source.GetSample(t.SampleTick, sampleRate, t);
				output.Item1 = (short)(output.Item1 * kake * (1 - panrt));
				output.Item2 = (short)(output.Item2 * kake * panrt);

				t.SampleTick = (int)at.Update();
				return output;
			}
			
			public static float GetFreq(int noteno) => (float)(441 * Pow(2, (noteno - 69) / 12.0));

		}
		
	}

	public class AudioTimer
	{
		public AudioTimer()
		{
			_step = 0;
			_position = 0;
		}

		public void Reset()
		{
			_position = 0;
		}

		public AudioTimer(double cycle)
			: this()
		{
			SetCycle(cycle);
		}

		private double _step;
		private double _position;

		public void SetCycle(double cycle)
		{
			if (cycle > 0)
				_step = Division / cycle;
			else
				_step = 0;
		}

		const double Division = 100d;

		public void SetCycle(double freq, double samplerate)
		{
			SetCycle(samplerate / freq);
		}

		public double Update() => _position = (_position + _step);

	}

}