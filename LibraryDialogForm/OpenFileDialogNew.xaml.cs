using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LibraryDialogForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OpenFileDialogNew : Window
    {
        public OpenFileDialogNew()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Lv.ItemsSource = DirectoriesList;
            ComboBox.ItemsSource = ComboBoxItems;
        }

        public string FileName { get; set; }

        private ObservableCollection<Directories> DirectoriesList { get; set; } =
            new ObservableCollection<Directories>();
        
        private ObservableCollection<ComboBoxItem> ComboBoxItems { get; set; } =
            new ObservableCollection<ComboBoxItem>();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GetFiles();
            ComboBoxItems.Add(new ComboBoxItem
            {
                Name = "Все файлы",
                Filter = "*"
            });
            ComboBoxItems.Add(new ComboBoxItem
            {
                Name = "DB files (*.mdb)",
                Filter = "*.mdb"
            });
        }

        private async void GetFiles(string path = null)
        {
            try
            {
                CurrentDirectory = new DirectoryInfo(path ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                if (!CurrentDirectory.Exists)
                {
                    MessageBox.Show("Данного каталога не существует");
                    return;
                }
                FileBox.Text = CurrentDirectory.FullName;
                DirectoriesList.Clear();
                DirectoriesList.Add(new Directories{Name = "..."});
                foreach (var directoryInfo in CurrentDirectory.GetDirectories())
                {
                    DirectoriesList.Add(new Directories{Name = directoryInfo.Name, FullName = directoryInfo.FullName});
                }

                var searchParam = ComboBox.SelectedIndex != -1 ? ComboBoxItems[ComboBox.SelectedIndex].Filter : "";
                foreach (var directoryInfo in CurrentDirectory.GetFiles(searchParam))
                {
                    DirectoriesList.Add(new Directories{Name = directoryInfo.Name, FullName = directoryInfo.FullName});
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Lv.Items.Refresh();
        }

        private DirectoryInfo CurrentDirectory { get; set; }

        private void Lv_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            switch (Lv.SelectedIndex)
            {
                case -1:
                    return;
                case 0:
                {
                    var dir = CurrentDirectory.FullName.Split('\\', '/').ToList();
                    dir.Remove(dir.Last());
                    var str = string.Join(@"\", dir);
                    GetFiles(str);
                    
                    return;
                }
            }

            var destDirName = (Lv.SelectedItem as Directories)?.FullName;
            if (File.Exists(destDirName))
            {
                FileName = destDirName;
                //Cmd(destDirName);
                return;
            }
            GetFiles(destDirName);
        }

        private static void Cmd(string path)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = path;
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int _currentIndex;
        
        private void Lv_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Lv.SelectedIndex != -1 & Lv.SelectedIndex == _currentIndex)
            {
                Lv.SelectedIndex = -1;
            }

            _currentIndex = Lv.SelectedIndex;
        }

        private void FileBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var selectStart = FileBox.SelectionStart;
            GetFiles(FileBox.Text);
            FileBox.SelectionStart = selectStart;
        }

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetFiles();
        }

        private void ButtonOpenClick(object sender, RoutedEventArgs e)
        {
            OpenFile();
            this.Close();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    
    public class Directories
    {
        public string Name { get; set; }
        public string FullName { get; set; }
    }

    public class ComboBoxItem
    {
        public string Name { get; set; }
        public string Filter { get; set; }
    }
}