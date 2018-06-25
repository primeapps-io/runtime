using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public class ActionButtonEnum
    {
        public enum ActionType
        {
            Scripting = 1,
            Webhook = 2,
            ModalFrame = 3
        }
        public enum ActionTrigger
        {
            Detail = 1,
            Form = 2,
            All = 3
        }
        public enum WebhhookHttpMethod
        {
            [EnumMember(Value = "")]
            NotSet = 0,

            [EnumMember(Value = "post")]
            Post = 1,

            [EnumMember(Value = "get")]
            Get = 2
        }
    }
}
