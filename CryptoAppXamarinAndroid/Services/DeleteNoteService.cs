using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CryptoAppXamarinAndroid.MyDatabase;
using Xamarin.Essentials;

namespace CryptoAppXamarinAndroid.Services
{
    [Service]
    public class DeleteNoteService : IntentService
    {
        static CryptoAppNotesDatabase cryptoAppNotesDatabase;
        public static CryptoAppNotesDatabase NotesDatabase
        {
            get
            {
                if (cryptoAppNotesDatabase == null)
                {
                    cryptoAppNotesDatabase = new CryptoAppNotesDatabase();
                }
                return cryptoAppNotesDatabase;
            }
        }

        public DeleteNoteService():base("DeleteNoteService")
        {

        }

        protected override void OnHandleIntent(Intent intent)
        {
            Task.Run(async () =>
            {
                try
                {
                    int noteID = int.Parse(intent.GetStringExtra(GlobalConstants.NOTE_ID));
                    NoteModel noteModel = await NotesDatabase.GetNoteWithID(noteID);
                    await NotesDatabase.DeleteNote(noteModel);
                }
                catch (Exception)
                {
                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Note could not be deleted", ToastLength.Long).Show();
                    });
                }
            });
        }
    }
}