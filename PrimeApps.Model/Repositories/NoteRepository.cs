using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class NoteRepository : RepositoryBaseTenant, INoteRepository
    {
        public NoteRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Note> GetById(int id)
        {
            var note = await DbContext.Notes
                .Include(x => x.CreatedBy)
                .Include(x => (x.Notes as Note).CreatedBy)
                .Include(x => (x.Notes as Note).NoteLikes)
                .Include(x => x.NoteLikes)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            note.Likes = note.NoteLikes.Select(x => x.TenantUser).ToList();

            if (note.Notes.Count > 0)
            {
                foreach (var subNote in note.Notes)
                {
                    subNote.Likes = subNote.NoteLikes.Select(x => x.TenantUser).ToList();
                }
            }

            return note;
        }

        public async Task<Note> GetByIdBasic(int id)
        {
            var note = await DbContext.Notes
                .Include(x => x.NoteLikes)
                .ThenInclude(y => y.TenantUser)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return note;
        }

        public async Task<ICollection<Note>> Find(NoteRequest request)
        {
            var notes = GetNoteQuery(request);

            notes = notes
                .OrderByDescending(x => x.CreatedAt)
                .Skip(request.Offset)
                .Take(request.Limit);

            var noteList = await notes.ToListAsync();

            foreach (var note in noteList)
            {
                note.Likes = note.NoteLikes.Select(x => x.TenantUser).ToList();

                if(note.Notes.Count > 0)
                {
                    foreach (var subNote in note.Notes)
                    {
                        subNote.Likes = subNote.NoteLikes.Select(x => x.TenantUser).ToList();
                    }
                }
            }

            return noteList;
        }

        public async Task<int> Count(NoteRequest request)
        {
            var totalCount = await GetNoteQuery(request, false).CountAsync();

            return totalCount;
        }

        public async Task<int> Create(Note note)
        {
            DbContext.Notes.Add(note);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Note note)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Note note)
        {
            note.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Note note)
        {
            DbContext.Notes.Remove(note);

            return await DbContext.SaveChangesAsync();
        }

        private IQueryable<Note> GetNoteQuery(NoteRequest request, bool withIncludes = true)
        {
            var notes = DbContext.Notes.Include(x=>x.CreatedBy)
                .Where(x => !x.Deleted);

            if (withIncludes)
            {
				notes = notes
                    .Include(x => (x.Notes as Note).CreatedBy)
                    .Include(x => (x.Notes as Note).NoteLikes)
                    .Include(x => x.NoteLikes)
                    .Include(x => x.Module)
					.Include(x => x.Module).ThenInclude(y => y.Fields)
					.Include(x => x.CreatedBy);

				/*notes = notes
						.Include(x => x.Notes)
						.Include(x => x.Likes).ThenInclude(y => y.TenantUser)
						.Include(x => x.Module).ThenInclude(y => y.Fields)
						.Include(x => x.CreatedBy);*/
			}

            if (request.ModuleId.HasValue)
                notes = notes.Where(x => x.ModuleId == request.ModuleId.Value);

            if (request.RecordId.HasValue)
                notes = notes.Where(x => x.RecordId == request.RecordId.Value);

            notes = notes.Where(x => !x.NoteId.HasValue);

            return notes;
        }
    }
}

