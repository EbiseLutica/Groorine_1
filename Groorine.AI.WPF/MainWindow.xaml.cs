using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            LogWrite($"Groorine.AI {version}");
            LogWrite($"(C)2018 Xeltica");
            LogWrite($"Groorine Core (C)2017 Xeltica");
            LogWrite($"OS: {Environment.OSVersion.VersionString} {(Environment.Is64BitOperatingSystem ? "64bit" : "32bit")}");
            LogWrite($"\nReady.");
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

        private void Label_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            foreach (var d in dropFiles)
            {
                LogWrite($"{d} has been analyzed!");
            }
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            LogWrite("Synthesizing a song...");
            LogWrite("Successfully finished! Playing.");
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            LogWrite("Stopped playing!");
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            LogWrite("Clear all learned data.");
        }

        private void replay_Click(object sender, RoutedEventArgs e)
        {
            LogWrite("Play the analyzed song again");
        }

        public void LogWrite(object text) => log.Text += text + Environment.NewLine;

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
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
                    LogWrite($"{d} has been analyzed!");
                }
            }
            else
            {
                LogWrite("Canceled!");
            }
        }
    }
}
