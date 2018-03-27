using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Application
{
    [Table("user_custom_shares")]
    public class UserCustomShare : BaseEntity
    {
        public UserCustomShare()
        {
            _usersList = new List<string>();
            _userGroupsList = new List<string>();
            _moduleList = new List<string>();
        }
        
        [Column("user_id"), ForeignKey("User")]
        public int UserId { get; set; }

        [Column("shared_user_id"), ForeignKey("SharedUser")]
        public int SharedUserId { get; set; }

        [Column("users")]
        public string Users
        {
            get
            {
                return string.Join(",", _usersList);
            }
            set
            {
                _usersList = value?.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        [Column("user_groups")]
        public string UserGroups
        {
            get
            {
                return string.Join(",", _userGroupsList);
            }
            set
            {
                _userGroupsList = value?.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        [Column("modules")]
        public string Modules
        {
            get
            {
                return string.Join(",", _moduleList);
            }
            set
            {
                _moduleList = value?.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        [NotMapped]
        private List<string> _usersList { get; set; }

        [NotMapped]
        private List<string> _userGroupsList { get; set; }

        [NotMapped]
        private List<string> _moduleList { get; set; }

        [NotMapped]
        public List<string> UsersList
        {
            get
            {
                return _usersList;
            }
            set
            {
                _usersList = value;
            }
        }

        [NotMapped]
        public List<string> UserGroupsList
        {
            get
            {
                return _userGroupsList;
            }
            set
            {
                _userGroupsList = value;
            }
        }

        [NotMapped]
        public List<string> ModuleList
        {
            get
            {
                return _moduleList;
            }
            set
            {
                _moduleList = value;
            }
        }

        public virtual TenantUser User { get; set; }

        public virtual TenantUser SharedUser { get; set; }
    }
}
