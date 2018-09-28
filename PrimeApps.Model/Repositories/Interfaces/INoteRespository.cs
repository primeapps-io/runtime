using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface INoteRepository : IRepositoryBaseTenant
    {
        Task<Note> GetById(int id);
        Task<Note> GetByIdBasic(int id);
        Task<ICollection<Note>> Find(NoteRequest request);
        Task<int> Count(NoteRequest request);
        Task<int> Create(Note note);
        Task<int> Update(Note note);
        Task<int> DeleteSoft(Note note);
        Task<int> DeleteHard(Note note);
    }
}
