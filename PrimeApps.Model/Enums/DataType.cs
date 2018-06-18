using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum DataType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "text_single")]
        TextSingle = 1,

        [EnumMember(Value = "text_multi")]
        TextMulti = 2,

        [EnumMember(Value = "number")]
        Number = 3,

        [EnumMember(Value = "number_auto")]
        NumberAuto = 4,

        [EnumMember(Value = "number_decimal")]
        NumberDecimal = 5,

        [EnumMember(Value = "currency")]
        Currency = 6,

        [EnumMember(Value = "date")]
        Date = 7,

        [EnumMember(Value = "date_time")]
        DateTime = 8,

        [EnumMember(Value = "time")]
        Time = 9,

        [EnumMember(Value = "email")]
        Email = 10,

        [EnumMember(Value = "picklist")]
        Picklist = 11,

        [EnumMember(Value = "multiselect")]
        Multiselect = 12,

        [EnumMember(Value = "lookup")]
        Lookup = 13,

        [EnumMember(Value = "checkbox")]
        Checkbox = 14,

        [EnumMember(Value = "document")]
        Document = 15,

        [EnumMember(Value = "url")]
        Url = 16,

        [EnumMember(Value = "location")]
        Location = 17,

        [EnumMember(Value = "image")]
        Image = 18,

        [EnumMember(Value = "rating")]
        Rating = 19,

        [EnumMember(Value = "tag")]
        Tag = 20,

    }
}
