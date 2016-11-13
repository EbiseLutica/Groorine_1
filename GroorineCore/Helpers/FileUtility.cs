using System;
using System.IO;
using GroorineCore.DataModel;
using GroorineCore.Synth;

namespace GroorineCore.Helpers
{

	public static class FileUtility
	{


		public static Mssf LoadMssf(Stream stream)
		{
			var br = new BinaryReader(stream);
			char[] magic = br.ReadChars(8);
			if (new string(magic) != "MSSF_VER")
				throw new Exception("マジックナンバーが一致しません。");
			br.ReadBytes(3);
			short[] wave = new short[32];
			for (var i = 0; i < 32; i++)
				wave[i] = br.ReadInt16();

			var env = new Envelope
				(
					a: br.ReadInt32(),
					d: br.ReadInt32(),
					s: br.ReadByte(),
					r: br.ReadInt32()
				);

			var pan = br.ReadInt32();

			return new Mssf(wave, env, pan);
		}
	}

}
