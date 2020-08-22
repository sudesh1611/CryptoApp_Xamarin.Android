using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace CryptoAppXamarinAndroid.MyDatabase
{
    public class NoteModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string NoteTitle { get; set; }

        public string NoteType { get; set; }

        public string NoteContent { get; set; }

        public DateTime NoteCreationTime { get; set; }

        public DateTime NoteLastModifiedTime { get; set; }

        public DateTime LastEncrypted { get; set; }

    }

}