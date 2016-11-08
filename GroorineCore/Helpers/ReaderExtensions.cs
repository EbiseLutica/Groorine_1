using System;
using System.IO;

namespace GroorineCore
{
	internal static class ReaderExtensions
	{
		// Note this MODIFIES THE GIVEN ARRAY then returns a reference to the modified array.
		public static byte[] Reverse(this byte[] b)
		{
			Array.Reverse(b);
			return b;
		}

		public static bool And(int a, int b) => (a & b) == b;

		public static ushort ReadUInt16BE(this BinaryReader binRdr) => BitConverter.ToUInt16(binRdr.ReadBytesRequired(sizeof(ushort)).Reverse(), 0);

		public static short ReadInt16BE(this BinaryReader binRdr) => BitConverter.ToInt16(binRdr.ReadBytesRequired(sizeof(short)).Reverse(), 0);

		public static uint ReadUInt32BE(this BinaryReader binRdr) => BitConverter.ToUInt32(binRdr.ReadBytesRequired(sizeof(uint)).Reverse(), 0);

		public static int ReadInt32BE(this BinaryReader binRdr) => BitConverter.ToInt32(binRdr.ReadBytesRequired(sizeof(int)).Reverse(), 0);

		public static int ReadVariableLength(this BinaryReader br, ref int count)
		{
			int value;
			byte c;
			if (And(value = br.ReadByte(), 0x80))
			{
				value &= 0x7f;
				do
				{
					value = (value << 7) + ((c = br.ReadByte()) & 0x7f);
					count++;
				} while (And(c, 0x80)); 
			}
			count++;
			return value;
		}
		
		

		public static byte[] ReadBytesRequired(this BinaryReader binRdr, int byteCount)
		{
			byte[] result = binRdr.ReadBytes(byteCount);

			if (result.Length != byteCount)
				throw new EndOfStreamException($"{byteCount} bytes required from stream, but only {result.Length} returned.");

			return result;
		}
	}
}