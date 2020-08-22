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

namespace CryptoAppXamarinAndroid
{
    public class GlobalConstants
    {
        public const string NOTE_ID = "NOTE_ID";
        public const string NOTE_CONTENT = "NOTE_CONTENT";
        public const string NOTE_CREATION_DATE = "NOTE_CREATION_DATE";
        public const string NOTE_UPDATION_DATE = "NOTE_UPDATION_DATE";
        public const string NOTE_TITLE = "NOTE_TITLE";
        public const string NOTE_TYPE = "NOTE_TYPE";
        public const string TEXT_TYPE_NOTE = "TEXT_TYPE_NOTE";
        public const string IMAGE_TYPE_NOTE = "IMAGE_TYPE_NOTE";
        public const int NOTIFICATION_ID = 1024;
    }
}