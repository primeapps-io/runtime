using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PrimeApps.Model.Common.Bpm
{
    public class BpmNotification
    {

        public int WorkflowId { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; }

        public string Message { get; set; }

        [MaxLength(4000)]
        public string Recipients { get; set; }

        [MaxLength(4000)]
        public JArray CC { get; set; }

        [MaxLength(4000)]
        public JArray Bcc { get; set; }

        public int? Schedule { get; set; }

        public virtual BpmWorkflow BpmWorkflow { get; set; }

        public string[] RecipientsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Recipients))
                    return null;

                return Recipients.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    Recipients = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }

        public ICollection<UserBasic> RecipientList { get; set; }


        public string[] CCArray
        {
            get
            {
                if (CC.IsNullOrEmpty())
                    return null;

                List<string> data=new List<string>();

                foreach (var item in CC)
                    data.Add(item["email"].Value<string>());

                return data.ToArray<string>();
            }
            set
            {
                if (value != null)
                {
                    JArray data = new JArray();
                    foreach (var cc in value)
                    {
                        data["email"] = cc;
                    }
                    CC = data;
                }
            }
        }

        public ICollection<UserBasic> CCList { get; set; }

        public string[] BccArray
        {
            get
            {
                if (Bcc.IsNullOrEmpty())
                    return null;

                List<string> data = new List<string>();

                foreach (var item in CC)
                    data.Add(item.Value<string>());

                return data.ToArray<string>();
            }
            set
            {
                if (value != null)
                {
                    JArray data = new JArray();
                    foreach (var cc in value)
                    {
                        data["email"] = cc;
                    }
                    Bcc = data;
                } 
            }
        }

        public ICollection<UserBasic> BccList { get; set; }
    }
}
