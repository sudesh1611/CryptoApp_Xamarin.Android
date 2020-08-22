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
using SQLite;

namespace CryptoAppXamarinAndroid.MyDatabase
{
    public class CryptoAppNotesDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(DatabaseConstants.NotesDBPath, DatabaseConstants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public CryptoAppNotesDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(NoteModel).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(NoteModel)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }

        public Task<NoteModel> GetNoteWithID(int id)
        {
            return Database.Table<NoteModel>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<List<NoteModel>> GetAllNotesList()
        {
            return Database.Table<NoteModel>().ToListAsync();
        }

        public Task<int> SaveOrUpdateNode(NoteModel note)
        {
            if(note.ID!=0)
            {
                return Database.UpdateAsync(note);
            }
            else
            {
                return Database.InsertAsync(note);
            }
        }

        public Task<int> DeleteNote(NoteModel note)
        {
            return Database.DeleteAsync(note);
        }
    }
}