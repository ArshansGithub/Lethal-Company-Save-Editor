using System.Diagnostics;
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
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LC_Save_Editor
{

    public partial class Editor : Window
    {
        private Dictionary<string, string> data;
        private string picked;
        private int[] shipScrap, shipGrabbableItems, enemyScan, storyLogs, shipUpgrades;
        private int stepsTaken, valueCollected, deaths, daysSpent, seed, deadline, groupCredits, planetIndex, profitQuota, quotasPassed, quotasFullfilled, globalTime, fileGameVersion;

        private string[] everyNameStats = ["StepsTaken", "ValueCollected", "DeathsTaken", "DaysSpent", "DaSeed", "DeadlineTime", "GroupCredits", "ProfitQuota", "QuotasPassed", "QuotasFulfilled", "GlobalTime", "FileGameVersion"];
        private string[] everyNameStatsSaveKey = ["Stats_StepsTaken", "Stats_ValueCollected", "Stats_Deaths", "Stats_DaysSpent", "RandomSeed", "DeadlineTime", "GroupCredits", "ProfitQuota", "QuotasPassed", "QuotaFulfilled", "GlobalTime", "FileGameVers"];
        private string[] everyNameCheckbox = ["OrangeSuit", "GreenSuit", "HazardSuit", "PajamaSuit", "CozyLights", "Teleporter", "Television", "Cupboard", "FileCabinet", "Toliet", "Shower", "LightSwitch", "RecordPlayer", "Table", "RomanticTable", "Bunkbed", "Terminal", "SignalTranslator", "LoudHorn", "InverseTeleporter", "JackoLantern", "WelcomeMat", "Goldfish", "PlushieMan",];
        private string[] nonScrap = ["Binoculars", "BoomBox", "Flashlight", "Jetpack", "Key", "Lockpick", "HandheldMonitor", "ProFlashlight", "Shovel", "Flashbang", "ExtensionLadder", "TzpInhalant", "WalkieTalkie", "StunGun", "SprayPaint", "ShotgunShell", "Shotgun"];
        private List<int> nonScrapIDs = new List<int>();
        private dynamic jsonified;
        private int lastId;

        private List<int> lastScrapValues;
        private List<int> lastScrapIds;
        private List<int> lastScrapIdsClean;
        private List<int> containsPlaceholders;

        public string password = "lcslime14a5";

        string default_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + "\\ZeekerssRBLX\\Lethal Company";

        public Editor(Dictionary<string, string> data, string pickedKey)
        {
            InitializeComponent();

            this.savename.Content += pickedKey;

            this.data = data;
            this.picked = pickedKey;

            foreach (var item in Enum.GetValues(typeof(VanillaItems)))
            {
                if (this.nonScrap.Contains(item.ToString()))
                {
                    this.nonScrapIDs.Add((int) item);
                }
            }

            this.lastScrapValues = null;
            this.lastScrapIds = null;
            this.lastScrapIdsClean = null;
            this.containsPlaceholders = new List<int> { };
            

            this.jsonified = null;

            Debug.WriteLine(this.data[this.picked]);

            Load_Data();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            // Delete item from player possession
            int index = this.SaveScrap.SelectedIndex;
            if (index != -1) {

                List<int> temp = (this.jsonified.shipGrabbableItemIDs.value).ToObject<List<int>>();
                temp.RemoveAt(index);
                this.jsonified.shipGrabbableItemIDs.value = JArray.FromObject(temp);

                // Delete item from player position
                List<dynamic> temp2 = (this.jsonified.shipGrabbableItemPos.value).ToObject<List<dynamic>>();
                temp2.RemoveAt(index);
                this.jsonified.shipGrabbableItemPos.value = JArray.FromObject(temp2);

                // Delete item from scrap values only if it's a scrap
                if (!this.nonScrap.Contains(this.SaveScrap.SelectedItem.ToString()))
                {
                    List<int> temp3 = (this.jsonified.shipScrapValues.value).ToObject<List<int>>();
                    temp3.RemoveAt(index);
                    this.jsonified.shipScrapValues.value = JArray.FromObject(temp3);
                }

                // Delete item from placeholder list
                this.containsPlaceholders.RemoveAt(index);

                // Delete item from SaveScrap
                this.SaveScrap.Items.RemoveAt(index);

                // Change dropdown index to 0
                this.SaveScrap.SelectedIndex = 0;

                System.Windows.MessageBox.Show("Done!");
            } else
            {
                System.Windows.MessageBox.Show("You must select an item to delete!");
            }
            
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
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(theRest, 0, theRest.Length);
                        }

                        // Return the decrypted file as a string (UTF8)
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }

        }

        public byte[] EncryptSave()
        {
            // Dump the jsonified object into a string
            string json = JsonConvert.SerializeObject(this.jsonified);

            // Encrypt the json converted string
                using (Aes aes = Aes.Create())
                {

                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    aes.GenerateIV();

                    using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, aes.IV, 100, HashAlgorithmName.SHA1))
                    {
                        aes.Key = pbkdf2.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                

                                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                                cs.Write(jsonBytes, 0, jsonBytes.Length);
                            }

                            byte[] IV = aes.IV;
                            byte[] encrypted = ms.ToArray();

                            byte[] final = new byte[IV.Length + encrypted.Length];
                            Array.Copy(IV, final, IV.Length);
                            Array.Copy(encrypted, 0, final, IV.Length, encrypted.Length);

                            return final;
                        }
                    }

                    // Encrypt the file
                    
                
            }

        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            int selectedValue;
            string selectedItem = this.AdditionalScraps.SelectedItem.ToString();

            if (this.nonScrap.Contains(selectedItem))
            {
                selectedValue = -1;
            }
            else
            {
                if (this.AddScrapValue.Text == "")
                {
                    System.Windows.MessageBox.Show("You must pick a value for this scrap!");
                    return;
                }

                selectedValue = int.Parse(this.AddScrapValue.Text);

                if (selectedValue < 0)
                {
                    System.Windows.MessageBox.Show("You can't have negative value scrap!");
                    return;
                }

                // Make sure value doesn't exceed safe int range
                if (selectedValue > 2147483647)
                {
                    System.Windows.MessageBox.Show("You can't have a value that exceeds the safe int range!");
                    return;
                }

            }

            // Add to scrap values
            if (!this.nonScrap.Contains(selectedItem))
            {
                List<int> temp = (this.jsonified.shipScrapValues.value).ToObject<List<int>>();
                temp.Add(selectedValue);
                this.jsonified.shipScrapValues.value = JArray.FromObject(temp);
            }

            // Add to scrap ids
            List<int> temp2 = (this.jsonified.shipGrabbableItemIDs.value).ToObject<List<int>>();
            temp2.Add((int)Enum.Parse(typeof(VanillaItems), selectedItem));
            this.jsonified.shipGrabbableItemIDs.value = JArray.FromObject(temp2);

            // Add pos
            Dictionary<string, float> temp3 = new Dictionary<string, float>();
            temp3["x"] = 6.0F;
            temp3["y"] = 0.30F;
            temp3["z"] = -14.0F;

            List<dynamic> temp4 = (this.jsonified.shipGrabbableItemPos.value).ToObject<List<dynamic>>();

            temp4.Add(temp3);

            this.jsonified.shipGrabbableItemPos.value = JArray.FromObject(temp4);
    
            // Update the placeholder list
            this.containsPlaceholders.Add(selectedValue);

            this.SaveScrap.Items.Add(selectedItem);
    
            System.Windows.MessageBox.Show("Done!");
        }

        private void AdditionalScraps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AdditionalScraps.SelectedItem != null)
            {
                if (this.nonScrap.Contains(this.AdditionalScraps.SelectedItem))
                {
                    this.AddScrapValue.Text = "Not scrap!";
                    this.AddScrapValue.IsEnabled = false;
                } else
                {
                    this.AddScrapValue.Text = "";
                    this.AddScrapValue.IsEnabled = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedValue;

            if (this.SaveScrap.SelectedItem == null) {
                System.Windows.MessageBox.Show("Seems you don't have any scrap! You can't do that!");
                return;
            }

            string selectedItem = this.SaveScrap.SelectedItem.ToString();

            if (this.nonScrap.Contains(selectedItem))
            {
                selectedValue = -1;
            } else
            {
                selectedValue = int.Parse(this.CurrentScrapValue.Text);

                if (selectedValue < 0)
                {
                    System.Windows.MessageBox.Show("You can't have negative value scrap!");
                    return;
                }

                // Make sure value doesn't exceed safe int range
                if (selectedValue > 2147483647)
                {
                    System.Windows.MessageBox.Show("You can't have a value that exceeds the safe int range!");
                    return;
                }

            }

            // Update the placeholder list
            this.containsPlaceholders[this.SaveScrap.SelectedIndex] = selectedValue;

            int index = this.lastScrapIdsClean.IndexOf((int)Enum.Parse(typeof(VanillaItems), selectedItem));

            if (index != -1)
            {
                int[] temp = (this.jsonified.shipScrapValues.value).ToObject<int[]>();
                temp[index] = selectedValue;
                this.jsonified.shipScrapValues.value = JArray.FromObject(temp);
            }
            else
            {
                System.Windows.MessageBox.Show("You can't set a value for a non-scrap item!");
                return;
            }

            System.Windows.MessageBox.Show("Done!");
        }

        private void SaveScrap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SaveScrap.SelectedItem != null)
            {
                int index = this.SaveScrap.SelectedIndex;

                if (this.containsPlaceholders[index] == -1) 
                {
                    this.CurrentScrapValue.Text = "Not scrap!";
                    this.CurrentScrapValue.IsEnabled = false;
                    return;

                } else
                {
                    this.CurrentScrapValue.Text = this.containsPlaceholders[index].ToString();
                    this.CurrentScrapValue.IsEnabled = true;
                    return;


                }
            }

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            // Check if StoryLogs exists
            if (this.jsonified.StoryLogs == null)
            {
                this.jsonified.StoryLogs = JObject.FromObject(new { __type = "System.Int32[],mscorlib", value = new int[] { } });
            }
            // Make StoryLogs an array of every value in the enum VanillaStoryLogs
            this.jsonified.StoryLogs.value = JArray.FromObject((Enum.GetValues(typeof(VanillaStoryLogs)).Cast<int>().ToArray()));

            System.Windows.MessageBox.Show("Done!");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // Check if EnemyScans exists
            if (this.jsonified.EnemyScans == null)
            {
                this.jsonified.EnemyScans = JObject.FromObject(new { __type = "System.Int32[],mscorlib", value = new int[] { } });
            }
            // Make EnemyScans an array of every value in the enum VanillaBestiary
            this.jsonified.EnemyScans.value = JArray.FromObject(Enum.GetValues(typeof(VanillaBestiary)).Cast<int>().ToArray());

            System.Windows.MessageBox.Show("Done!");

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            // Go through every checkbox, and if it's checked, check if the value is in the array. If it isn't, add it.
            foreach (var checkbox in this.everyNameCheckbox)
            {
                System.Windows.Controls.CheckBox eachCheck = this.FindName(checkbox) as System.Windows.Controls.CheckBox;
                int enumValue = (int)Enum.Parse(typeof(VanillaShipUpgrades), checkbox);
                if (eachCheck.IsChecked == true)
                {
                    if (!shipUpgrades.Contains(enumValue))
                    {
                        List<int> temp = (this.jsonified.UnlockedShipObjects.value).ToObject<List<int>>();
                        temp.Add(enumValue);
                        this.jsonified.UnlockedShipObjects.value = JArray.FromObject(temp);
                    }
                } else
                {
                    if (shipUpgrades.Contains(enumValue))
                    {
                        List<int> temp = (this.jsonified.UnlockedShipObjects.value).ToObject<List<int>>();
                        temp.Remove(enumValue);
                        this.jsonified.UnlockedShipObjects.value = JArray.FromObject(temp);
                    }
                }
            }

            // Using everyNameStats go through each and match index to the index in everyNameStatsSaveKey and set the value to the value in the textbox
            foreach (var stat in this.everyNameStats)
            {
                System.Windows.Controls.TextBox tempText = this.FindName(stat) as System.Windows.Controls.TextBox;
                int index = Array.IndexOf(this.everyNameStats, stat);
                this.jsonified[this.everyNameStatsSaveKey[index]].value = int.Parse(tempText.Text);
            }
            

            // Set the planet index to the selected value
            this.jsonified.CurrentPlanetID.value = this.PlanetDropdown.SelectedIndex;

            // Ask user if they want to pick where the save goes or if they want to overwrite the current save
            var result = System.Windows.MessageBox.Show("Do you want to overwrite the current (yes) or save to a new file (no)?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            byte[] encryptedSave = EncryptSave();

            if (result == MessageBoxResult.Yes)
            {
                // Overwrite the current save
                int which = Array.IndexOf(this.data.Keys.ToArray(), this.picked) + 1;

                File.WriteAllBytes(default_path + "\\LCSaveFile" + which.ToString(), encryptedSave);

                System.Windows.MessageBox.Show("Done! Saved to: " + default_path + "\\LCSaveFile" + which.ToString());
            } else if (result == MessageBoxResult.No)
            {
                // Save to a new file
                using var dialog = new SaveFileDialog
                {
                    Title = "Where do you want to save your modified save file?",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + "\\ZeekerssRBLX\\Lethal Company"
                };

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    System.Windows.MessageBox.Show("You must select a file to continue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                File.WriteAllBytes(dialog.FileName, encryptedSave);

                System.Windows.MessageBox.Show("Done! Saved to: " + dialog.FileName);
            } else
            {
                return;
            }
        }

        private void Load_Data()
        {
            this.jsonified = JsonConvert.DeserializeObject(this.data[this.picked]);

            this.lastScrapValues = null;
            this.lastScrapIds = null;
            this.lastScrapIdsClean = null;
            this.containsPlaceholders = new List<int> { };

            // Reset the comboboxes
            this.PlanetDropdown.Items.Clear();
            this.AdditionalScraps.Items.Clear();
            this.SaveScrap.Items.Clear();

            // Go through enum for planets and add them to the combobox
            foreach (var planet in Enum.GetValues(typeof(VanillaMoons)))
            {
                this.PlanetDropdown.Items.Add(planet.ToString());
            }

            foreach (var item in Enum.GetValues(typeof(VanillaItems)))
            {
                this.AdditionalScraps.Items.Add(item.ToString());
            }

            this.AdditionalScraps.SelectedIndex = 0;

            try
            {
                shipScrap = (this.jsonified.shipScrapValues.value).ToObject<int[]>();
            } catch
            {
                // If the shipScrapValues doesn't exist, create it as a JArray object
                Dictionary<string, dynamic> temp = new Dictionary<string, dynamic>();
                temp["__type"] = "System.Int32[],mscorlib";
                temp["value"] = new int[] { };
                this.jsonified.shipScrapValues = JObject.FromObject(temp);
                shipScrap = (this.jsonified.shipScrapValues.value).ToObject < int[]>();

            }

            this.lastScrapValues = shipScrap.ToList();

            foreach (var scrap in shipScrap)
            {
                this.AdditionalScraps.Items.Add(((VanillaItems) scrap).ToString());
            }

            try
            {
                shipGrabbableItems = (this.jsonified.shipGrabbableItemIDs.value).ToObject<int[]>();
            } catch
            {
                Dictionary<string, dynamic> temp = new Dictionary<string, dynamic>();
                temp["__type"] = "System.Int32[],mscorlib";
                temp["value"] = new int[] { };
                this.jsonified.shipGrabbableItemIDs = JObject.FromObject(temp);
                shipGrabbableItems = (this.jsonified.shipGrabbableItemIDs.value).ToObject < int[]>();
            }

            this.lastScrapIds = shipGrabbableItems.ToList();

            List<int> tempConverted = new List<int>();

            int i = 0;
            int i2 = 0;
            foreach (var itemID in (this.jsonified.shipGrabbableItemIDs.value).ToObject<int[]>())
            {
                if (!this.nonScrapIDs.Contains(itemID))
                {
                    tempConverted.Add(itemID);
                    this.containsPlaceholders.Insert(i, (this.jsonified.shipScrapValues.value).ToObject<int[]>()[i2]);
                    i2++;

                } else
                {
                    this.containsPlaceholders.Insert(i, -1);
                }
                i++;
            }

            this.lastScrapIdsClean = tempConverted;


            //enemyScan = (int[]) this.jsonified.EnemyScans.value;

            //storyLogs = (int[]) this.jsonified.StoryLogs.value;

            shipUpgrades = (this.jsonified.UnlockedShipObjects.value).ToObject<int[]>();

            stepsTaken = (int) this.jsonified.Stats_StepsTaken.value;
            valueCollected = (int) this.jsonified.Stats_ValueCollected.value;
            deaths = (int) this.jsonified.Stats_Deaths.value;
            daysSpent = (int) this.jsonified.Stats_DaysSpent.value;
            seed = (int) this.jsonified.RandomSeed.value;
            deadline = (int) this.jsonified.DeadlineTime.value;
            groupCredits = (int) this.jsonified.GroupCredits.value;  
            profitQuota = (int) this.jsonified.ProfitQuota.value;
            quotasPassed = (int) this.jsonified.QuotasPassed.value;
            quotasFullfilled = (int) this.jsonified.QuotaFulfilled.value;
            try
            {
                globalTime = (int)this.jsonified.GlobalTime.value;
            } catch
            {
                // Create GlobalTime if it doesn't exist
                this.jsonified.GlobalTime = JObject.FromObject(new { __type = "System.Int32,mscorlib", value = 0 });
            }
            try
            {
                planetIndex = (int)this.jsonified.CurrentPlanetID.value;
            } catch
            {
                this.jsonified.CurrentPlanetID = JObject.FromObject(new { __type = "System.Int32,mscorlib", value = 0 });
            }

            fileGameVersion = (int) this.jsonified.FileGameVers.value;
            

            /*foreach (var stat in this.everyNameStats)
            {
                System.Windows.Controls.CheckBox tempText = this.FindName(stat) as System.Windows.Controls.CheckBox;
                tempText.Text = this.jsonified[stat].value.ToString();
            }*/

            this.PlanetDropdown.SelectedValue = ((VanillaMoons)planetIndex).ToString();

            this.StepsTaken.Text = stepsTaken.ToString();
            this.ValueCollected.Text = valueCollected.ToString();
            this.DeathsTaken.Text = deaths.ToString();
            this.DaysSpent.Text = daysSpent.ToString();
            this.DaSeed.Text = seed.ToString();
            this.DeadlineTime.Text = deadline.ToString();
            this.GroupCredits.Text = groupCredits.ToString();

            this.ProfitQuota.Text = profitQuota.ToString();
            this.QuotasPassed.Text = quotasPassed.ToString();
            this.QuotasFulfilled.Text = quotasFullfilled.ToString();
            this.GlobalTime.Text = globalTime.ToString();
            this.FileGameVersion.Text = fileGameVersion.ToString();

            foreach (var checkbox in this.everyNameCheckbox)
            {
                System.Windows.Controls.CheckBox eachCheck = this.FindName(checkbox) as System.Windows.Controls.CheckBox;
                int enumValue = (int)Enum.Parse(typeof(VanillaShipUpgrades), checkbox);
                if (shipUpgrades.Contains(enumValue))
                {
                    eachCheck.IsChecked = true;
                }
            }

            foreach (var currentScrap in shipGrabbableItems)
            {
                this.SaveScrap.Items.Add(((VanillaItems) currentScrap).ToString());
            }

            this.SaveScrap.SelectedIndex = 0;


        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new SavePicker(data).Show();
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Are you sure you want to reset your save? This will delete all progress.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            Load_Data();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }
    }
}
