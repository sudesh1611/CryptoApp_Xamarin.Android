using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MainTheme", MainLauncher = true,NoHistory =true)]
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
            Button button = new Button(this);
            button.Click += Button_Click;
            SaveButton = FindViewById<Button>(Resource.Id.SaveButton);
            password = FindViewById<EditText>(Resource.Id.PasswordEntry);
            confpassword= FindViewById<EditText>(Resource.Id.ConfirmPasswordEntry);
            heading = FindViewById<TextView>(Resource.Id.PasswordHeading);
            SaveButton.Click += SaveButton_Click;
            Button_Click(null, null);

            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
            //Button EncryptActivityButton = FindViewById<Button>(Resource.Id.EncryptActivityButton);
            //EncryptActivityButton.Click += EncryptActivityButton_Click;
            //Button DecryptActivityButton = FindViewById<Button>(Resource.Id.DecryptActivityButton);
            //DecryptActivityButton.Click += DecryptActivityButton_Click;
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
                ShowDialog("Success", "Password saved. If you forget your password, you will not be able to decrypt anything.");
                //StartActivity(new Android.Content.Intent(this, typeof(HomeAtivity)));
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
            alertDiag.SetPositiveButton("Confirm", (senderAlert, args) =>
            {
                StartActivity(new Android.Content.Intent(this, typeof(HomeAtivity)));
            });
            Dialog diag = alertDiag.Create();
            diag.Show();
        }

        private async void Button_Click(object sender, EventArgs e)
        {
            try
            {
                var FirstRun = await SecureStorage.GetAsync("FirstRun");
                if(FirstRun!=null)
                {
                    StartActivity(new Android.Content.Intent(this, typeof(HomeAtivity)));
                }
                else
                {
                    heading.Visibility = ViewStates.Visible;
                    password.Visibility = ViewStates.Visible;
                    confpassword.Visibility = ViewStates.Visible;
                    SaveButton.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "This app can not run", ToastLength.Long).Show();
                heading.Text = "This app can not run";
                heading.Visibility = ViewStates.Visible;
                password.Visibility = ViewStates.Invisible;
                confpassword.Visibility = ViewStates.Invisible;
                SaveButton.Visibility = ViewStates.Invisible;
            }
        }


        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.menu_main, menu);
        //    return true;
        //}

        //public override bool OnOptionsItemSelected(IMenuItem item)
        //{
        //    int id = item.ItemId;
        //    if (id == Resource.Id.action_settings)
        //    {
        //        return true;
        //    }

        //    return base.OnOptionsItemSelected(item);
        //}

        //private void FabOnClick(object sender, EventArgs eventArgs)
        //{
        //    View view = (View) sender;
        //    Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
        //        .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        //}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
