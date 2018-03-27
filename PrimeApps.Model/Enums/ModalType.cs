using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ModalType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "modal")]
        Modal = 1,

        [EnumMember(Value = "side_modal")]
        SideModal = 2        
    }
}
