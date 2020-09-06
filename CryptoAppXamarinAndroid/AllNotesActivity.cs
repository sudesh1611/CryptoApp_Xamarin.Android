using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CryptoAppXamarinAndroid.MyDatabase;
using CryptoAppXamarinAndroid.Services;
using Xamarin.Essentials;

namespace CryptoAppXamarinAndroid
{
    [Activity(Label = "All Notes", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AllNotesActivity : AppCompatActivity
    {
        static CryptoAppNotesDatabase cryptoAppNotesDatabase;
        bool IsLoading = false;
        bool IsBusy = false;
        IMenu MainMenu;
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
        RecyclerView mRecyclerView;
        List<NoteModel> AllNotes;
        NotesAdapter notesAdapter;
        SwipeRefreshLayout swipeContainer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.AllNotesView);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            swipeContainer = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeContainer);
            swipeContainer.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight);
            swipeContainer.Refresh += SwipeContainer_Refresh;
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mRecyclerView.SetLayoutManager(new StaggeredGridLayoutManager(2, StaggeredGridLayoutManager.Vertical));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.AllActivityMenu, menu);
            MainMenu = menu;
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_Disclaimer)
            {
                ShowDialog("Info", "All of the Crypto App notes will get deleted if you clear app's data or uninstall app. It is advised to export notes before uninstalling app.");
                return true;
            }
            if (id == Android.Resource.Id.Home)
            {
                this.Finish();
            }
            if (id == Resource.Id.action_ExportNotes)
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var PermissionStatus = await CheckAndRequestStoragePermission();
                    if (PermissionStatus != PermissionStatus.Granted)
                    {
                        ShowDialog("Permission Error", "Storage permission is required to export notes.");
                        return;
                    }
                    Intent exportNotesJson = new Intent(this, typeof(ExportNoteAsJSONService));
                    ShowDialog("Info", "Notes will be exported in JSON format to Crypto App folder");
                    StartService(exportNotesJson);
                });
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void SwipeContainer_Refresh(object sender, EventArgs e)
        {
            if (IsLoading)
            {
                return;
            }
            IsLoading = true;
            swipeContainer.Refreshing = true;
            FetchAndSetView(null, null);
        }

        protected override void OnResume()
        {
            base.OnResume();
            IsBusy = false;
            FetchAndSetView(null, null);
        }

        private async void FetchAndSetView(object sender, EventArgs e)
        {
            if (mRecyclerView.GetAdapter() != null)
            {
                await Task.Delay(2000);
            }
            var TempAllNotes = await NotesDatabase.GetAllNotesList();
            AllNotes = new List<NoteModel>();
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
                            TempAllNotes[i].NoteContent = DecryptionResult.DecryptedString;
                            AllNotes.Add(TempAllNotes[i]);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            AllNotes = AllNotes.OrderByDescending(i => i.NoteLastModifiedTime.Ticks).ToList();
            notesAdapter = new NotesAdapter(AllNotes);
            notesAdapter.ItemClick += NotesAdapter_ItemClick;
            if (mRecyclerView.GetAdapter() == null)
            {
                mRecyclerView.SetAdapter(notesAdapter);
            }
            else
            {
                mRecyclerView.SwapAdapter(notesAdapter, false);
            }
            IsLoading = false;
            swipeContainer.Refreshing = false;
        }


        private void NotesAdapter_ItemClick(object sender, int position)
        {
            if (IsBusy == false)
            {
                IsBusy = true;
                Intent intent = new Intent(this, typeof(EditNoteActivity));
                intent.PutExtra(GlobalConstants.NOTE_ID, AllNotes[position].ID.ToString());
                intent.PutExtra(GlobalConstants.NOTE_TITLE, AllNotes[position].NoteTitle);
                intent.PutExtra(GlobalConstants.NOTE_CONTENT, AllNotes[position].NoteContent);
                intent.PutExtra(GlobalConstants.NOTE_CREATION_DATE, JsonSerializer.Serialize(AllNotes[position].NoteCreationTime));
                intent.PutExtra(GlobalConstants.NOTE_UPDATION_DATE, JsonSerializer.Serialize(AllNotes[position].NoteLastModifiedTime));
                StartActivity(intent);
                IsBusy = false;
            }
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            StartActivity(new Android.Content.Intent(this, typeof(NewNoteActivity)));
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
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

        public async Task<PermissionStatus> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return status;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class NoteViewHolder : RecyclerView.ViewHolder
    {
        public TextView NoteText { get; private set; }

        public NoteViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            NoteText = itemView.FindViewById<TextView>(Resource.Id.textView);
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    public class NotesAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<int> ItemClick;


        // Underlying data set (a photo album):
        List<NoteModel> AllnoteModels;

        // Load the adapter with the data set (photo album) at construction time:
        public NotesAdapter(List<NoteModel> noteModels)
        {
            AllnoteModels = noteModels;
        }

        // Create a new photo CardView (invoked by the layout manager): 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.NoteCardView, parent, false);

            // Create a ViewHolder to find and hold these view references, and 
            // register OnClick with the view holder:
            NoteViewHolder vh = new NoteViewHolder(itemView, OnClick);
            return vh;
        }

        // Fill in the contents of the photo card (invoked by the layout manager):
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            NoteViewHolder vh = holder as NoteViewHolder;

            // Set the ImageView and TextView in this ViewHolder's CardView 
            // from this position in the photo album:
            //vh.Image.SetImageResource(mPhotoAlbum[position].PhotoID);
            if (AllnoteModels[position].NoteContent.Length > 200)
            {
                var truncText = AllnoteModels[position].NoteContent.Substring(0, 199);
                var Alllines = truncText.Split('\n');
                if (Alllines.Length > 6)
                {
                    vh.NoteText.Text = new StringBuilder().AppendJoin('\n', Alllines[0..5]).ToString();
                }
                else
                {
                    vh.NoteText.Text = truncText;
                }
            }
            else
            {
                var truncText = AllnoteModels[position].NoteContent;
                var Alllines = truncText.Split('\n');
                if (Alllines.Length > 6)
                {
                    vh.NoteText.Text = new StringBuilder().AppendJoin('\n', Alllines[0..5]).ToString();
                }
                else
                {
                    vh.NoteText.Text = truncText;
                }
            }

        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return AllnoteModels.Count; }
        }

        // Raise an event when the item-click takes place:
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
    }
}