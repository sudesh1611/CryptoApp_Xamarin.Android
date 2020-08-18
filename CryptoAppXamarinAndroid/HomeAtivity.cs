using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "Home", Theme = "@style/MainTheme")]
    public class HomeAtivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Button EncryptActivityButton = FindViewById<Button>(Resource.Id.EncryptActivityButton);
            EncryptActivityButton.Click += EncryptActivityButton_Click;
            Button DecryptActivityButton = FindViewById<Button>(Resource.Id.DecryptActivityButton);
            DecryptActivityButton.Click += DecryptActivityButton_Click;
            Button NotesActivityButton = FindViewById<Button>(Resource.Id.NotesActivityButton);
            NotesActivityButton.Click += NotesActivityButton_Click;
            Button SettingsActivityButton = FindViewById<Button>(Resource.Id.SettingsActivityButton);
            SettingsActivityButton.Click += SettingsActivityButton_Click;
        }

        private void SettingsActivityButton_Click(object sender, EventArgs e)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Coming Soon", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        private void NotesActivityButton_Click(object sender, EventArgs e)
        {
            StartActivity(new Android.Content.Intent(this, typeof(NewNoteActivity)));
            //View view = (View)sender;
            //Snackbar.Make(view, "Coming Soon", Snackbar.LengthLong)
            //    .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        private void DecryptActivityButton_Click(object sender, EventArgs e)
        {
            StartActivity(new Android.Content.Intent(this, typeof(DecryptActivity)));
        }

        private void EncryptActivityButton_Click(object sender, EventArgs e)
        {
            StartActivity(new Android.Content.Intent(this, typeof(EncryptActivity)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}