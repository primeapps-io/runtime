using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/note"), Authorize, SnakeCase]
    public class NoteController : BaseController
    {
        private INoteRepository _noteRepository;
        private IUserRepository _userRepository;
        private IRecordRepository _recordRepository;
        private IProfileRepository _profileRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;

        public NoteController(INoteRepository noteRepository, IUserRepository userRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _profileRepository = profileRepository;
            _picklistRepository = picklistRepository;
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            var noteEntity = await _noteRepository.GetById(id);

            if (noteEntity == null)
                return NotFound();

            if (noteEntity.CreatedBy.Picture != null && !noteEntity.CreatedBy.Picture.StartsWith("http://"))
                noteEntity.CreatedBy.Picture = Storage.GetAvatarUrl(noteEntity.CreatedBy.Picture);

            if (noteEntity.Likes.Count > 0)
            {
                foreach (var likedUser in noteEntity.Likes)
                {
                    if (likedUser.Picture != null && !likedUser.Picture.StartsWith("http://"))
                        likedUser.Picture = Storage.GetAvatarUrl(likedUser.Picture);
                }
            }
            

            return Ok(noteEntity);
        }

        [Route("find"), HttpPost]
        public async Task<ICollection<Note>> Find(NoteRequest request, [FromUri]string locale = "", [FromUri]int? timezoneOffset = 180)
        {
            var notes = await _noteRepository.Find(request);
            var moduleRecordIds = new Dictionary<string, List<int>>();

            foreach (var note in notes)
            {
                if (note.CreatedBy.Picture != null && !note.CreatedBy.Picture.StartsWith("http://"))
                    note.CreatedBy.Picture = Storage.GetAvatarUrl(note.CreatedBy.Picture);

                if(note.Likes.Count > 0)
                {
                    foreach (var likedUser in note.Likes)
                    {
                        if (likedUser.Picture != null && !likedUser.Picture.StartsWith("http://"))
                            likedUser.Picture = Storage.GetAvatarUrl(likedUser.Picture);
                    }
                }

                if (note.Notes.Count > 0)
                {
                    foreach (var subNote in note.Notes)
                    {
                        if (subNote.CreatedBy.Picture != null && !subNote.CreatedBy.Picture.StartsWith("http://"))
                            subNote.CreatedBy.Picture = Storage.GetAvatarUrl(subNote.CreatedBy.Picture);

                        if (subNote.Likes.Count > 0)
                        {
                            foreach (var subLikedUser in note.Likes)
                            {
                                if (subLikedUser.Picture != null && !subLikedUser.Picture.StartsWith("http://"))
                                    subLikedUser.Picture = Storage.GetAvatarUrl(subLikedUser.Picture);
                            }
                        }
                    }
                }

                if (note.RecordId.HasValue)
                {
                    if (!moduleRecordIds.ContainsKey(note.Module.Name))
                    {
                        moduleRecordIds.Add(note.Module.Name, new List<int> { note.RecordId.Value });
                    }
                    else
                    {
                        var moduleRecordId = moduleRecordIds.Single(x => x.Key == note.Module.Name);
                        moduleRecordId.Value.Add(note.RecordId.Value);
                    }
                }
            }

            if (request.RecordId.HasValue)
                return notes;

            var noteList = new List<Note>();
            var notesHasNotRecord = notes.Where(x => !x.RecordId.HasValue).ToList();
            var notesHasRecord = notes.Where(x => x.RecordId.HasValue).ToList();
            var moduleRecords = new Dictionary<string, JArray>();

            foreach (var moduleRecordId in moduleRecordIds)
            {
                var records = _recordRepository.GetAllById(moduleRecordId.Key, moduleRecordId.Value);
                moduleRecords.Add(moduleRecordId.Key, records);
            }

            var currentCulture = locale == "en" ? "en-US" : "tr-TR";

            foreach (var note in notesHasRecord)
            {
                var record = (JObject)moduleRecords[note.Module.Name].FirstOrDefault(x => (int)x["id"] == note.RecordId.Value);
                var profile = await _profileRepository.GetProfileById(AppUser.ProfileId);
                var hasPermission = false;

                if (AppUser.HasAdminProfile)
                    hasPermission = true;
                else
                {
                    foreach (var permission in profile.Permissions)
                    {
                        if (note.ModuleId == permission.ModuleId && permission.Read)
                            hasPermission = true;
                    }
                }
                

                if(!hasPermission)
                    continue;

                if (record.IsNullOrEmpty())
                    continue;

                var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(note.Module, record, _moduleRepository, _picklistRepository, AppUser.TenantLanguage, currentCulture, timezoneOffset.Value);

                if (!record.IsNullOrEmpty())
                {
                    var primaryField = note.Module.Fields.Single(x => x.Primary);
                    note.RecordPrimaryValue = (string)recordFormatted[primaryField.Name];
                }

                noteList.Add(note);
            }

            noteList.AddRange(notesHasNotRecord);

            noteList = noteList.OrderByDescending(x => x.CreatedAt).ToList();

            return noteList;
        }

        [Route("count"), HttpPost]
        public async Task<int> Count(NoteRequest request)
        {
            return await _noteRepository.Count(request);
        }

        [Route("create"), HttpPost]
        public async Task<IHttpActionResult> Create(NoteBindingModel note)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var noteEntity = await NoteHelper.CreateEntity(note, _userRepository);
            var result = await _noteRepository.Create(noteEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            noteEntity = await _noteRepository.GetById(noteEntity.Id);
            noteEntity.CreatedBy.Picture = Storage.GetAvatarUrl(noteEntity.CreatedBy.Picture);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/note/get/" + noteEntity.Id, noteEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IHttpActionResult> Update([FromUri]int id, [FromBody]NoteBindingModel note)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var noteEntity = await _noteRepository.GetByIdBasic(id);

            if (noteEntity == null)
                return NotFound();

            await NoteHelper.UpdateEntity(note, noteEntity, _userRepository);
            await _noteRepository.Update(noteEntity);

            return Ok(noteEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri]int id)
        {
            var noteEntity = await _noteRepository.GetByIdBasic(id);

            if (noteEntity == null)
                return NotFound();

            await _noteRepository.DeleteSoft(noteEntity);

            return Ok();
        }

        [Route("like"), HttpPost]
        public async Task<IHttpActionResult> Like(LikedNoteBindingModel note)

        {
            var noteEntity = await _noteRepository.GetByIdBasic(note.NoteId);

            await NoteHelper.UpdateLikedNote(noteEntity, note.UserId, _userRepository);
            await _noteRepository.Update(noteEntity);
            return Ok(noteEntity);
        }
    }
}
