using GroorineCore;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace GroorineTest
{


	class Program
	{

		static BufferedWaveProvider bwp;
		static Player player;
		static void Main(string[] args)
		{
			
			bwp = new BufferedWaveProvider(new WaveFormat(44100, 16, 2));
			MMDevice mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

			player = new Player(50);
			//player.BufferCallBack += Player_BufferCallBack;
			//player.Play();

			Task t = Playing();

			using (IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 100))
			{
				wavPlayer.Init(bwp);
				wavPlayer.Play();
				Console.WriteLine("キーを押すと終了");
				Console.CancelKeyPress += (sender, e) =>
				{
					//player.Stop();
					wavPlayer.Stop();
					Environment.Exit(0);
				};
				for (;;)
				{
					Thread.Sleep(1);
					Console.Write($"{bwp.BufferedBytes}\t\t{bwp.BufferLength}\t\t{player.Time}\t\t{player.CurrentFile?.Length}\t\t{delta}\t\r");
				}
			}

			
		}

		private static int delta;

		private static async Task Playing()
		{
			player.Load(SmfParser.Parse(File.OpenRead("C020.mid")));
			player.Play();
			while (true)
			{
				var t = Environment.TickCount;
				short[] buffer = await player.GetBufferAsync();
				delta = Environment.TickCount - t;
				var ms = new MemoryStream();
				var size = buffer.Length * sizeof(short);
				byte[] b = new byte[size];

				unsafe
				{
					fixed (short* psrc = &buffer[0])
					{
						using (var strmSrc = new UnmanagedMemoryStream((byte*)psrc, size))
							strmSrc.Read(b, 0, size);
					}
				}

				bwp.AddSamples(b, 0, size);
				while (bwp.BufferedBytes > buffer.Length * 2)
					await Task.Delay(1);
				//await Task.Delay(1);
			}
		}

		private static void Player_BufferCallBack(object sender, short[] buffer)
		{
			
			var ms = new MemoryStream();
			var size = buffer.Length * sizeof(short);
			byte[] b = new byte[size];
			
			unsafe
			{
				fixed (short* psrc = &buffer[0])
				{
					using (var strmSrc = new UnmanagedMemoryStream((byte*)psrc, size))
						strmSrc.Read(b, 0, size);
				}
			}

			bwp.AddSamples(b, 0, size);
		}
	}
}
