using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;
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
            int x = 0;
            foreach (var user in note.Likes)
            {
                if (user.Id == userId)
                    x++;
            }

            if(x>0)
                note.Likes.Remove(likedUser);
            else
                note.Likes.Add(likedUser);

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