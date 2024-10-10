using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
using System.Windows.Shapes;

namespace NewProcessMonitoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _watchProcessTimeoutMs = 5000;

        private ObservableCollection<ProcessTableItem> _tableItems = new ObservableCollection<ProcessTableItem>();
        private ProcessFileWorker _processFileWorker = new ProcessFileWorker(System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewPm", "processes.json"));

        private SemaphoreSlim _tableItemsRefreshSemaphore = new SemaphoreSlim(1);
        private MinimizeToTray _minimizeToTray;

        ManagementObjectSearcher processSearcher = new ManagementObjectSearcher("SELECT ProcessId, Caption, ExecutablePath, CommandLine FROM Win32_Process");

        public MainWindow()
        {
            InitializeComponent();
            _minimizeToTray = new MinimizeToTray(this);

            var tbnOpenGoogle = new FrameworkElementFactory(typeof(Image));
            var tbnOpenFolder = new FrameworkElementFactory(typeof(Image));

            BitmapImage tbnOpenGoogleImage = new BitmapImage(new Uri("pack://application:,,,/Assets/WebBrowser.png"));
            tbnOpenGoogle.SetValue(Image.SourceProperty, tbnOpenGoogleImage);

            BitmapImage tbnOpenFolderImage = new BitmapImage(new Uri("pack://application:,,,/Assets/FolderOpened.png"));
            tbnOpenFolder.SetValue(Image.SourceProperty, tbnOpenFolderImage);

            tbnOpenGoogle.AddHandler(Image.MouseUpEvent, new MouseButtonEventHandler((s, e) =>
            {
                if (tProcesses.SelectedItem != null)
                {
                    var path = ((ProcessTableItem)tProcesses.SelectedItem).FullPath;
                    var fileName = System.IO.Path.GetFileName(path);
                    System.Diagnostics.Process.Start("https://www.google.com/search?q=What+is+" + fileName);
                }
            }));
            tbnOpenFolder.AddHandler(Image.MouseUpEvent, new MouseButtonEventHandler((s, e) =>
            {
                if (tProcesses.SelectedItem != null)
                {
                    var filePath = ((ProcessTableItem)tProcesses.SelectedItem).FullPath;
                    Process.Start("explorer.exe", "/select, \"" + filePath + "\"");
                }
            }));

            this.tProcesses.Columns.Insert(0, new DataGridTemplateColumn()
            {
                Header = "",
                CellTemplate = new DataTemplate() { VisualTree = tbnOpenGoogle }
            }
            );
            this.tProcesses.Columns.Insert(0, new DataGridTemplateColumn()
            {
                Header = "",
                CellTemplate = new DataTemplate() { VisualTree = tbnOpenFolder }
            }
            );

            tProcesses.ItemsSource = _tableItems;

            

            _ = ProcessWatcherLoopAsync();
            Application.Current.Exit += App_Exit;
        }

        public async Task ProcessWatcherLoopAsync()
        {

            var newTableItems = new List<ProcessTableItem>();
            while (true)
            {
                await Task.Delay(_watchProcessTimeoutMs);
                if (!this.IsActive) //watch only if background
                    await RefreshProcessTableAsync();
            }
        }

        private async Task RefreshProcessTableAsync()
        {
            await _tableItemsRefreshSemaphore.WaitAsync();

            Debug.WriteLine("Refresh table");

            var newTableItems = new Dictionary<string, ProcessTableItem>();

            using (var results = processSearcher.Get())
            {
                foreach (var resultItem in results)
                {
                    var mo = (ManagementObject)resultItem;
                    var pTitle = (string)mo["Caption"];
                    var pPath = (string)mo["ExecutablePath"];
                    var pCommandLine = (string)mo["CommandLine"];
                    if (!string.IsNullOrEmpty(pPath))
                    {
                        if (!_processFileWorker.ItemsByFullPath.ContainsKey(pPath))
                            newTableItems[pPath] = new ProcessTableItem(pTitle, pPath, DateTime.Now);
                    }
                }
            }


            //Fill table with untrusted items
            _tableItems.Clear();
            foreach(var newItem in newTableItems.Values)
                _tableItems.Add(newItem);
            foreach (var untrustedItem in _processFileWorker.ItemsByFullPath.Values.Where(x => !x.IsTrusted))
                _tableItems.Add(new ProcessTableItem(untrustedItem.Title, untrustedItem.FullPath, untrustedItem.FirstFoundUtcDate));

            //Add new untrusted items to file
            _processFileWorker.TryAddNewProcess(newTableItems.Values.Select(x => new ProcessFileItem(x.Name, x.FullPath, x.FirstFoundLocalDate.ToUniversalTime(), false)));

            //Order by alph:
            var ordered = _tableItems.OrderBy(x => x.Name).ToList();

            _tableItems.Clear();
            foreach (var item in ordered)
                _tableItems.Add(item);

            _tableItemsRefreshSemaphore.Release();
        }


        private void bnApplyAllToTrusted_Click(object sender, RoutedEventArgs e)
        {
            _tableItemsRefreshSemaphore.Wait();
            _processFileWorker.TryAddNewProcess(_tableItems.Select(x => new ProcessFileItem(x.Name, x.FullPath, x.FirstFoundLocalDate.ToUniversalTime(), true)));
            _tableItemsRefreshSemaphore.Release();

            _ = RefreshProcessTableAsync();
        }

        //private void bnClose_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}

        private void bnClearTrustList_Click(object sender, RoutedEventArgs e)
        {
            _processFileWorker.ClearAllAddedProcesses();
            _ = RefreshProcessTableAsync();
        }

        private void wMain_Activated(object sender, EventArgs e)
        {
            _ = RefreshProcessTableAsync(); //Force refresh
        }

        private void tProcesses_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            e.Cancel = true;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            WindowExtensions.HideMinimizeAndMaximizeButtons(this);
            this.WindowState = WindowState.Minimized;
        }
        private void App_Exit(object sender, ExitEventArgs e)
        {
            _minimizeToTray.Dispose();
        }
    }
}
