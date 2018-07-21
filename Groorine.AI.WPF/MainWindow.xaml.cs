using Groorine.DataModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Groorine.AI.WPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly String version = "0.0.0";

		Student student = new Student();

		MidiFile currentFile;
		DotNet45.Player player = new DotNet45.Player();

        public MainWindow()
        {
            InitializeComponent();
            LogWrite($"Groorine.AI {version}");
            LogWrite($"(C)2018 Xeltica");
            LogWrite($"Groorine Core (C)2017 Xeltica");
            LogWrite($"OS: {Environment.OSVersion.VersionString} {(Environment.Is64BitOperatingSystem ? "64bit" : "32bit")}");
			LogWrite($"Date: {DateTime.Now.ToShortDateString()}");
			LogWrite($"\nReady.");
        }

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			player.Dispose();
		}

		private void Label_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void Label_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            foreach (var d in dropFiles)
			{
				await AnalyzeAsync(d);
			}
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            LogWrite("Synthesizing a song...");
			currentFile = student.Generate(64);
			if (currentFile == null)
			{
				LogWrite("Couldn't start playing because I learn nothing yet.");
				return;
			}
			player.CorePlayer.Load(currentFile);
			player.PlayAsync();

			LogWrite("Successfully finished! Playing.");
		}

        private async void stop_Click(object sender, RoutedEventArgs e)
        {
			await player.StopAsync();
            LogWrite("Stopped playing!");
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
			student.Clear();
            LogWrite("Clear all learned data.");
        }

        private void replay_Click(object sender, RoutedEventArgs e)
		{
			if (currentFile == null)
			{
				LogWrite("Couldn't replay because there is no generated song yet.");
				return;
			}
			player.CorePlayer.Load(currentFile);
			player.PlayAsync();
			LogWrite("Playing the analyzed song again.");
        }

		public void LogWrite(object text)
		{
			log.Text += $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}]" + text + Environment.NewLine;
			log.ScrollToEnd();
		}

        private async void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LogWrite("Selecting files...");

            var dialog = new OpenFileDialog
            {
                Filter = "Standard MIDI File (*.mid, *.midi)|*.mid;*.midi|All Files (*.*)|*.*",
                Multiselect = true,
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var d in dialog.FileNames)
                {
					await AnalyzeAsync(d);
                }
            }
            else
            {
                LogWrite("Canceled!");
            }
        }

		public async Task AnalyzeAsync(string path)
		{
			try
			{
				using (var fs = new FileStream(path, FileMode.Open))
				{
					var m = SmfParser.Parse(fs);

					await Task.Factory.StartNew(() => student.Learn(m));
					LogWrite($"{path} has been analyzed!");
				}
			}
			catch (ArgumentException)
			{
				LogWrite("Failed to analyze the file because it wasn't a midi file!");
			}
			catch (FileNotFoundException)
			{
				LogWrite("Failed to analyze the file because it wasn't found!");
			}
			catch (Exception ex)
			{
				LogWrite($"Unknown error! {ex.GetType().Name} : {ex.Message}\n{ex.StackTrace}");
			}
		}

		private async void Export_Click(object sender, RoutedEventArgs e)
		{
			if (currentFile == null)
			{
				LogWrite("Couldn't export because there is no generated song yet.");
				return;
			}
			LogWrite("Select a destination to export.");

			var sfd = new SaveFileDialog();
			sfd.Filter = "Audio File (*.wav)|*.wav";
			if (sfd.ShowDialog() != true)
			{
				LogWrite("Canceled.");
				return;
			}

			LogWrite("Start exporting! Please wait...");
			
			IsEnabled = false;
			try
			{
				await player.SaveAsync(sfd.FileName);
				LogWrite("Successfully exported!");
			}
			catch (Exception ex)
			{
				LogWrite("Couldn't export it due to a unknown error! " + ex.Message);
			}
			IsEnabled = true;
		}
	}
}
