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

namespace CryptoAppXamarinAndroid.Services
{
    [Service]
    public class AddNewNoteService : Service
    {
        bool isStarted;
        public override void OnCreate()
        {
            base.OnCreate();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if(isStarted)
            {

            }
            else
            {
                RegisterForegroundService();
                isStarted = true;
            }
            return StartCommandResult.NotSticky;
        }

        PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(HomeAtivity));
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }

        void RegisterForegroundService()
        {
            var notification = new Notification.Builder(this)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("Saving New Note")
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOngoing(true)
                .Build();

            StartForeground(45, notification);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}