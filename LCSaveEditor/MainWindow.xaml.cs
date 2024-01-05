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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string password = "lcslime14a5";
        string default_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + "\\ZeekerssRBLX\\Lethal Company";

        public MainWindow()
        {
            InitializeComponent();
        }

        public Object DecryptSave(string path)
        {
            // Open the file
            FileStream file = File.Open(path, FileMode.Open);

            // Get bytes from file
            byte[] fileBytes = new byte[file.Length];
            file.Read(fileBytes, 0, fileBytes.Length);

            // Using PBKDF2 decrypt file
            byte[] IV = new byte[16];
            byte[] theRest = new byte[fileBytes.Length - 16];
            Array.Copy(fileBytes, IV, 16);
            Array.Copy(fileBytes, 16, theRest, 0, theRest.Length);

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, IV, 100, HashAlgorithmName.SHA1))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = pbkdf2.GetBytes(16);
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Decrypt the file
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(theRest, 0, theRest.Length);
                        }

                        // Return the decrypted file as a string (UTF8)
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }   

        } 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Save1, Save2, Save3, Save1Decrypted, Save2Decrypted, Save3Decrypted;
            if (this.automatic_radio.IsChecked is true)
            {
                Save1 = default_path + "\\LCSaveFile1";
                Save2 = default_path + "\\LCSaveFile2";
                Save3 = default_path + "\\LCSaveFile3";

            } else if (this.manual_radio.IsChecked is true)
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Where do you have your Lethal Company saves?",
                    UseDescriptionForTitle = true,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"),
                    ShowNewFolderButton = true
                };

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    System.Windows.MessageBox.Show("You must select a folder to continue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.automatic_radio.IsChecked = false;
                    this.manual_radio.IsChecked = false;
                    return;
                }

                Save1 = dialog.SelectedPath + "\\LCSaveFile1";
                Save2 = dialog.SelectedPath + "\\LCSaveFile2";
                Save3 = dialog.SelectedPath + "\\LCSaveFile3";

                // Check if the files exist
                if (!File.Exists(Save1) && !File.Exists(Save2) && !File.Exists(Save3))
                {
                    System.Windows.MessageBox.Show("Could not find any save files. Please make sure you have the correct folder selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.automatic_radio.IsChecked = false;
                    this.manual_radio.IsChecked = false;
                    return;
                }

                
            }
            else
            {
                System.Windows.MessageBox.Show("You must select a folder to continue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.automatic_radio.IsChecked = false;
                this.manual_radio.IsChecked = false;
                return;
            }
            try
            {
                if (!File.Exists(Save1))
                {
                    Save1 = null;
                }
                if (!File.Exists(Save2))
                {
                    Save2 = null;
                }
                if (!File.Exists(Save3))
                {
                    Save3 = null;
                }

                if (Save1 is null)
                {
                    Save1Decrypted = null;
                } else
                {
                    Save1Decrypted = DecryptSave(Save1).ToString();

                }
                if (Save2 is null)
                {
                    Save2Decrypted = null;
                }
                else
                {
                    Save2Decrypted = DecryptSave(Save2).ToString();
                }
                if (Save3 is null)
                {
                    Save3Decrypted = null;
                }
                else
                {
                    Save3Decrypted = DecryptSave(Save3).ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                System.Windows.MessageBox.Show("An error occured while decrypting the save files.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.automatic_radio.IsChecked = false;
                this.manual_radio.IsChecked = false;
                return;
            }

            Dictionary<string, string> saves = new Dictionary<string, string>
            {
                { "Save 1", Save1Decrypted },
                { "Save 2", Save2Decrypted },
                { "Save 3", Save3Decrypted }
            };

            SavePicker savePicker = new SavePicker(saves);
            savePicker.Show();
            this.Close();

        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            // Automatic was clicked
            this.manual_radio.IsChecked = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Manual was clicked
            this.automatic_radio.IsChecked = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}