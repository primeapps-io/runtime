using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/note")/*, SnakeCase*/]
	[Authorize]

	public class NoteController : BaseController
    {
        private INoteRepository _noteRepository;
        private IUserRepository _userRepository;
        private IRecordRepository _recordRepository;
        private IProfileRepository _profileRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;
		private IHttpContextAccessor _httpContextAccessor;

		public NoteController(INoteRepository noteRepository, IUserRepository userRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository, IHttpContextAccessor httpContextAccessor)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _profileRepository = profileRepository;
            _picklistRepository = picklistRepository;
			_httpContextAccessor = httpContextAccessor;

			/*SetCurrentUser(_noteRepository, _httpContextAccessor);
            SetCurrentUser(_userRepository, _httpContextAccessor);
            SetCurrentUser(_recordRepository, _httpContextAccessor);
            SetCurrentUser(_moduleRepository, _httpContextAccessor);
            SetCurrentUser(_profileRepository, _httpContextAccessor);
            SetCurrentUser(_picklistRepository, _httpContextAccessor);*/
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var noteEntity = await _noteRepository.GetById(id);

            if (noteEntity == null)
                return NotFound();

            if (noteEntity.CreatedBy.Picture != null && !noteEntity.CreatedBy.Picture.StartsWith("http://"))
                noteEntity.CreatedBy.Picture = AzureStorage.GetAvatarUrl(noteEntity.CreatedBy.Picture);

            if (noteEntity.Likes.Count > 0)
            {
                foreach (var likedUser in noteEntity.Likes)
                {
                    if (likedUser.TenantUser.Picture != null && !likedUser.TenantUser.Picture.StartsWith("http://"))
                        likedUser.TenantUser.Picture = AzureStorage.GetAvatarUrl(likedUser.TenantUser.Picture);
                }
            }
            
            return Ok(noteEntity);
        }

        [Route("find"), HttpPost]
        public async Task<ICollection<Note>> Find(NoteRequest request, [FromRoute]string locale = "", [FromRoute]int? timezoneOffset = 180)
        {
            var notes = await _noteRepository.Find(request);
            var moduleRecordIds = new Dictionary<string, List<int>>();

            foreach (var note in notes)
            {
                if (note.CreatedBy.Picture != null && !note.CreatedBy.Picture.StartsWith("http://"))
                    note.CreatedBy.Picture = AzureStorage.GetAvatarUrl(note.CreatedBy.Picture);

                if (note.Likes.Count > 0)
                {
                    foreach (var likedUser in note.Likes)
                    {
                        if (likedUser.TenantUser.Picture != null && !likedUser.TenantUser.Picture.StartsWith("http://"))
                            likedUser.TenantUser.Picture = AzureStorage.GetAvatarUrl(likedUser.TenantUser.Picture);
                    }
                }

                if (note.Notes.Count > 0)
                {
                    foreach (var subNote in note.Notes)
                    {
                        if (subNote.CreatedBy.Picture != null && !subNote.CreatedBy.Picture.StartsWith("http://"))
                            subNote.CreatedBy.Picture = AzureStorage.GetAvatarUrl(subNote.CreatedBy.Picture);

                        if (subNote.Likes.Count > 0)
                        {
                            foreach (var subLikedUser in note.Likes)
                            {
                                if (subLikedUser.TenantUser.Picture != null && !subLikedUser.TenantUser.Picture.StartsWith("http://"))
                                    subLikedUser.TenantUser.Picture = AzureStorage.GetAvatarUrl(subLikedUser.TenantUser.Picture);
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


                if (!hasPermission)
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
        public async Task<IActionResult> Create(NoteBindingModel note)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var noteEntity = await NoteHelper.CreateEntity(note, _userRepository);
            var result = await _noteRepository.Create(noteEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            noteEntity = await _noteRepository.GetById(noteEntity.Id);
            noteEntity.CreatedBy.Picture = AzureStorage.GetAvatarUrl(noteEntity.CreatedBy.Picture);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/note/get/" + noteEntity.Id, noteEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/view/get/" + workflowEntity.Id, workflowEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]NoteBindingModel note)
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
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var noteEntity = await _noteRepository.GetByIdBasic(id);

            if (noteEntity == null)
                return NotFound();

            await _noteRepository.DeleteSoft(noteEntity);

            return Ok();
        }

        [Route("like"), HttpPost]
        public async Task<IActionResult> Like(LikedNoteBindingModel note)

        {
            var noteEntity = await _noteRepository.GetByIdBasic(note.NoteId);

            await NoteHelper.UpdateLikedNote(noteEntity, note.UserId, _userRepository);
            await _noteRepository.Update(noteEntity);
            return Ok(noteEntity);
        }
    }
}
