using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Encryptie_Tools
{
    public partial class RSAWindow : Window
    {
        #region Variables
        private MainWindow mainWindow;

        private string SelectedPublicRSAEncryptionKey;
        private string SelectedPrivateRSADecryptionKey;

        private string SelectedAESKeyToEncrypt;
        private byte[] SelectedAESKeyToDecrypt;

        private string FilePath_AESKey;
        private string FilePath_Keys;
        #endregion

        public RSAWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            // Default Storage Locations : Documents Folder
            FilePath_Keys = Path.GetFullPath(mainWindow.FilePath_Keys);
            FilePath_AESKey = FilePath_Keys;
        }

        #region Kies Standaard Locatie
        private void MenuRSAKeyLocationStorage_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath_AESKey = dialog.SelectedPath;
                }
            }
        }
        #endregion

        #region Buttons Encrypt Tab
        private void BtnAESKeyInlezenEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any Public AES Key
            string[] files = Directory.GetFiles(FilePath_Keys, "AES*.txt");

            if (files.Length > 0)
            {
                string filePath = "";

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = "Text files (*.txt)|AES*.txt",
                    InitialDirectory = FilePath_AESKey
                };

                if (ofd.ShowDialog() == true)
                {
                    filePath = ofd.FileName;
                }

                if (filePath != "")
                {
                    // Open AES Key file
                    SelectedAESKeyToEncrypt = File.ReadAllText(filePath);

                    // Show FileName
                    LblAESKeyNaamEncrypt.Content = Path.GetFileName(ofd.FileName);
                }
            }
            else
            {
                // There are no files with "AES" in the file name in the folder
                MessageBox.Show("Geen AES Keys gevonden\n\nGa naar het MainWindow om een AES Key te maken");
            }
        }
        private void BtnPublicRSAKeyInlezenEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any Public RSA Key
            string[] files = Directory.GetFiles(FilePath_Keys, "RSA_Public_*.xml");

            if (files.Length > 0)
            {
                string filePath = "";

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = "Xml files (*.xml)|RSA_Public_*.xml",
                    InitialDirectory = FilePath_Keys
                };

                if (ofd.ShowDialog() == true)
                {
                    filePath = ofd.FileName;
                }

                if (filePath != "")
                {
                    // Save RSA Key file
                    SelectedPublicRSAEncryptionKey = File.ReadAllText(filePath);

                    // Show FileName
                    LblPublicRSAKeyNaamEncrypt.Content = Path.GetFileName(ofd.FileName);
                }
            }
            else
            {
                // There are no files with "RSA_Public" in the file name in the folder
                MessageBox.Show("Geen Publieke RSA Keys gevonden\n\nGa naar het MainWindow om een RSA Key te maken");
            }
        }
        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check if User selected AES AND RSA Key
            if (SelectedAESKeyToEncrypt == null)
            {
                MessageBox.Show("Kies een AES Key");
                return;
            }
            else if (SelectedPublicRSAEncryptionKey == null)
            {
                MessageBox.Show("Kies een Public RSA Key");
                return;
            }

            // Check if user chose a FileName
            else if (TxtFileNameEncrypt.Text == null || TxtFileNameEncrypt.Text == "")
            {
                MessageBox.Show("Geef een bestandsnaam");
                return;
            }

            try
            {
                // Read the AES Key to bytes
                byte[] aesKeyToEncrypt = Encoding.UTF8.GetBytes(SelectedAESKeyToEncrypt);

                // Create RSA and Encrypt the AES Key using RSA
                byte[] encryptedBytes;
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(SelectedPublicRSAEncryptionKey);
                    encryptedBytes = rsa.Encrypt(aesKeyToEncrypt, RSAEncryptionPadding.Pkcs1);
                }

                // Write the Encrypted AES Key to User chosen FilePath
                File.WriteAllBytes($"{FilePath_AESKey}\\{TxtFileNameEncrypt.Text}_Encrypted_Key.encrypted", encryptedBytes);

                MessageBox.Show("Key succesvol geëencrypteerd");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error encrypting");
            }
        }
        #endregion

        #region Buttons Decrypt Tab
        private void BtnAESKeyInlezenDecrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any AES Key
            string[] files = Directory.GetFiles(FilePath_AESKey, "*.encrypted");

            if (files.Length > 0)
            {
                string filePath = "";

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = "Encrypted files (*.encrypted)|*.encrypted",
                    InitialDirectory = FilePath_AESKey
                };

                if (ofd.ShowDialog() == true)
                {
                    filePath = ofd.FileName;
                }

                if (filePath != "")
                {
                    // Open AES Key file
                    SelectedAESKeyToDecrypt = File.ReadAllBytes(filePath);

                    // Show FileName
                    LblAESKeyNaamDecrypt.Content = Path.GetFileName(ofd.FileName);
                }
            }
            else
            {
                // There are no files with ".encrypted" in as extention in the folder
                MessageBox.Show("Geen .encrypted file gevonden\n\nGa naar het Encrypt tab om een AES Key te encrypteren naar een .encrypted file");
            }
        }
        private void BtnPrivateRSAKeyInlezenDecrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check existence of any Private RSA Key
            string[] files = Directory.GetFiles(FilePath_Keys, "RSA_Private_*.xml");

            if (files.Length > 0)
            {
                string filePath = "";

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = "Xml files (*.xml)|RSA_Private_*.xml",
                    InitialDirectory = FilePath_Keys
                };

                if (ofd.ShowDialog() == true)
                {
                    filePath = ofd.FileName;
                }

                if (filePath != "")
                {
                    // Save RSA Key file
                    SelectedPrivateRSADecryptionKey = File.ReadAllText(filePath);

                    // Show FileName
                    LblPrivateRSAKeyNaamDecrypt.Content = Path.GetFileName(ofd.FileName);
                }
            }
            else
            {
                // There are no files with "RSA_Private" in the file name in the folder
                MessageBox.Show("Geen Private RSA Keys gevonden\n\nGa naar het MainWindow om een RSA Key te maken");
            }

        }
        private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            // Check if User selected AES AND RSA Key
            if (SelectedAESKeyToDecrypt == null)
            {
                MessageBox.Show("Kies een Encrypted AES Key");
                return;
            }
            else if (SelectedPrivateRSADecryptionKey == null)
            {
                MessageBox.Show("Kies een Private RSA Key");
                return;
            }

            // Check if user chose a FileName
            else if (TxtFileNameDecrypt.Text == null || TxtFileNameDecrypt.Text == "")
            {
                MessageBox.Show("Geef een bestandsnaam");
                return;
            }

            try
            {
                // Create RSA and Decrypt the AES Key using RSA
                byte[] decryptedBytes;


                using (RSA rsa = RSA.Create())
                {
                    rsa.KeySize = 2048;
                    rsa.FromXmlString(SelectedPrivateRSADecryptionKey);
                    decryptedBytes = rsa.Decrypt(SelectedAESKeyToDecrypt, RSAEncryptionPadding.Pkcs1);
                }

                // Write the Decrypted AES Key to User chosen FilePath
                File.WriteAllBytes($"{FilePath_AESKey}\\{TxtFileNameDecrypt.Text}_Decrypted_Key.txt", decryptedBytes);

                MessageBox.Show("Key succesvol gedecrypteerd");
            }
            catch (CryptographicException) // Specific exception when Wrong Key is selected
            {
                MessageBox.Show("Wrong Key!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error encrypting");
            }
        }
        #endregion

        #region TabControl
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reset all fields if user changes tabs, this to prevent errors like "File is used by another process"
            SelectedPublicRSAEncryptionKey = null;
            SelectedPrivateRSADecryptionKey = null;
            SelectedAESKeyToEncrypt = null;

            LblAESKeyNaamEncrypt.Content = "Key Name";
            LblPublicRSAKeyNaamEncrypt.Content = "Key Name";

            LblAESKeyNaamDecrypt.Content = ".encrypted file";
            LblPrivateRSAKeyNaamDecrypt.Content = "Key Name";

            TxtFileNameEncrypt.Text = null;
            TxtFileNameDecrypt.Text = null;
        }
        #endregion
    }
}
