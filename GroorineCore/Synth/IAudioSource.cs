using System;
using static GroorineCore.MathHelper;
namespace GroorineCore
{

	public interface IAudioSource
	{
		(float l, float r) GetSample(int index, double sampleRate, double freq);
	}

	/// <summary>
	/// 正弦波を出力する音源を表します。
	/// </summary>
	public class AudioSourceSine : AudioSourceWaveform
	{
		public override (float l, float r) GetSample(int index, double cycle)
		{
			var sample = (float)Math.Sin(ToRadian(index * (360 / cycle)));
			return (sample, sample);
		}

	}

	/// <summary>
	/// 同じ波形を繰り返し出力する音源を表す抽象クラスです。
	/// </summary>
	public abstract class AudioSourceWaveform : IAudioSource
	{
		public (float l, float r) GetSample(int index, double sampleRate, double freq)
		{
			var cycle = sampleRate / freq;
			if (cycle == 0)
				return (0, 0);
			index %= (int)cycle;
			return GetSample(index, cycle);
		}

		public abstract (float l, float r) GetSample(int index, double cycle);

	}


}