using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    public class EditNoteService : IntentService
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

        public EditNoteService() : base("EditNoteService")
        {

        }

        protected override void OnHandleIntent(Intent intent)
        {
            Task.Run(async () =>
            {
                try
                {
                    int noteID = int.Parse(intent.GetStringExtra(GlobalConstants.NOTE_ID));
                    string noteContent = intent.GetStringExtra(GlobalConstants.NOTE_CONTENT);
                    DateTime noteUpdateTime = JsonSerializer.Deserialize<DateTime>(intent.GetStringExtra(GlobalConstants.NOTE_UPDATION_DATE));
                    string password = await SecureStorage.GetAsync("AppPassword");
                    if (password == null)
                    {
                        throw new Exception("App's password missing");
                    }
                    NoteModel noteModel = await NotesDatabase.GetNoteWithID(noteID);
                    if (noteModel == null)
                    {
                        throw new Exception("Note doesn't exist");
                    }
                    EncryptionResult encryptionResult = EncryptionService.EncryptText(noteContent, password);
                    if (encryptionResult.Result)
                    {
                        noteModel.LastEncrypted = DateTime.Now;
                        noteModel.NoteContent = encryptionResult.EncryptedString;
                        noteModel.NoteLastModifiedTime = noteUpdateTime;
                        await NotesDatabase.SaveOrUpdateNode(noteModel);
                    }
                    else
                    {
                        throw new Exception(encryptionResult.Error);
                    }
                }
                catch (Exception)
                {
                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Note could not be updated", ToastLength.Long).Show();
                    });
                }
            });
        }
    }
}