using System;
using System.Collections.Generic;
using System.IO;
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
    public class ExportNoteJsonModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModificationTime { get; set; }

        public ExportNoteJsonModel(string title,string content, DateTime creationTime, DateTime modifyTime)
        {
            Title = title;
            Content = content;
            CreationTime = creationTime;
            ModificationTime = modifyTime;
        }
    }
    [Service]
    public class ExportNoteAsJSONService : IntentService
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
        public ExportNoteAsJSONService():base("ExportNoteAsJSONService")
        {

        }

        protected override void OnHandleIntent(Intent intent)
        {
            Task.Run(async () =>
            {
                try
                {
                    var TempAllNotes = await NotesDatabase.GetAllNotesList();
                    var AllNotes = new List<ExportNoteJsonModel>();
                    if (TempAllNotes != null)
                    {
                        string password = await SecureStorage.GetAsync("AppPassword");
                        if (password == null)
                        {
                            return;
                        }
                        for (int i = 0; i < TempAllNotes.Count; i++)
                        {
                            try
                            {
                                var DecryptionResult = DecryptionService.DecryptText(TempAllNotes[i].NoteContent, password);
                                if (DecryptionResult.Result)
                                {
                                    AllNotes.Add(new ExportNoteJsonModel(TempAllNotes[i].NoteTitle, DecryptionResult.DecryptedString, TempAllNotes[i].NoteCreationTime, TempAllNotes[i].NoteLastModifiedTime));
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    var CryptoAppPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "Crypto App");
                    if (!Directory.Exists(CryptoAppPath))
                    {
                        System.IO.Directory.CreateDirectory(CryptoAppPath);
                    }
                    string ExportFilePath = System.IO.Path.Combine(CryptoAppPath, "CryptoApp_Notes_Export_" + DateTime.Now.ToString("ddMMyyyyHmmss") + ".json");
                    File.WriteAllText(ExportFilePath, System.Text.Json.JsonSerializer.Serialize(AllNotes, jsonSerializerOptions));
                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Note exported", ToastLength.Long).Show();
                    });
                }
                catch (Exception)
                {
                    Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Toast.MakeText(Android.App.Application.Context, "Note could not be exported", ToastLength.Long).Show();
                    });
                }
            });
        }
    }
}