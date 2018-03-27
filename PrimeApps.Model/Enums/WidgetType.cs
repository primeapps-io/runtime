using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum WidgetType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "summary_count")]
        SummaryCount = 1, //Summary Count Widget (AVG ETC is used for rectangle widget for only 1 return value like % or number. Example : Total Company Joined This Year : 1719 or OUR sales AVERAGE for this year : 253.000 K etc.

        [EnumMember(Value = "report_table")]
        ReportTable = 2, //Report Table Widget is only TABLE report that contains table data // Not in use for now.

        [EnumMember(Value = "custom_widget")]
        CustomWidget = 3 //Loading from other html maybe. LoadUrl string is typed at Widget entity.
    }
}
