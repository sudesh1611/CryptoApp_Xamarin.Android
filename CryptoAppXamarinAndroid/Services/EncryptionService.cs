using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CryptoAppXamarinAndroid.Services
{
    public class EncryptionResult
    {
        public bool Result { get; set; }

        public string Error { get; set; }

        public string EncryptedString { get; set; }

        public byte[] EncryptedBytes { get; set; }

        public string FilePath { get; set; }
    }
    public class EncryptionService
    {
        public static Tuple<decimal, string> GetFormatedSize(Int64 bytes)
        {
            string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return new Tuple<decimal, string>(number, suffixes[counter]);
        }

        public static byte[] GetEncryptedByteArray(byte[] encryptedBytes, byte[] password)
        {
            //the salt bytes must be at least 8 bytes
            byte[] saltBytes = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] encyptedbytes = null;
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(password, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.Zeros;
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                        cs.Close();
                    }
                    encyptedbytes = ms.ToArray();
                }
                return encyptedbytes;
            }
        }

        public static EncryptionResult EncryptText(string inputText, string password)
        {
            EncryptionResult encryptionResult = new EncryptionResult() { Result = false };
            try
            {
                byte[] encryptedBytes = System.Text.Encoding.UTF8.GetBytes(inputText);
                byte[] passwordToByteArray = System.Text.Encoding.UTF8.GetBytes(password);
                passwordToByteArray = SHA256.Create().ComputeHash(passwordToByteArray);
                byte[] encryptedByteArray = EncryptionService.GetEncryptedByteArray(encryptedBytes, passwordToByteArray);
                try
                {
                    encryptionResult.EncryptedString = System.Convert.ToBase64String(encryptedByteArray);
                    encryptionResult.Error = null;
                    encryptionResult.Result = true;
                }
                catch (Exception)
                {
                    encryptionResult.Result = false;
                    encryptionResult.Error = "This string can not be enrypted";
                }
            }
            catch (Exception ex)
            {
                encryptionResult.Result = false;
                encryptionResult.Error = ex.Message;
            }
            return encryptionResult;
        }


        public static EncryptionResult EncryptFile(string inputFile, string password)
        {
            EncryptionResult encryptionResult = null;
            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(inputFile);
                byte[] passwordToByteArray = System.Text.Encoding.ASCII.GetBytes(password);
                passwordToByteArray = SHA256.Create().ComputeHash(passwordToByteArray);
                byte[] SaltedHashedPassword = SHA256.Create().ComputeHash(System.Text.Encoding.ASCII.GetBytes(password + "CryptoApp"));
                byte[] encryptedByteArray = EncryptionService.GetEncryptedByteArray(encryptedBytes, passwordToByteArray);
                string CurrentDirectoryPath = System.IO.Path.GetDirectoryName(inputFile);
                string CurrentFileName = Path.GetFileNameWithoutExtension(inputFile);
                string CurrentFileExtension = Path.GetExtension(inputFile);
                byte[] FileNameBytes = System.Text.Encoding.ASCII.GetBytes(CurrentFileName);
                byte[] ExtensionBytes = System.Text.Encoding.ASCII.GetBytes(CurrentFileExtension);
                int FileNameBytesLength = FileNameBytes.Length;
                int ExtensionBytesLength = ExtensionBytes.Length;
                FileNameBytesLength = (FileNameBytesLength >= 1000) ? 999 : FileNameBytesLength;
                ExtensionBytesLength = (ExtensionBytesLength >= 1000) ? 999 : ExtensionBytesLength;
                byte[] FileContentBytes = new byte[1000 + 1000 + 32 + encryptedByteArray.Length];
                System.Buffer.BlockCopy(FileNameBytes, 0, FileContentBytes, 0, FileNameBytesLength);
                System.Buffer.BlockCopy(ExtensionBytes, 0, FileContentBytes, 1000, ExtensionBytesLength);
                System.Buffer.BlockCopy(SaltedHashedPassword, 0, FileContentBytes, 2000, 32);
                System.Buffer.BlockCopy(encryptedByteArray, 0, FileContentBytes, 2032, encryptedByteArray.Length);
                string CryptoAppPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "Crypto App");
                Directory.CreateDirectory(CryptoAppPath);
                //string WritePath = System.IO.Path.Combine(CurrentDirectoryPath, CurrentFileName + ".senc");
                string WritePath = System.IO.Path.Combine(CryptoAppPath, CurrentFileName + ".senc");
                File.WriteAllBytes(WritePath, FileContentBytes);
                encryptionResult = new EncryptionResult()
                {
                    Result = true,
                    Error = null,
                    EncryptedString = CurrentFileName + ".senc"
                };
            }
            catch (Exception ex)
            {
                encryptionResult = new EncryptionResult()
                {
                    Result = false,
                    Error = ex.Message,
                    EncryptedString = null,
                };
            }
            return encryptionResult;
        }
    }
}