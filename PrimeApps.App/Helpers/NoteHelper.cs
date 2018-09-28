using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public static class NoteHelper
    {
        public static async Task<Note> CreateEntity(NoteBindingModel noteModel, IUserRepository userRepository)
        {
            var note = new Note
            {
                Text = noteModel.Text,
                ModuleId = noteModel.ModuleId,
                RecordId = noteModel.RecordId,
                NoteId = noteModel.NoteId
            };

            await CreateLikesRelations(noteModel, note, userRepository);

            return note;
        }

        public static async Task<Note> UpdateEntity(NoteBindingModel noteModel, Note note, IUserRepository userRepository)
        {
            note.Text = noteModel.Text;
            
            await CreateLikesRelations(noteModel, note, userRepository);

            return note;
        }

        public static async Task<Note> UpdateLikedNote(Note note, int userId, IUserRepository userRepository)
        {
            var likedUser = await userRepository.GetById(userId);

            var noteLike = new NoteLikes();
            noteLike.NoteId = note.Id;
            noteLike.UserId = likedUser.Id;

            int x = 0;
            foreach (var like in note.NoteLikes)
            {
                if (like.UserId == userId)
                {
                    x++;
                    noteLike = like;
                }
                    
            }

            if(x>0)
                note.NoteLikes.Remove(noteLike);
            else
                note.NoteLikes.Add(noteLike);

            return note;
        }

        private static async Task CreateLikesRelations(NoteBindingModel noteModel, Note note, IUserRepository userRepository)
        {
            if (noteModel.Likes != null && noteModel.Likes.Count > 0)
            {
                foreach (var userId in noteModel.Likes)
                {
                    var likedUser = await userRepository.GetById(userId);

                    if (likedUser != null)
                        note.Likes.Add(likedUser);
                }
            }
        }
    }
}