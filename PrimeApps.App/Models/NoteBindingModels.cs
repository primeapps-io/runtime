using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class NoteBindingModel
    {
        [Required, StringLength(4000)]
        public string Text { get; set; }

        public int? ModuleId { get; set; }

        public int? RecordId { get; set; }

        public int? NoteId { get; set; }

        public List<int> Likes { get; set; }

        public List<NoteBindingModel> Notes { get; set; }
    }

    public class LikedNoteBindingModel
    {
        [Required, Range(1, int.MaxValue)]
        public int NoteId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}