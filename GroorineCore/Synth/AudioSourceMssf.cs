using System;
using GroorineCore.DataModel;
using GroorineCore.Helpers;

namespace GroorineCore.Synth
{

	public class AudioSourceMssf : IAudioSource
	{
		/// <summary>
		/// 波形データを取得します．
		/// </summary>
		public short[] Wave { get; }

		/// <summary>
		/// エンベロープデータを取得します．
		/// </summary>
		public Envelope Envelope { get; }

		/// <summary>
		/// -256 ～ 255 の範囲をとるパンポットを取得します。
		/// </summary>
		public int Pan { get; }

		internal AudioSourceMssf(short[] wave, Envelope envelope, int pan)
		{
			if (wave == null)
				throw new ArgumentNullException(nameof(wave));
			if (wave.Length != 32)
				throw new ArgumentException("波形の大きさは32でなければなりません。");
			Wave = wave;
			Envelope = envelope;
			Pan = pan;
			
		}


		public ValueTuple<short, short> GetSample(int index, double sampleRate)
		{
			var time = MidiTimingConverter.GetTime(index, (int)sampleRate);


			var i = (int)((index % (100)) * (32 * 0.01));
			if (i > 31)
				i -= 32;
			var o = Wave[i];
			float vol;
			EnvelopeFlag flag = EnvelopeFlag.Attack;
			if (time > Envelope.A)
				flag = EnvelopeFlag.Decay;
			if (time > Envelope.A + Envelope.D)
				flag = EnvelopeFlag.Sustain;
			switch (flag)
			{
				case EnvelopeFlag.Attack:
					vol = (float)MathHelper.Linear(time, 0, Envelope.A, 0, 1);
					break;
				case EnvelopeFlag.Decay:
					vol = (float)MathHelper.Linear(time, Envelope.A, Envelope.A + Envelope.D, 1, Envelope.S * 0.0039);
					break;
				case EnvelopeFlag.Sustain:
					vol = Envelope.S * 0.0039f;
					break;
				case EnvelopeFlag.Release:
				case EnvelopeFlag.None:
				default:
					vol = 0;
					break;
			}
			short a = (short)(Math.Min(short.MaxValue, Math.Max(short.MinValue, o * vol)));
			return new ValueTuple<short, short>(a, a);

		}
	}
}