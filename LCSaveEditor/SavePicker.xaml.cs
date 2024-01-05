using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;

namespace LC_Save_Editor
{
    public partial class SavePicker : Window
    {
        private Dictionary<string, string> allSaves;

        public SavePicker(Dictionary<string, string> data)
        {
            InitializeComponent();

            this.allSaves = data;
            int bad = 0;

            foreach (var eachSave in data)
            {
                if (eachSave.Value != null)
                {
                    this.listofsaves.Items.Add(eachSave.Key);
                } else
                {
                    bad += 1;
                }

            }

            if (bad == data.Count)
            {
                System.Windows.MessageBox.Show("No saves found! Please make sure you have a save file in the default location.");
                new MainWindow().Show();
                this.Close();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected save
            string selectedSave = this.listofsaves.SelectedItem.ToString();

            new Editor(allSaves, selectedSave).Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}