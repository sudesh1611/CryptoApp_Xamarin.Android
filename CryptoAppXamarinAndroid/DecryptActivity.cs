using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;
using CryptoAppXamarinAndroid.Services;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "Decrypt", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, WindowSoftInputMode = SoftInput.StateHidden)]
    public class DecryptActivity : AppCompatActivity
    {
        Button OpenFileButton;
        TextView SelectedFileNameLabel;
        Button RemoveButton;
        EditText TextInputEditor;
        Switch PasswordSwitch;
        EditText CustomPasswordEntry;
        Button DecryptButton;
        GridLayout MainGridLayout;
        GridLayout ActivityGridLayout;
        string fileData = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_Decrypt);
            OpenFileButton = FindViewById<Button>(Resource.Id.OpenFileButton);
            OpenFileButton.Click += OpenFileButton_Click;
            SelectedFileNameLabel = FindViewById<TextView>(Resource.Id.SelectedFileNameLabel);
            RemoveButton = FindViewById<Button>(Resource.Id.RemoveButton);
            RemoveButton.Click += RemoveButton_Click;
            TextInputEditor = FindViewById<EditText>(Resource.Id.TextInputEditor);
            TextInputEditor.TextChanged += TextInputEditor_TextChanged;
            PasswordSwitch = FindViewById<Switch>(Resource.Id.PasswordSwitch);
            PasswordSwitch.CheckedChange += PasswordSwitch_CheckedChange;
            CustomPasswordEntry = FindViewById<EditText>(Resource.Id.CustomPasswordEntry);
            DecryptButton = FindViewById<Button>(Resource.Id.DecryptButton);
            DecryptButton.Click += DecryptButton_Click;
            ActivityGridLayout = FindViewById<GridLayout>(Resource.Id.ActivityGrid);
            MainGridLayout = FindViewById<GridLayout>(Resource.Id.MainGrid);
        }

        private void ShowDialog(string title, string messaage)
        {
            Android.Support.V7.App.AlertDialog.Builder alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDiag.SetTitle(title);
            alertDiag.SetMessage(messaage);
            alertDiag.SetPositiveButton("Okay", (senderAlert, args) =>
            {
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
        }

        private async void OpenFileButton_Click(object sender, EventArgs e)
        {
            var PermissionStatus = await CheckAndRequestStoragePermission();
            if (PermissionStatus != PermissionStatus.Granted)
            {
                ShowDialog("Permission Error", "Storage permission is required to decrypt files.");
                return;
            }
            try
            {
                if (!String.IsNullOrEmpty(TextInputEditor.Text))
                {
                    fileData = null;
                    ShowDialog("Error", "You can decrypt either file or text at a time. First, clear the text!");
                    return;
                }
                OpenFileButton.Enabled = false;
                DecryptButton.Enabled = false;
                RemoveButton.Visibility = ViewStates.Invisible;
                SelectedFileNameLabel.Text = "No file selected";

                Intent intent = new Intent(Intent.ActionOpenDocument);
                intent.AddCategory(Intent.CategoryOpenable);
                intent.SetType("*/*");
                StartActivityForResult(intent, 5);
            }
            catch (Exception ex)
            {
                ShowDialog("Error", "Unable to open file. Ensure that storage permission is granted.");
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 5)
            {
                if (resultCode == Result.Ok && data != null)
                {
                    fileData = GetActualPathFromFile(data.Data);
                    string fileName = System.IO.Path.GetFileName(fileData);
                    if (!String.IsNullOrEmpty(fileName) && fileName.Length > 50)
                    {
                        SelectedFileNameLabel.Text = fileName.Substring(0, 50);
                    }
                    else
                    {
                        SelectedFileNameLabel.Text = fileName;
                    }
                    OpenFileButton.Enabled = true;
                    DecryptButton.Enabled = true;
                    RemoveButton.Visibility = ViewStates.Visible;
                    ShowDialog("Success", "You selected \"" + fileName + "\"");
                }
                else
                {
                    fileData = null;
                    OpenFileButton.Enabled = true;
                    SelectedFileNameLabel.Text = "No file selected";
                    RemoveButton.Visibility = ViewStates.Invisible;
                    return;
                }
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            fileData = null;
            SelectedFileNameLabel.Text = "No file selected";
            DecryptButton.Enabled = false;
            OpenFileButton.Enabled = true;
            RemoveButton.Visibility = ViewStates.Invisible;
        }

        private void TextInputEditor_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TextInputEditor.Text))
            {
                if (TextInputEditor.Text.Length > 51400)
                {
                    Toast.MakeText(this, "Invalid Text", ToastLength.Long).Show();
                    ShowDialog("Warning", "Text size is larger than 50KB and can not be decrypted");
                    TextInputEditor.Text = String.Empty;
                }
            }
            if (String.IsNullOrEmpty(TextInputEditor.Text) && fileData == null)
            {
                DecryptButton.Enabled = false;
            }
            else
            {
                DecryptButton.Enabled = true;
            }
        }

        private void PasswordSwitch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                CustomPasswordEntry.Visibility = ViewStates.Invisible;
                CustomPasswordEntry.Text = String.Empty;
            }
            else
            {
                CustomPasswordEntry.Visibility = ViewStates.Visible;
                CustomPasswordEntry.Text = String.Empty;
            }
        }

        private async void DecryptButton_Click(object sender, EventArgs e)
        {
            var RemoveButtonState = DisableAllControls();
            if (String.IsNullOrEmpty(TextInputEditor.Text) && fileData == null)
            {
                RestoreAllControls(RemoveButtonState);
                ShowDialog("Error", "Choose file or write/paste text to encrypt!");
                DecryptButton.Enabled = false;
                return;
            }
            if (!String.IsNullOrEmpty(TextInputEditor.Text) && fileData != null)
            {
                RestoreAllControls(RemoveButtonState);
                ShowDialog("Error", "You can encrypt either file or text at a time!");
                return;
            }
            string password = "";
            try
            {
                password = await SecureStorage.GetAsync("AppPassword");
                if (password == null)
                {
                    ShowDialog("Error", "App's password is missing. Close application and try again!");
                    return;
                }
            }
            catch (Exception)
            {
                ShowDialog("Error", "App's password is missing. Close application and try again!");
                return;
            }
            if (!PasswordSwitch.Checked)
            {
                if (String.IsNullOrEmpty(CustomPasswordEntry.Text) || CustomPasswordEntry.Text.Length < 6 || CustomPasswordEntry.Text.Length > 16)
                {
                    RestoreAllControls(RemoveButtonState);
                    ShowDialog("Error", "Custom Password length should be at least 6 and at most 16");
                    return;
                }
                password = CustomPasswordEntry.Text;
            }
            if (fileData == null)
            {
                DecryptionResult decryptionResult = null;
                await Task.Run(() =>
                {
                    decryptionResult = DecryptionService.DecryptText(TextInputEditor.Text, password);
                });
                if (decryptionResult.Result)
                {
                    ClipboardManager clipboardManager = (ClipboardManager)this.GetSystemService(Context.ClipboardService);
                    clipboardManager.PrimaryClip = ClipData.NewPlainText("Decrypted Text", decryptionResult.DecryptedString);
                    Toast.MakeText(this, "Decrypted text copied to clipboard", ToastLength.Long);
                    fileData = null;
                    RestoreAllControls(ViewStates.Invisible);
                    RemoveButton_Click(null, null);
                    TextInputEditor.Text = String.Empty;
                    CustomPasswordEntry.Text = String.Empty;
                    PasswordSwitch.Checked = true;
                    ShowDialog("Decrypted Text", decryptionResult.DecryptedString);
                }
                else
                {
                    RestoreAllControls(RemoveButtonState);
                    ShowDialog("Failed", "Decryption failed. This text can not be encrypted. Error: \"" + decryptionResult.Error + "\"");
                }
            }
            else
            {
                DecryptionResult decryptionResult = null;
                await Task.Run(() =>
                {
                    decryptionResult = DecryptionService.DecryptFile(fileData, password);
                });
                if (decryptionResult.Result)
                {
                    RestoreAllControls(ViewStates.Invisible);
                    ShowDialog("Success", "File decrypted. Decrypted filename is \"" + decryptionResult.DecryptedString + "\" and stored at \"Crypto App"  + "\" folder.");
                    fileData = null;
                    RemoveButton_Click(null, null);
                    TextInputEditor.Text = String.Empty;
                    CustomPasswordEntry.Text = String.Empty;
                    PasswordSwitch.Checked = true;
                }
                else if (decryptionResult.Result == false && decryptionResult.Error == "Password")
                {
                    RestoreAllControls(RemoveButtonState);
                    PasswordSwitch.Checked = false;
                    CustomPasswordEntry.Visibility = ViewStates.Visible;
                    ShowDialog("Error", "Password is wrong. Enter correct password to decrypt.");
                    return;
                }
                else
                {
                    RestoreAllControls(RemoveButtonState);
                    ShowDialog("Failed", "File decryption failed. This file can not be decrypted.Error: \"" + decryptionResult.Error + "\"");
                }
            }
        }

        private ViewStates DisableAllControls()
        {
            //ActivityIndicatorLayout.IsVisible = true;
            MainGridLayout.Visibility = ViewStates.Invisible;
            ActivityGridLayout.Visibility = ViewStates.Visible;
            DecryptButton.Text = "Decrypting...";
            var RemoveButtonState = RemoveButton.Visibility;
            DecryptButton.Enabled = false;
            OpenFileButton.Enabled = false;
            RemoveButton.Visibility = ViewStates.Invisible;
            TextInputEditor.Enabled = false;
            PasswordSwitch.Enabled = false;
            CustomPasswordEntry.Enabled = false;
            return RemoveButtonState;
        }

        private void RestoreAllControls(ViewStates RemoveButtonState)
        {
            //ActivityIndicatorLayout.IsVisible = false;
            MainGridLayout.Visibility = ViewStates.Visible;
            ActivityGridLayout.Visibility = ViewStates.Invisible;
            DecryptButton.Enabled = true;
            OpenFileButton.Enabled = true;
            RemoveButton.Visibility = RemoveButtonState;
            TextInputEditor.Enabled = true;
            PasswordSwitch.Enabled = true;
            CustomPasswordEntry.Enabled = true;
            DecryptButton.Text = "Start Decryption";
        }

        private string GetActualPathFromFile(Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(this, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    string[] split = docId.Split(chars);
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {
                    string id = DocumentsContract.GetDocumentId(uri);

                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                                    Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    //System.Diagnostics.Debug.WriteLine(contentUri.ToString());

                    return getDataColumn(this, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    String[] split = docId.Split(chars);

                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[]
                    {
                split[1]
                    };

                    return getDataColumn(this, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {

                // Return the remote address
                if (isGooglePhotosUri(uri))
                    return uri.LastPathSegment;

                return getDataColumn(this, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection, String[] selectionArgs)
        {
            ICursor cursor = null;
            String column = "_data";
            String[] projection =
            {
        column
    };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        //Whether the Uri authority is ExternalStorageProvider.
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is DownloadsProvider.
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is MediaProvider.
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is Google Photos.
        public static bool isGooglePhotosUri(Android.Net.Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
        }


        public async Task<PermissionStatus> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return status;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            if (ActivityGridLayout.Visibility == ViewStates.Visible)
            {
                Toast.MakeText(this, "Please wait! Decryption in progress", ToastLength.Long);
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}