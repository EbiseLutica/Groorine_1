using System;
using System.Threading;
using static System.Math;
using static GroorineCore.MathHelper;
using System.Threading.Tasks;
using System.Linq;

namespace GroorineCore
{

	public delegate void BufferCallbackEventHandler(object sender, short[] buffer);

	/// <summary>
	/// Groorine プロジェクトファイルを読み込み、再生する機能を提供します。
	/// </summary>
	public class Player : BindableBase
	{
		
		private long _time;
		private bool _isPlaying;
		private short[] _buffer;
		//private int _bufptr;
		private int _preTick;
		//private Timer _timer;
		//private EventWaitHandle _eventWaitHandle;
		//private AudioTimer _cycler;
		private int _timerSpan;
		//private int _bufferingTick;
		/// <summary>
		/// バッファを生成するためにかかる時間を取得します。単位はミリ秒。
		/// </summary>
		public int Latency { get; }
		/// <summary>
		/// バッファの実際のサイズを取得します。
		/// </summary>
		public int BufferSize { get; }
		/// <summary>
		/// 現在の <see cref="Player"/> のサンプリング周波数を取得します。
		/// </summary>
		public int SampleRate { get; }
		/// <summary>
		/// バッファの再生準備が完了したときに発生します。
		/// </summary>
		public event BufferCallbackEventHandler BufferCallBack;
		/// <summary>
		/// 読み込まれた Groorine プロジェクトファイルを取得します。
		/// </summary>
		public GroorineFile CurrentFile { get; private set; }



		/// <summary>
		/// サンプル周波数、バッファサイズを指定してインスタンスを初期化します。
		/// </summary>
		/// <param name="latency">再生にかかる時間。単位はミリ秒。</param>
		/// <param name="sampleRate">再生時のサンプリング周波数。</param>
		public Player(int latency = 50, int sampleRate = 44100)
		{
			SampleRate = sampleRate;
			Latency = latency;
			BufferSize = (int)(Latency * SampleRate * 0.001 * 2);
			_buffer = new short[BufferSize];
			//_eventWaitHandle = new ManualResetEvent(false);
			//_cycler = new AudioTimer();
		//	_timer = new Timer(CallBack, _eventWaitHandle, Timeout.Infinite, Timeout.Infinite);
		}

		/// <summary>
		/// 内部のタイマーが呼び出すコールバックです。
		/// </summary>
		/// <param name = "state" ></ param >
		//private void CallBack(object state)
		//{
		//	var ewh = state as EventWaitHandle;
		//	ewh?.Reset();
		//	if (_buffer != null)
		//	{
		//		_cycler.SetCycle(441, SampleRate);
		//		/*for (var i = 0; i < Latency; i++)
		//		{

		//			 サンプル周波数 * ミリ秒単位 * バッファ時間 * ステレオチャンネル分
		//			int getPtr(int time) => (int)(SampleRate * 0.001 * time * 2);
		//			var ptr = getPtr(i);
		//			var x = _cycler.Update();

		//			_buffer[ptr] = _buffer[ptr + 1] = (short)(Sin(ToRadian(x * 360)) * short.MaxValue / 2);

		//			if (prevPtr is int pp)
		//			{
		//				for (var j = pp; j <= ptr; j += 2)
		//				{
		//					_buffer[j] = (short)Linear((ptr - j) / (double)(ptr - pp), _buffer[pp], _buffer[ptr]);
		//				}
		//			}
		//			prevPtr = ptr;
		//			Time++;
		//		}*/
		//		double firstTime = Time;
		//		for (var i = 0; i < BufferSize; i += 2)
		//		{

		//			Time = (long)(firstTime + time);

		//		}
		//		OnBufferCompleted();
		//		/*_bufferingTick += 2;
		//		if (_bufferingTick >= BufferSize)
		//		{
		//			OnBufferCompleted();
		//			_bufferingTick = 0;

		//		}*/
		//	}

		//	ewh?.Set();
		//	_timer.Change(Latency, Timeout.Infinite);
		//	;
		//}


		public async Task<short[]> GetBufferAsync()
		{
			short[] buf = new short[BufferSize];

			if (!IsPlaying)
				return buf;

			//_cycler.SetCycle(441, SampleRate);

			double firstTime = Time;

			double getTime(int index) => index / (SampleRate * 0.001 * 2);

			for (var i = 0; i < BufferSize; i += 2)
			{
				var time = getTime(i);
				//var x = _cycler.Update();
				Time = (long)(firstTime + getTime(i));
				var tick = CurrentFile.Conductor.ToTick((int)Time);
			
				if (CurrentFile != null)
				{
					if (tick >= CurrentFile.Length)
					{
						if (CurrentFile.LoopStart is int loop)
						{
							tick = _preTick = loop;
							Time = CurrentFile.Conductor.ToMilliSeconds(tick);
						}
						else
						{
							Stop();
							return buf;
						}
					}

					if (tick != _preTick)
					{
						foreach (GroorineCore.Track t in CurrentFile.Tracks)
						{
							if (tick > t.Length)
								continue;
							//foreach (MidiEvent me in t.GetDataBetweenTicks(_preTick, tick))
							foreach (MidiEvent me in t.Events)
							{
								if (_preTick <= me.Tick && me.Tick <= tick)
									Tracks[me.Channel]?.SendEvent(me, tick);
							}
						}
					}
				}

				
				(float, float) tmp = (0, 0);
				//foreach (Track t in Tracks)
				//{
				for (var ti = 0; ti < Track.Tones.Length; ti++)
				{
					Tone t = Track.Tones[ti];
					if (t == null)
						continue;
					if (t.RealTick > t.Gate)
					{
						Track.Tones[ti] = null;
						continue;
					}
					(float, float) sample = await Tracks[t.Channel].ProcessAsync(t, SampleRate);

					//(float, float) sample = (0, 0);
					tmp.Item1 += sample.Item1 / Track.Tones.Length;
					tmp.Item2 += sample.Item2 / Track.Tones.Length;
					t.Tick = tick;
				}
				//}

				buf[i] = (short)(tmp.Item1 * 32767);
				buf[i + 1] = (short)(tmp.Item2 * 32767);
				

				_preTick = tick;

				//buf[i] = buf[i + 1] = (short)(x % 100 < 50 ? -32768 : 32767);

			}
			return buf;
		}


		/// <summary>
		/// 指定したプロジェクトファイルを読み込み、再生の準備をします。現在再生中の場合は停止します。
		/// </summary>
		/// <param name="gf"></param>
		public void Load(GroorineFile gf)
		{
			//_eventWaitHandle?.WaitOne();
			Stop();
			CurrentFile = gf;
			Tracks = new Track[Constants.MaxChannelCount];
			for (var i = 0; i < Tracks.Length; i++)
				Tracks[i] = new Track();
		}

		/// <summary>
		/// バッファが満たされたときに内部的に呼ばれます。
		/// </summary>
		protected virtual void OnBufferCompleted()
		{
			BufferCallBack?.Invoke(this, _buffer);
		}

		/// <summary>
		/// プレイヤーの現在位置をミリ秒で取得します。
		/// </summary>
		public long Time
		{
			get { return _time; }
			set { SetProperty(ref _time, value); }
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
		public void Play()
		{
			IsPlaying = true;
			_buffer = new short[BufferSize];
			//_timer?.Change(0, Timeout.Infinite);
			_timerSpan = Latency / 2;
		}

		/// <summary>
		/// 再生中の <see cref="Player"/> を停止し、 <see cref="Time"/> を初期化します。
		/// </summary>
		public void Stop()
		{
			Pause();
			Time = 0;
		}

		/// <summary>
		/// 再生中の <see cref="Player"/> を一時停止します。 <see cref="Play()"/> を呼び出すと続きから再生します。
		/// </summary>
		public void Pause()
		{
			//_eventWaitHandle.WaitOne();
			IsPlaying = false;
			//_timer?.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public class Track
		{
			/// <summary>
			/// このトラックが使用する音色を取得または設定します。
			/// </summary>
			public int ProgramChange { get; set; }

			public async Task<IInstrument> GetCurrentInstrument() => (await AudioSourceManager.GetInstance()).Instruments[ProgramChange];



			/// <summary>
			/// このトラックの MIDI チャンネル情報を取得または設定します。
			/// </summary>
			public IChannel Channel { get; set; }

			public static Tone[] Tones { get; }

			private short[] rpns;

			static Track()
			{
				Tones = new Tone[Constants.MaxToneCount];
			}

			public Track()
			{
				Channel = new Channel();
				rpns = new short[4];
			}
			
			

			public void SendEvent(MidiEvent me, int tick)
			{
				switch (me)
				{
					case NoteEvent n:
						int? candiate = null;
						for (var i = 0; i < Tones.Length; i++)
							if (Tones[i] == null || (Tones[i].Channel == n.Channel && Tones[i].NoteNum == n.Note))
							{
								candiate = i;
								break;
							}
						if (candiate == null)
						{
							double max = 0;
							for (var i = 0; i < Tones.Length; i++)
							{
								Tone t = Tones[i];
								if (max < t?.RealTick)
								{
									candiate = i;
									max = t?.RealTick ?? 0;
								}
							}
							if (candiate == null)
								break;
						}
						Tones[candiate ?? 0] = new Tone(n) { StartTick = tick, Tick = tick };
						break;
					case ControlEvent c:
						if (!Enum.IsDefined(typeof(ControlChangeType), (int)c.ControlNo))
							break;
						var cc = (ControlChangeType)c.ControlNo;
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
							case ControlChangeType.DataMSB:
								rpns[2] = c.Data;
								switch (rpns[1])
								{
									case 0:
										Channel.BendRange = rpns[2];
										break;
									case 2:
										Channel.NoteShift = (short)(rpns[2] - 64);
										break;
								}
								break;
							case ControlChangeType.DataLSB:
								rpns[3] = c.Data;
								if (rpns[1] == 1)
									Channel.Tweak = (short)((rpns[2] << 7) + rpns[3] - 8192);
								break;
							case ControlChangeType.RPNLSB:
								rpns[1] = c.Data;
								break;
							case ControlChangeType.RPNMSB:
								rpns[0] = c.Data;
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
							case ControlChangeType.BankMSB:
								break;
							case ControlChangeType.BankLSB:
								break;
							case ControlChangeType.Modulation:
								break;
							default:
								break;
						}
						break;
					case PitchEvent p:
						Channel.Pitchbend = p.Bend;
						break;
					case ProgramEvent pg:
						ProgramChange = pg.ProgramNo;
						break;
				}
			}

			public async Task<(float, float)> ProcessAsync(Tone t, int sampleRate)
			{
				(float, float) output = (0, 0);
				IInstrument il = (await GetCurrentInstrument());
				if (il == null)
					return output;

				var panrt = Channel.Panpot / 127f;
				var volrt = Channel.Volume / 127f;
				var exprt = Channel.Expression / 127f;
				var kake = volrt * exprt;
				var i1 = kake * (1 - panrt);
				var i2 = kake * panrt;
				if (t == null)
					return output;
				/*for (var i = 0; i < Tones.Length; i++)
				{
					Tone t = Tones[i];
					if (t == null)
						continue;
						*/
					var freq = GetFreq(t.NoteNum) * Channel.FreqExts;

					var velrt = t.Velocity / 127f;
				/*
					//foreach (IInstrument ii in il.FindInstruments(t.NoteNum, t.Velocity))
					//{
					(float, float) tmp = il.Source.GetSample(t.SampleTick, sampleRate, freq);
						output.Item1 += tmp.Item1 * i1 * velrt;
						output.Item2 += tmp.Item2 * i2 * velrt;
					//}
				}*/
				t.SampleTick++;

				output = il.Source.GetSample(t.SampleTick, sampleRate, freq);
				output.Item1 *= i1 * velrt;
				output.Item2 *= i2 * velrt;

				return output;
			}


			public static float GetFreq(int noteno) => (float)(441 * Pow(2, (noteno - 69) / 12.0));

		}
	}

	public class AudioTimer
	{
		public AudioTimer()
		{
			step = 0;
			position = 0;
		}

		public AudioTimer(double cycle)
			: this()
		{
			SetCycle(cycle);
		}

		private double step;
		private double position;

		public void SetCycle(double cycle)
		{
			if (cycle > 0)
				step = Division / cycle;
			else
				step = 0;
		}

		const double Division = 100d;

		public void SetCycle(double freq, double samplerate)
		{
			SetCycle(samplerate / freq);
		}

		public double Update() => position = (position + step);

	}

}