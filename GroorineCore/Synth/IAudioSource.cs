using System;
using System.IO;
using GroorineCore.Helpers;

namespace GroorineCore.Synth
{

	public interface IAudioSource
	{
		(short l, short r) GetSample(int index, double sampleRate);
	}

	/// <summary>
	/// 正弦波を出力する音源を表します。
	/// </summary>
	public class AudioSourceSine : AudioSourceWaveTable
	{
		public override (short l, short r) GetSample(int index)
		{
			var sample = (short)(Math.Sin(MathHelper.ToRadian(index * 360)) * 32767);
			return (sample, sample);
		}
	}



	public class AudioSourceWav : IAudioSource
	{

		public (short, short)[] Samples;

		private int _sampleRate;



		public AudioSourceWav(Stream wavFile)
		{
			var br = new BinaryReader(wavFile);

			// RIFF ヘッダー
			if (br.ReadString(4) != "RIFF")
				throw new InvalidDataException("このファイルは RIFF ではありません。");
			var size = br.ReadInt32();
			if (br.ReadString(4) != "WAVE")
				throw new InvalidDataException("このファイルは WAVE ではありません。");

			// fmt チャンク
			if (br.ReadString(4) != "fmt ")
				throw new InvalidDataException("fmt チャンクのマジックナンバーが不正です。");
			var fmtsize = br.ReadInt32();
			var formatid = br.ReadInt16();
			var chCount = br.ReadInt16();
			if (chCount != 1 && chCount != 2)
				throw new InvalidDataException("サポートされないチャンネル数です。 モノラル、ステレオのみサポートされます。");
			_sampleRate = br.ReadInt32();
			var dataRate = br.ReadInt32();
			var blockSize = br.ReadInt16();
			var bitRate = br.ReadInt16();
			if (bitRate != 8 && bitRate != 16)
				throw new InvalidDataException("サポートされないビットレートです。");
			bitRate /= 8;
			if (dataRate != _sampleRate * chCount * bitRate)
				throw new InvalidDataException("データ速度、サンプリングレート、ビットレートおよびチャンネル数が矛盾しています。");
			if (blockSize != chCount * bitRate)
				throw new InvalidDataException("ブロックサイズ、ビットレートおよびチャンネル数が矛盾しています。");
			if (fmtsize >= 18)
			{
				var extSize = br.ReadInt16();
				br.ReadBytes(extSize);
			}

			var s = br.ReadString(4);

			if (s == "fact")
			{
				var factsize = br.ReadInt32();
				br.ReadBytes(factsize);
				s = br.ReadString(4);
			}

			if (s != "data")
				throw new InvalidDataException("dataチャンクのマジックナンバーが不正です。");

			// dataチャンク
			var chunkSize = br.ReadInt32();
			var dataCount = chunkSize / bitRate / chCount;

			short l, r;
			Samples = new(short, short)[dataCount];
			for (var i = 0; i < dataCount; i++)
			{
				l = r = bitRate == 1 ? (short)((br.ReadByte() << 8) - 65536) : (br.ReadInt16());
				if (chCount == 2)
					r = bitRate == 1 ? (short)((br.ReadByte() << 8) - 65536) : (br.ReadInt16());
				Samples[i] = (l, r);
			}

		}


		public (short l, short r) GetSample(int index, double sampleRate)
		{
			var i = index;
			return Samples.Length <= i ? ((short, short))(0, 0) : Samples[i];
		}
	}

	/// <summary>
	/// 同じ波形を繰り返し出力する音源を表す抽象クラスです。
	/// </summary>
	public abstract class AudioSourceWaveTable : IAudioSource
	{

		public (short l, short r) GetSample(int index, double sampleRate) => GetSample(index % 100);

		public abstract (short l, short r) GetSample(int index);

	}


}