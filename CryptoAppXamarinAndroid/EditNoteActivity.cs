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
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CryptoAppXamarinAndroid.Services;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "Edit Note", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, WindowSoftInputMode = SoftInput.StateHidden)]
    public class EditNoteActivity : AppCompatActivity
    {
        EditText editNoteEdittext;
        bool IsChanged = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.EditNote);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.Title = "Crypto Note";
            editNoteEdittext = FindViewById<EditText>(Resource.Id.editNoteEditText);
            if(Intent==null)
            {
                this.Finish();
                return;
            }
            editNoteEdittext.Text = Intent.GetStringExtra(GlobalConstants.NOTE_CONTENT);
            DateTime CreatedDateTime = JsonSerializer.Deserialize<DateTime>(Intent.GetStringExtra(GlobalConstants.NOTE_CREATION_DATE));
            DateTime UpdatedDateTime = JsonSerializer.Deserialize<DateTime>(Intent.GetStringExtra(GlobalConstants.NOTE_UPDATION_DATE));
            FindViewById<TextView>(Resource.Id.BottomTextView).Text = "Created: " + GetFormattedDateTime(CreatedDateTime) + "     Modified: " + GetFormattedDateTime(UpdatedDateTime);
            editNoteEdittext.TextChanged += EditNoteEdittext_TextChanged;
        }

        String GetFormattedDateTime(DateTime dateTime)
        {
            if(dateTime.ToString("dd MMMM yyyy") == DateTime.Now.ToString("dd MMMM yyyy"))
            {
                return "Today " + dateTime.ToString("hh:mm tt");
            }
            if(dateTime.AddDays(1).ToString("dd MMMM yyyy") == DateTime.Now.ToString("dd MMMM yyyy"))
            {
                return "Yesterday " + dateTime.ToString("hh:mm tt");
            }
            if(dateTime.ToString("yyyy") == DateTime.Now.ToString("yyyy"))
            {
                return dateTime.ToString("dd MMMM");
            }
            else
            {
                return dateTime.ToString("dd MMMM yyyy");
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

        private void EditNoteEdittext_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            IsChanged = true;
            if (e.Text.Count() > 1000000)
            {
                ShowDialog("Error", "Maximum allowed note size is 1MB");
                editNoteEdittext.Text = e.Text.ToString().Substring(0, 999990);
                editNoteEdittext.SetSelection(editNoteEdittext.Text.Length);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.EditNoteMenu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_Discard)
            {
                DeleteNote();
                this.Finish();
                return true;
            }
            if (id == Android.Resource.Id.Home)
            {
                DecideAndUpdate();
                this.Finish();
            }

            return base.OnOptionsItemSelected(item);
        }

        void DeleteNote()
        {
            Intent deleteNoteIntent = new Intent(this, typeof(DeleteNoteService));
            deleteNoteIntent.PutExtra(GlobalConstants.NOTE_ID, Intent.GetStringExtra(GlobalConstants.NOTE_ID));
            StartService(deleteNoteIntent);
        }

        void DecideAndUpdate()
        {
            if(IsChanged==false)
            {
                this.Finish();
                return;
            }
            var noteContent = editNoteEdittext.Text;

            if (String.IsNullOrEmpty(noteContent) || String.IsNullOrWhiteSpace(noteContent))
            {
                DeleteNote();
            }
            else
            {
                Intent editNoteIntent = new Intent(this, typeof(EditNoteService));
                editNoteIntent.PutExtra(GlobalConstants.NOTE_ID, Intent.GetStringExtra(GlobalConstants.NOTE_ID));
                editNoteIntent.PutExtra(GlobalConstants.NOTE_CONTENT, noteContent);
                editNoteIntent.PutExtra(GlobalConstants.NOTE_UPDATION_DATE, JsonSerializer.Serialize(DateTime.Now));
                StartService(editNoteIntent);
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
            DecideAndUpdate();
            base.OnBackPressed();
        }
    }
}