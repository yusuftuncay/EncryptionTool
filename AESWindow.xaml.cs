using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;

using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Encryptie_Tools
{
    public partial class AESWindow : Window
    {
        #region Variables
        private readonly MainWindow MainWindowField;

        private byte[] SelectedEncryptionKey;
        private byte[] SelectedEncryptionIV;

        private byte[] SelectedDecryptionKey;
        private byte[] SelectedDecryptionIV;

        private string SelectedImageToEncrypt;
        private string SelectedImageToDecrypt;

        private string FilePath_EncryptedImages;
        private string FilePath_DecryptedImages;
        private string FilePath_PlainImages;
        private readonly string FilePath_Keys;
        #endregion

        public AESWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindowField = mainWindow;

            // Default Storage Locations
            FilePath_Keys = Path.GetFullPath(mainWindow.FilePath_Keys);
            FilePath_EncryptedImages = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Encrypted Images";
            FilePath_DecryptedImages = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Decrypted Images";
            FilePath_PlainImages = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Plain Images";
        }

        #region Kies Standaard Locatie
        private void MenuPlainImagesStorage_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath_PlainImages = dialog.FileName;
                }
            }
        }
        private void MenuEncryptedImagesStorage_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath_EncryptedImages = dialog.FileName;
                }
            }
        }
        private void MenuDecryptedImagesStorage_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath_DecryptedImages = dialog.FileName;
                }
            }
        }
        #endregion

        #region Buttons Encrypt Tab
        private void BtnKeyInlezenEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any AES Key
            string[] files = Directory.GetFiles(FilePath_Keys, "AES_*.txt");

            if (files.Length > 0)
            {
                string filePath = "";

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = "txt files (*.txt)|*.txt",
                    InitialDirectory = Path.GetFullPath(FilePath_Keys)
                };

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = ofd.FileName;
                }

                if (filePath != "")
                {
                    // Open AES Key file
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            // Store the Key and IV read from the file in a byte array for later use
                            SelectedEncryptionKey = Convert.FromBase64String(sr.ReadLine());
                            SelectedEncryptionIV = Convert.FromBase64String(sr.ReadLine());
                        }
                    }

                    LblKeyNaamEncrypt.Content = Path.GetFileName(ofd.FileName);
                }
            }
            else
            {
                // There are no files with "AES" in the file name in the folder
                MessageBox.Show("No AES Keys found\n\nGo to the MainWindow to create an AES Key");
            }
        }
        private void BtnKiesImageEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any .png file
            if (Directory.GetFiles(FilePath_PlainImages, "*.png").Length > 0)
            {
                // OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "PNG files (*.png)|*.png",
                    InitialDirectory = FilePath_PlainImages
                };

                // Process Result
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedImageToEncrypt = openFileDialog.FileName;

                    ImgToEncrypt.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                }
            }
            else
            {
                // There are no files with ".png" in the file extention in the folder
                MessageBox.Show("No .png files found\n\nDownload .png files, or make sure there are .png files in your chosen folder");
            }
            
        }
        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check if User selected Image AND Key
            if (SelectedImageToEncrypt == null)
            {
                MessageBox.Show("Choose a .png file");
                return;
            }
            else if (LblKeyNaamEncrypt.Content.ToString() == "Key Name")
            {
                MessageBox.Show("Choose an AES key");
                return;
            }

            // Check if user chose a FileName
            else if (TxtFileNameEncrypt.Text == null || TxtFileNameEncrypt.Text == "")
            {
                MessageBox.Show("Enter a file name");
                return;
            }

            try
            {
                // Read the image bytes
                byte[] imgToEncryptInBytes = File.ReadAllBytes(SelectedImageToEncrypt);

                // Encrypt the image bytes
                byte[] encryptedBytes;

                // Create an Aes object with the specified key and IV
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.BlockSize = 128;

                    aes.Key = SelectedEncryptionKey;
                    aes.IV = SelectedEncryptionIV;

                    // Create an encryptor to perform the stream transform
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    // Create the streams used for encryption
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(imgToEncryptInBytes, 0, imgToEncryptInBytes.Length);
                        }
                        encryptedBytes = ms.ToArray();
                    }

                    // Write the Encrypted Image bytes as .png to User chosen FilePath
                    File.WriteAllBytes($"{FilePath_EncryptedImages}\\{TxtFileNameEncrypt.Text}.encrypted", encryptedBytes);

                    MessageBox.Show("Image encrypted successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error encrypting");
            }
        }
        #endregion

        #region Buttons Decrypt Tab
        private void BtnKeyInlezenDecrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any AES Key
            if (Directory.GetFiles(FilePath_Keys, "AES_*.txt").Length > 0)
            {
                string filePath = "";

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = "Text files (*.txt)|*.txt",
                    InitialDirectory = FilePath_Keys
                };

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = ofd.FileName;
                }

                if (filePath != "")
                {
                    // Open AES Key file
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            // Store the Key and IV read from the file in a byte array for later use
                            SelectedDecryptionKey = Convert.FromBase64String(sr.ReadLine());
                            SelectedDecryptionIV = Convert.FromBase64String(sr.ReadLine());
                        }
                    }

                    LblKeyNaamDecrypt.Content = Path.GetFileName(ofd.FileName);
                }
            }
            else
            {
                // There are no files with "AES" in the file name in the folder
                MessageBox.Show("No AES Keys found\n\nGo to the MainWindow to create an AES Key");
            }
        }
        private void BtnKiesImageDecrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any .encrypted file
            if (Directory.GetFiles(FilePath_EncryptedImages, "*.encrypted").Length > 0)
            {
                // OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Encrypted files (*.encrypted)|*.encrypted",
                    InitialDirectory = FilePath_EncryptedImages
                };

                // Process Result
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LblEncryptedFile.Content = openFileDialog.SafeFileName;
                    SelectedImageToDecrypt = openFileDialog.FileName;
                }
            }
            else
            {
                // There are no files with ".encrypted" in as extention in the folder
                MessageBox.Show("No .encrypted file found\n\nGo to the Encrypt tab to encrypt a .png file into an .encrypted file");
            }

        }
        private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check if User selected Encrypted Image AND Key
            if (SelectedImageToDecrypt == null)
            {
                MessageBox.Show("Choose a .encrypted file");
                return;
            }
            else if (LblKeyNaamDecrypt.Content.ToString() == "Key Name")
            {
                MessageBox.Show("Choose an AES key");
                return;
            }

            // Check if user chose a FileName
            else if (TxtFileNameDecrypt.Text == null || TxtFileNameDecrypt.Text == "")
            {
                MessageBox.Show("Enter a file name");
                return;
            }

            try
            {
                // Reset Image
                ImgDecrypt.Source = null;

                // Read the image bytes
                byte[] imgToDecryptInBytes = File.ReadAllBytes(SelectedImageToDecrypt);

                // Encrypt the image bytes
                byte[] decryptedBytes;

                // Create an Aes object with the specified key and IV
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.BlockSize = 128;

                    aes.Key = SelectedDecryptionKey;
                    aes.IV = SelectedDecryptionIV;

                    // Create a decryptor to perform the stream transform
                    ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    // Create the streams used for decryption
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(imgToDecryptInBytes, 0, imgToDecryptInBytes.Length);
                        }
                        decryptedBytes = ms.ToArray();
                    }

                    // Write the Decrypted Image bytes as .png to User chosen FilePath
                    File.WriteAllBytes($"{FilePath_DecryptedImages}\\{TxtFileNameDecrypt.Text}.png", decryptedBytes);
                }

                // Show Decrypted Image on Application
                ImgDecrypt.Source = new BitmapImage(new Uri($"{FilePath_DecryptedImages}\\{TxtFileNameDecrypt.Text}.png"));

                MessageBox.Show("Image decrypted successfully");
            }
            catch (CryptographicException) // Specific exception when Wrong Key is selected
            {
                MessageBox.Show("Wrong Key");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error decrypting");
            }
        }
        #endregion

        #region TabControl
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reset all fields if user changes tabs, this to prevent errors like "Image is used by another process"
            SelectedEncryptionKey = null;
            SelectedEncryptionIV = null;

            SelectedDecryptionKey = null;
            SelectedDecryptionIV = null;

            SelectedImageToEncrypt = null;
            SelectedImageToDecrypt = null;

            ImgToEncrypt.Source = null;
            ImgDecrypt.Source = null;

            LblKeyNaamEncrypt.Content = "Key Name";
            LblKeyNaamDecrypt.Content = "Key Name";

            LblEncryptedFile.Content = ".encrypted file";

            TxtFileNameEncrypt.Text = null;
            TxtFileNameDecrypt.Text = null;
        }
        #endregion
    }
}
