using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GroorineCore.DataModel;
using GroorineCore.DotNet45;
using GC = GroorineCore;
using System.Threading;
using static System.Console;

namespace GroorineTest
{

	class Program
	{

		enum ViewMode
		{
			Tone,
			Channel
		}

		static Player _player;
		private static bool _useAutoPlay;
		static void Main(string[] args)
		{

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
			WriteLine(@"Groorine Test Console");

			var myViewMode = ViewMode.Tone;

			if (args.Length > 0)
			{

				_useAutoPlay = true;
				_files = args;
				_ptr = 0;
				WriteLine("Play these files: ");
				foreach (var s in _files)
					WriteLine($"- {s}");
			}
			else
			{

				Write("Do you want to use auto play?(Y/N) >");
				var r = ReadKey();
				_useAutoPlay = r.Key == ConsoleKey.Y;
			}
			string Arrow(int length, double rate)
			{
				int a = (int)(length * rate);
				int b = length - a;
				return (a > 1 ? new string('=', a - 1) : "") + (a > 0 ? ">" : "") + (b > 0 ? new string('-', b) : "");
			}

			_player = new Player(latency: 25);

			//Console.CancelKeyPress += async (s, e) =>
			//{
			//	e.Cancel = true;
			//	await _player.StopAsync();
			//};

			while (true)
			{
				if (!Menu())
					break;
				Clear();
				do
				{
					if (KeyAvailable)
					{
						switch (ReadKey().Key)
						{
							case ConsoleKey.Spacebar:
								_player.StopAsync().Wait();
								break;
							case ConsoleKey.F1:
								myViewMode = ViewMode.Tone;
								break;
							case ConsoleKey.F2:
								myViewMode = ViewMode.Channel;
								break;
						}
					}

					SetCursorPosition(0, 0);
					WriteLine($"File: {_ptr - 1}) {(_ptr > 0 ? _files[_ptr - 1] : "NULL")}");
					WriteLine($"♪ {_player.CorePlayer.CurrentFile?.Title ?? "NULL"} - {_player.CorePlayer.CurrentFile?.Copyright ?? "NULL"}");
					WriteLine(	"[SPACE]: 再生停止\n" +
								"[F1]: ToneView\n" +
								"[F2]: ChannelView");
					if (_player.CorePlayer.CurrentFile?.Length is long l)
						WriteLine($"|{Arrow(80, _player.CorePlayer.Tick / (double)l)}| {_player.CorePlayer.Time / 1000d:###0.00 秒} / {_player.CorePlayer.MaxTime / 1000d:###0.00 秒}");

					switch (myViewMode)
					{
						case ViewMode.Tone:
							WriteLine("Tone View");
							foreach (var t in GC.Player.Track.Tones.OrderBy(t => t?.Channel ?? int.MaxValue))
								if (t == null)
									WriteLine("                                                                      ");
								else
									WriteLine(
											$"CH{t.Channel,2} ♪{t.NoteNum,3} V{t.Velocity,3} {t.RealTick,5}|{Arrow(10, t.RealTick / t.Gate)}|{t.Gate,5}             ");
							break;
						case ViewMode.Channel:
							WriteLine("Channel View");
							foreach ((int pc, IChannel ch, int no) t in _player.CorePlayer.Tracks.Select((t, i) => (t.ProgramChange, t.Channel, i)))
								WriteLine($"CH{t.no,2} V|{Arrow(16, t.ch.Volume / 127d)}| E|{Arrow(16, t.ch.Expression / 127d)}| Pan|{Arrow(24, t.ch.Panpot / 127)}| P.B.|{Arrow(24, (t.ch.Pitchbend + 8192) / 16384d)}|");
							break;
					}
					

					Thread.Sleep(1);
				} while (_player.IsPlaying);

			}
			_player.Dispose();
		}

		static string[] _files;
		private static int _ptr = -1;
		static bool Menu()
		{
			if (_files == null)
				_files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(s => Path.GetExtension(s) == ".mid").Select(Path.GetFileName).ToArray();
			Clear();
			if (!_useAutoPlay || _ptr == -1)
			{
				foreach ((string path, int index) fs in _files.Select((s, i) => (s, i)))
					WriteLine($"{fs.index})\t{fs.path}");
				WriteLine();
				Write("再生するものを選んでください(-1で終了) >");
				while (!int.TryParse(ReadLine(), out var a) || a >= _files.Length || a < 0)
				{
					if (a == -1)
						return false;
					Write("ちゃんと選べ。 >");
				}
				_ptr = a;
			}

			if (_ptr >= _files.Length)
				_ptr = 0;
			if (_files.Length > 0)
				_player.Play(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _files[_ptr]), _useAutoPlay ? 2 : 2, 8000);

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
