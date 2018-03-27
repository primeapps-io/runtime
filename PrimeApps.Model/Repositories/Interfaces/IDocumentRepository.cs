using System.Collections.Generic;
using PrimeApps.Model.Entities.Application;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Document;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDocumentRepository : IRepositoryBaseTenant
    {
        Task<Document> CreateAsync(Document document);
        Task<Document> UpdateAsync(Document document);
        Task<Document> RemoveAsync(int id, bool hardDelete = false);
        Task<Document> GetById(int id);
        Task<ICollection<DocumentResult>> GetDocumentsByLimit(DocumentFindRequest request);
        Task<int> Count(DocumentFindRequest request);
        Task<DocumentsResultSet> GetTopEntityDocuments(DocumentRequest request);
        Task<DocumentsResultSet> GetDocuments(DocumentRequest request);
        Task<ICollection<Document>> GetAll(int moduleId, int recordId);
        Task<int> Update(Document document);
        bool IsFieldDocumentSearchable(int moduleId, string fieldName);

    }
}
