using System;
using GroorineCore.DataModel;
using GroorineCore.Helpers;

namespace GroorineCore.Synth
{

	public class Mssf : IAudioSource
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

		internal Mssf(short[] wave, Envelope envelope, int pan)
		{
			if (wave == null)
				throw new ArgumentNullException(nameof(wave));
			if (wave.Length != 32)
				throw new ArgumentException("波形の大きさは32でなければなりません。");
			Wave = wave;
			Envelope = envelope;
			Pan = pan;
			
		}


		public (short l, short r) GetSample(int index, double sampleRate)
		{
			var time = MidiTimingConverter.GetTime(index, (int)sampleRate);
			//var cycle = sampleRate / freq;
			//if (cycle == 0)
			//	return (0, 0);

			var i = (int)((index % (100)) * (32 / 100d));
			//var i = index;
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
					vol = (float)MathHelper.Linear(time, Envelope.A + 1, Envelope.D, 1, Envelope.S / 255d);
					break;
				case EnvelopeFlag.Sustain:
					vol = Envelope.S / 255f;
					break;
				case EnvelopeFlag.Release:
				case EnvelopeFlag.None:
				default:
					vol = 0;
					break;
			}
			return ((short)(o * vol), (short)(o * vol));

		}
	}
}