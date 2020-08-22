using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CryptoAppXamarinAndroid.Services;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "New Note", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, WindowSoftInputMode = SoftInput.StateHidden)]
    public class NewNoteActivity : AppCompatActivity
    {
        EditText newNoteEdittext;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.NewNote);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.Title = "Crypto Note";
            newNoteEdittext = FindViewById<EditText>(Resource.Id.newNoteEditText);
            newNoteEdittext.TextChanged += NewNoteEdittext_TextChanged;
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

        private void NewNoteEdittext_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if(e.Text.Count() > 1000000)
            {
                ShowDialog("Error", "Maximum allowed note size is 1MB");
                newNoteEdittext.Text = e.Text.ToString().Substring(0, 999990);
                newNoteEdittext.SetSelection(newNoteEdittext.Text.Length);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_Discard)
            {
                this.Finish();
                return true;
            }
            if(id==Android.Resource.Id.Home)
            {
                DecideAndSaveNote();
            }

            return base.OnOptionsItemSelected(item);
        }

        void DecideAndSaveNote()
        {
            var noteContent = newNoteEdittext.Text;

            if (String.IsNullOrEmpty(noteContent) || String.IsNullOrWhiteSpace(noteContent))
            {

            }
            else
            {
                Intent newNoteIntent = new Intent(this, typeof(AddNewNoteService));
                newNoteIntent.PutExtra(GlobalConstants.NOTE_CONTENT, noteContent);
                newNoteIntent.PutExtra(GlobalConstants.NOTE_TITLE, "");
                newNoteIntent.PutExtra(GlobalConstants.NOTE_TYPE, GlobalConstants.TEXT_TYPE_NOTE);
                newNoteIntent.PutExtra(GlobalConstants.NOTE_CREATION_DATE, JsonSerializer.Serialize(DateTime.Now));
                StartService(newNoteIntent);
            }
            Finish();
        }

        

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            DecideAndSaveNote();
            base.OnBackPressed();
        }
    }
}