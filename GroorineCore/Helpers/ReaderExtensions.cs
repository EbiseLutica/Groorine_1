using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Groorine.Helpers
{
	internal static class BinaryReaderAndWriterExtension
	{
		// Note this MODIFIES THE GIVEN ARRAY then returns a reference to the modified array.
		public static byte[] Reverse(this byte[] b)
		{
			Array.Reverse(b);
			return b;
		}
		
		public static string ReadString(this BinaryReader reader, int length) => new string(reader?.ReadChars(length));

		public static void WriteString(this BinaryWriter writer, string text) => writer.Write(text.ToCharArray());

		public static bool And(int a, int b) => (a & b) == b;

		public static ushort ReadUInt16BE(this BinaryReader reader) => BitConverter.ToUInt16(reader.ReadBytesRequired(sizeof(ushort)).Reverse(), 0);

		public static short ReadInt16BE(this BinaryReader reader) => BitConverter.ToInt16(reader.ReadBytesRequired(sizeof(short)).Reverse(), 0);

		public static uint ReadUInt32BE(this BinaryReader reader) => BitConverter.ToUInt32(reader.ReadBytesRequired(sizeof(uint)).Reverse(), 0);

		public static int ReadInt32BE(this BinaryReader reader) => BitConverter.ToInt32(reader.ReadBytesRequired(sizeof(int)).Reverse(), 0);

		public static void WriteBE(this BinaryWriter writer, ushort v) => writer.Write(BitConverter.GetBytes(v).Reverse());

		public static void WriteBE(this BinaryWriter writer, short v) => writer.Write(BitConverter.GetBytes(v).Reverse());

		public static void WriteBE(this BinaryWriter writer, uint v) => writer.Write(BitConverter.GetBytes(v).Reverse());

		public static void WriteBE(this BinaryWriter writer, int v) => writer.Write(BitConverter.GetBytes(v).Reverse());

		public static int ReadVariableLength(this BinaryReader reader, ref int count)
		{
			int value;
			byte c;
			if (And(value = reader.ReadByte(), 0x80))
			{
				value &= 0x7f;
				do
				{
					value = (value << 7) + ((c = reader.ReadByte()) & 0x7f);
					count++;
				} while (And(c, 0x80)); 
			}
			count++;
			return value;
		}

		public static void WriteVariableLength(this BinaryWriter writer, int num)
		{
			var buf = new List<int>();

			buf.Add((num & 0x7f) + 0x80);
			num >>= 7;
			while (num > 0)
			{
				buf.Add((num & 0x7f) + 0x80);
				num >>= 7;
			}
			buf[buf.Count - 1] ^= 0b10000000;
			buf.Reverse();
			writer.Write(buf.Select(BitConverter.GetBytes).SelectMany(b => b).ToArray());
		}

		public static byte[] ReadBytesRequired(this BinaryReader reader, int byteCount)
		{
			byte[] result = reader.ReadBytes(byteCount);

			if (result.Length != byteCount)
				throw new EndOfStreamException($"{byteCount} bytes required from stream, but only {result.Length} returned.");

			return result;
		}
	}
}