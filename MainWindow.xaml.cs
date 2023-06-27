using System;
using System.IO;
using System.Windows;
using System.Security.Cryptography;

using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Forms;

namespace Encryptie_Tools
{
    public partial class MainWindow : Window
    {
        #region Variables
        private string KeyName;
        private string filePath_Keys;
        public string FilePath_Keys
        {
            get { return filePath_Keys; }
            set { filePath_Keys = value; }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            FilePath_Keys = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Keys";
            string encryptedImagesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Encrypted Images";
            string decryptedImagesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Decrypted Images";
            string plainImagesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EncryptionTool_Data\\Plain Images";

            if (!Directory.Exists(FilePath_Keys))
            {
                Directory.CreateDirectory(FilePath_Keys);
            }
            if (!Directory.Exists(encryptedImagesPath))
            {
                Directory.CreateDirectory(encryptedImagesPath);
            }
            if (!Directory.Exists(decryptedImagesPath))
            {
                Directory.CreateDirectory(decryptedImagesPath);
            }
            if (!Directory.Exists(plainImagesPath))
            {
                Directory.CreateDirectory(plainImagesPath);
            }

            // Warn User about Stock File Locations
            MessageBox.Show("The stock location for all files is in the Documents folder under EncryptionTool_Data\n\nThis can be changed from each windows top menu bar");
        }

        #region Kies Standaard Locatie
        private void MenuKeyStorage_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath_Keys = dialog.SelectedPath;
                }
            }
        }
        #endregion

        #region Generate Buttons and Coresponding Methods
        private void BtnGenereerAES_Click(object sender, RoutedEventArgs e)
        {
            if (TxtSleutel.Text == "" || TxtSleutel.Text == null)
            {
                MessageBox.Show("No key name entered");
            }
            else
            {
                KeyName = TxtSleutel.Text;
                
                // Generate a random AES key and IV
                Aes aes = Aes.Create();

                aes.KeySize = 128;
                aes.BlockSize = 128;

                aes.GenerateKey();
                aes.GenerateIV();

                // Convert the key and IV to Base64 strings
                string keyBase64 = Convert.ToBase64String(aes.Key);
                string ivBase64 = Convert.ToBase64String(aes.IV);

                using (StreamWriter sw = new StreamWriter(FilePath_Keys + "/AES_" + KeyName + ".txt"))
                {
                    sw.WriteLine(keyBase64);
                    sw.WriteLine(ivBase64);
                }

                MessageBox.Show("Key saved successfully");
            }
        }
        private void BtnGenereerRSA_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath_Keys == "" || FilePath_Keys == null)
            {
                MessageBox.Show("No key name entered");
            }
            else
            {
                KeyName = TxtSleutel.Text;

                // Create an instance of RSACryptoServiceProvider
                using (RSA rsa = RSA.Create())
                {
                    rsa.KeySize = 2048;

                    // Export the public key as a string
                    string publicKey = rsa.ToXmlString(false);

                    // Export the private key as a string
                    string privateKey = rsa.ToXmlString(true);

                    // Write the public and private key to a sw
                    using (StreamWriter sw = new StreamWriter(FilePath_Keys + "\\RSA_Public_" + KeyName + ".xml"))
                    {
                        sw.Write(publicKey);
                    }
                    using (StreamWriter sw = new StreamWriter(FilePath_Keys + "\\RSA_Private_" + KeyName + ".xml"))
                    {
                        sw.Write(privateKey);
                    }
                }

                MessageBox.Show("Key saved successfully");
            }
        }
        #endregion

        #region AES & RSA Windows
        private void MenuItemAES_Click(object sender, RoutedEventArgs e)
        {
            AESWindow aesWindow = new AESWindow(this);
            aesWindow.ShowDialog();
        }
        private void MenuItemRSA_Click(object sender, RoutedEventArgs e)
        {
            RSAWindow rsaWindow = new RSAWindow(this);
            rsaWindow.ShowDialog();
        }
        #endregion
    }
}
