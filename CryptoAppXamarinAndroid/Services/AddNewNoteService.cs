using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Systems;
using Android.Views;
using Android.Widget;
using CryptoAppXamarinAndroid.MyDatabase;
using Xamarin.Essentials;

namespace CryptoAppXamarinAndroid.Services
{
    [Service]
    public class AddNewNoteService : IntentService
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
        public AddNewNoteService():base("AddNewNoteService")
        {

        }

        protected override void OnHandleIntent(Intent intent)
        {
            Task.Run(async () =>
            {
                try
                {
                    string noteType = intent.GetStringExtra(GlobalConstants.NOTE_TYPE);
                    string noteContent = intent.GetStringExtra(GlobalConstants.NOTE_CONTENT);
                    string noteCreated = intent.GetStringExtra(GlobalConstants.NOTE_CREATION_DATE);
                    string noteTitle = intent.GetStringExtra(GlobalConstants.NOTE_TITLE);
                    if (!String.IsNullOrEmpty(noteType) && !String.IsNullOrEmpty(noteContent))
                    {
                        if (String.IsNullOrEmpty(noteCreated))
                        {
                            noteCreated = JsonSerializer.Serialize(DateTime.Now);
                        }
                        if (noteTitle == null)
                        {
                            noteTitle = String.Empty;
                        }
                        string password = await SecureStorage.GetAsync("AppPassword");
                        if (password == null)
                        {
                            throw new Exception("App's password missing");
                        }
                        EncryptionResult encryptionResult = EncryptionService.EncryptText(noteContent, password);
                        if (encryptionResult.Result)
                        {
                            NoteModel newNote = new NoteModel()
                            {
                                NoteContent = encryptionResult.EncryptedString,
                                NoteTitle = noteTitle,
                                NoteCreationTime = JsonSerializer.Deserialize<DateTime>(noteCreated),
                                NoteType = noteType,
                                NoteLastModifiedTime = JsonSerializer.Deserialize<DateTime>(noteCreated),
                                LastEncrypted = DateTime.Now
                            };
                            await NotesDatabase.SaveOrUpdateNode(newNote);
                        }
                        else
                        {
                            throw new Exception(encryptionResult.Error);
                        }
                    }
                }
                catch (Exception)
                {
                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Note could not be saved", ToastLength.Long).Show();
                    });
                }
            });
        }
    }
}