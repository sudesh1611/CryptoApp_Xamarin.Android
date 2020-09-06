using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.IO;
using AndroidX.Work;
using CryptoAppXamarinAndroid.Services;
using Xamarin.Essentials;
using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Util.Concurrent;
using Android.App.Job;
using Android.Content;
using Android.Content.PM;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MainTheme",NoHistory =true, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : AppCompatActivity
    {
        Button SaveButton;
        EditText password;
        EditText confpassword;
        TextView heading;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_Password);
            
            SaveButton = FindViewById<Button>(Resource.Id.SaveButton);
            password = FindViewById<EditText>(Resource.Id.PasswordEntry);
            confpassword= FindViewById<EditText>(Resource.Id.ConfirmPasswordEntry);
            heading = FindViewById<TextView>(Resource.Id.PasswordHeading);
            SaveButton.Click += SaveButton_Click;
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            SaveButton.Enabled = false;
            if(String.IsNullOrEmpty(password.Text) || password.Text.Length<6 || password.Text.Length>16)
            {
                ShowDialog("Error", "Password length should be at least 6 and at most 16");
                SaveButton.Enabled = true;
                return;
            }
            if (String.IsNullOrEmpty(confpassword.Text) || confpassword.Text.Length < 6 || confpassword.Text.Length > 16)
            {
                ShowDialog("Error", "Confirm Password length should be at least 6 and at most 16");
                SaveButton.Enabled = true;
                return;
            }
            if(confpassword.Text!=password.Text)
            {
                ShowDialog("Error", "Passwords do not match");
                SaveButton.Enabled = true;
                return;
            }
            SaveButton.Text = "Saving Password";
            try
            {
                await SecureStorage.SetAsync("AppPassword", password.Text);
                await SecureStorage.SetAsync("FirstRun", "True");
                ShowDialog2("Success", "Password saved. If you forget your password, you will not be able to decrypt anything.");
            }
            catch (Exception)
            {

                Toast.MakeText(this, "Something went wrong", ToastLength.Long).Show();
                Toast.MakeText(this, "Close application and try again", ToastLength.Long).Show();
            }

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

        private void ShowDialog2(string title, string messaage)
        {
            Android.Support.V7.App.AlertDialog.Builder alertDiag = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDiag.SetTitle(title);
            alertDiag.SetMessage(messaage);
            alertDiag.SetPositiveButton("Okay", (senderAlert, args) =>
            {
                StartActivity(new Android.Content.Intent(this, typeof(HomeAtivity)));
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
