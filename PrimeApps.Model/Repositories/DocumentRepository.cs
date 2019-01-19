using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Document;


namespace PrimeApps.Model.Repositories
{
    public class DocumentRepository : RepositoryBaseTenant, IDocumentRepository
    {
        private IConfiguration _configuration;

        public DocumentRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
            _configuration = configuration;
        }
        public async Task<Document> CreateAsync(Document document)
        {
            var newDocument = DbContext.Documents.Add(document);
            return await DbContext.SaveChangesAsync() > 0 ? newDocument.Entity : null;
        }
        public async Task<Document> UpdateAsync(Document document)
        {
            var dataDoc = DbContext.Documents.FirstOrDefault(r => r.Id == document.Id && r.ModuleId == document.ModuleId && r.RecordId == document.RecordId);
            dataDoc.Name = document.Name;
            dataDoc.Description = document.Description;
            var modified = await DbContext.SaveChangesAsync();
            return modified > 0 ? dataDoc : null;
        }
        /// <summary>
        /// Set record deleted property to TRUE
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Document> RemoveAsync(int id, bool hardDelete = false)
        {
            var dataDoc = DbContext.Documents.FirstOrDefault(r => r.Id == id && r.Deleted == false);
            if (dataDoc == null)
            {
                return null;
            }
            if (!hardDelete)
            {
                dataDoc.Deleted = true;
                var modified = await DbContext.SaveChangesAsync();
                return modified > 0 ? dataDoc : null;
            }
            else
            {
                var deletedRecord = DbContext.Documents.Remove(dataDoc);
                await DbContext.SaveChangesAsync();
                return deletedRecord.Entity;
            }

        }
        public async Task<Document> GetById(int id)
        {
            return await DbContext.Documents.FirstOrDefaultAsync(r => r.Id == id && r.Deleted == false);
        }
        public async Task<DocumentsResultSet> GetTopEntityDocuments(DocumentRequest request)
        {

            List<DocumentResult> documentResult = new List<DocumentResult>();

            //Get 5 record from the top of the document list ascending by the createdTime property. Filter the query by module and record id parameter.

            documentResult = await DbContext.Documents.Where(x => x.ModuleId == request.ModuleId && x.RecordId == request.RecordId && x.Deleted == false)
                .OrderByDescending(x => x.Id)
                .Take(5).Select(entity => new DocumentResult(_configuration)
                {
                    ID = entity.Id,
                    Name = entity.Name,
                    Type = entity.Type,
                    UniqueName = entity.UniqueName,
                    CreatedBy = entity.CreatedById,
                    CreatedTime = entity.CreatedAt,
                    Description = entity.Description,
                    FileSize = entity.FileSize,
                    ModuleId = entity.ModuleId,
                    RecordId = entity.RecordId,
                    ContainerId = request.ContainerId
                }).ToListAsync();

            return new DocumentsResultSet()
            {
                Documents = documentResult,
                FilteredDocumentsCount = documentResult.Count,
                TotalDocumentsCount = DbContext.Documents.Count(x => x.ModuleId == request.ModuleId && x.RecordId == request.RecordId && x.Deleted == false)
            };
        }
        /// <summary>
        /// Gets document results for the document explorer view on the client-side.
        /// </summary>
        /// <param name="InstanceID">Instance ID filter</param>
        /// <param name="EntityType">Entity Type Filter</param>
        /// <param name="EntityID">Entity ID Filter</param>
        /// <returns></returns>
        public async Task<DocumentsResultSet> GetDocuments(DocumentRequest request)
        {
            var documentResult = await DbContext.Documents.Where(x => x.ModuleId == request.ModuleId && x.RecordId == request.RecordId && x.Deleted == false)
                .OrderByDescending(x => x.Id)
                .Select(entity => new DocumentResult(_configuration)
                {
                    ID = entity.Id,
                    Name = entity.Name,
                    Type = entity.UniqueName,
                    UniqueName = entity.UniqueName,
                    CreatedBy = entity.CreatedById,
                    CreatedByName = entity.CreatedBy.FullName,
                    CreatedTime = entity.CreatedAt,
                    Description = entity.Description,
                    FileSize = entity.FileSize,
                    ModuleId = entity.ModuleId,
                    RecordId = entity.RecordId,
                    ContainerId = request.ContainerId
                }).ToListAsync();

            return new DocumentsResultSet()
            {
                Documents = documentResult,
                FilteredDocumentsCount = documentResult.Count,
                TotalDocumentsCount = documentResult.Count
            };

        }

        public async Task<ICollection<Document>> GetAll(int moduleId, int recordId)
        {
            var documents = await DbContext.Documents
                .Where(x => x.ModuleId == moduleId && x.RecordId == recordId && !x.Deleted)
                .ToListAsync();

            return documents;
        }

        public async Task<int> Update(Document document)
        {
            return await DbContext.SaveChangesAsync();
        }

        public bool IsFieldDocumentSearchable(int moduleId, string fieldName)
        {
            var documentField = DbContext.Fields.FirstOrDefault(x => x.ModuleId == moduleId && x.Name == fieldName);
            if (documentField != null)
            {
                return true;
            }
            return false;
        }

        public async Task<ICollection<DocumentResult>> GetDocumentsByLimit(DocumentFindRequest request)
        {
            var documents = DbContext.Documents.Where(x => x.ModuleId == request.ModuleId && x.RecordId == request.RecordId && x.Deleted == false)
                .OrderByDescending(x => x.CreatedAt)
                .Select(entity => new DocumentResult(_configuration)
                {
                    ID = entity.Id,
                    Name = entity.Name,
                    Type = entity.UniqueName,
                    UniqueName = entity.UniqueName,
                    CreatedBy = entity.CreatedById,
                    CreatedByName = entity.CreatedBy.FullName,
                    CreatedTime = entity.CreatedAt,
                    Description = entity.Description,
                    FileSize = entity.FileSize,
                    ModuleId = entity.ModuleId,
                    RecordId = entity.RecordId
                });

            documents = documents
                .Skip(request.Offset)
                .Take(request.Limit);

            return await documents.ToListAsync();
        }


        public async Task<int> Count(DocumentFindRequest request)
        {
            var totalCount = await GetDocumentQuery(request).CountAsync();

            return totalCount;
        }

        private IQueryable<Document> GetDocumentQuery(DocumentFindRequest request)
        {
            var documents = DbContext.Documents.Where(x => x.ModuleId == request.ModuleId && x.RecordId == request.RecordId && x.Deleted == false);

            return documents;
        }
    }
}
