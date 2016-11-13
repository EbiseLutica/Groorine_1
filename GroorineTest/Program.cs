using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GroorineCore.DataModel;
using GroorineCore.DotNet45;
using GC = GroorineCore;

namespace GroorineTest
{

	class Program
	{

		static Player _player;
		private static bool _useAutoPlay;
		static void Main(string[] args)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;


			Console.WriteLine(@"Groorine Test Console");

			Console.Write("Do you want to use auto play?(Y/N) >");
			var r = Console.ReadKey();
			_useAutoPlay = r.Key == ConsoleKey.Y;

			string Arrow(int length, double rate)
			{
				int a = (int)(length * rate);
				int b = length - a;
				return (a > 1 ? new string('=', a - 1) : "") + (a > 0 ? ">" : "") + (b > 0 ? new string('-', b) : "");
			}

			_player = new Player();

			Console.CancelKeyPress += async (s, e) =>
			{
				e.Cancel = true;
				await _player.StopAsync();
			};

			while (true)
			{
				if (!Menu())
					break;
				Console.Clear();
				do
				{
					Console.SetCursorPosition(0, 0);


					if (_player.CorePlayer.CurrentFile?.Length is long l)
						Console.WriteLine($"|{Arrow(80, _player.CorePlayer.Tick / (double)l)}|");
					foreach (var t in GC.Player.Track.Tones)
						if (t != null)
							Console.WriteLine(
								$"CH{t.Channel:#0} ♪{t.NoteNum:##0} V{t.Velocity:##0} {Enum.GetName(typeof(EnvelopeFlag), t.EnvFlag).PadRight(7)} G{t.Gate:####0} ST{t.StartTick:##0.0} T{t.Tick:##0.0}");
						else
							Console.WriteLine();
				} while (_player.IsPlaying);

			}

		}

		static string[] _files;
		private static int _ptr = -1;
		static bool Menu()
		{
			_files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(s => Path.GetExtension(s) == ".mid").Select(Path.GetFileName).ToArray();

			if (!_useAutoPlay || _ptr == -1)
			{
				foreach ((string path, int index) fs in _files.Select((s, i) => (s, i)))
					Console.WriteLine($"{fs.index})\t{fs.path}");
				Console.WriteLine();
				Console.Write("再生するものを選んでください(-1で終了) >");
				while (!int.TryParse(Console.ReadLine(), out var a) || a >= _files.Length || a < 0)
				{
					Console.Write("ちゃんと選べ。 >");
				}
				_ptr = a;
			}

			_player.Play(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _files[_ptr]), _useAutoPlay ? 2 : -1, 8000);

			if (_useAutoPlay)
				_ptr++;
			return true;
		}

		/*private static async Task Playing(CancellationToken ct)
		{
			

			Console.Clear();
			while (true)
			{
				var ti = Environment.TickCount;
				await player.GetBufferAsync(buffer);
				delta = Environment.TickCount - ti;

				if (!player.IsPlaying || ct.IsCancellationRequested)
					break;

				byte[] b = ToByte(buffer);
				

				bwp.AddSamples(b, 0, b.Length);

				Console.SetCursorPosition(0, 0);
				Console.WriteLine($"{bwp?.BufferedBytes:#######0} {bwp?.BufferLength:#######0} {player?.Time:#######0} {player?.CurrentFile?.Length:#######0} {delta:###0} {Player.Track.Tones.Count(t => t != null):#0}");
				foreach (Tone t in Player.Track.Tones)
					if (t != null)
						Console.WriteLine($"CH{t.Channel:#0} ♪{t.NoteNum:##0} V{t.Velocity:##0} {Enum.GetName(typeof(EnvelopeFlag), t.EnvFlag).PadRight(7)} G{t.Gate:####0} ST{t.StartTick:##0.0} T{t.Tick:##0.0}");
					else
						Console.WriteLine();

				while (bwp.BufferedBytes > buffer.Length * 4)
					await Task.Delay(1);
			}
		}*/
		
	}
}
