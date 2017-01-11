using GroorineCore.DataModel;
using System;
namespace GroorineCore.Synth
{

	/// <summary>
	/// 同じ波形を繰り返し出力する音源を表す抽象クラスです。
	/// </summary>
	public abstract class AudioSourceWaveTable : IAudioSource
	{

		public ValueTuple<short, short> GetSample(int index, double sampleRate, Tone t) => GetSample(index % 100);

		public abstract ValueTuple<short, short> GetSample(int index);

	}


}