using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ZipImgConv
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string setting_file;
        private Settings settings;
        private CancellationTokenSource cancellationTokenSource = null;
        private bool IsConvertPorcessing = false;

        public MainWindow()
        {
            InitializeComponent();

            setting_file = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "settings.json"
            );
            try
            {
                this.settings = Settings.LoadFromJsonFile(setting_file);
            }
            catch
            {
                this.settings = new Settings();
            }

        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null)
            {
                return;
            }

            var convert_list = this.DataContext as ConvertTargetList;

            foreach (var file in this.expandDirectory(files))
            {
                convert_list.Add(new ConvertTarget() { FileName = file });
            }
        }

        private IEnumerable<string> expandDirectory(string[] files)
        {
            foreach (var file in files)
            {
                if (Directory.Exists(file))
                {
                    var dinfo = new DirectoryInfo(file);
                    foreach (var finfo in dinfo.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        yield return finfo.FullName;
                    }
                }
                else if (File.Exists(file))
                {
                    yield return file;
                }
            }
        }

        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            var s = new Settings(this.settings);

            var w = new SettingWindow();
            w.DataContext = s;
            var model_result = w.ShowDialog();
            if (model_result.HasValue && model_result.Value)
            {
                this.settings = s;
                this.settings.WriteToJsonFile(setting_file);
            }
        }

        private void Convert_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsConvertPorcessing;
        }

        private async void Convert_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var convert_list = this.DataContext as ConvertTargetList;
            var w = new Worker(convert_list, this.settings);
            cancellationTokenSource = new CancellationTokenSource();
            IsConvertPorcessing = true;
            await w.Convert(cancellationTokenSource.Token);
            IsConvertPorcessing = false;
            cancellationTokenSource = null;
        }

        private void ConvertCancel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsConvertPorcessing;
        }

        private void ConvertCancel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        private void SelectionDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = 
                TargetListView != null && TargetListView.SelectedItem != null
                && !IsConvertPorcessing;
        }

        private void SelectionDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var convert_list = this.DataContext as ConvertTargetList;
            
            var delete_list = Array.CreateInstance(typeof(ConvertTarget), TargetListView.SelectedItems.Count);
            TargetListView.SelectedItems.CopyTo(delete_list, 0);

            TargetListView.SelectedIndex = TargetListView.SelectedIndex + 1;

            foreach (var i in delete_list)
            {
                convert_list.Remove(i as ConvertTarget);
            }
        }
    }
}
