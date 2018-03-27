using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ChartType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "column2d")]
        Column2D = 1,

        [EnumMember(Value = "column3d")]
        Column3D = 2,

        [EnumMember(Value = "line")]
        Line = 3,

        [EnumMember(Value = "area2d")]
        Area2D = 4,

        [EnumMember(Value = "bar2d")]
        Bar2D = 5,

        [EnumMember(Value = "bar3d")]
        Bar3D = 6,

        [EnumMember(Value = "pie2d")]
        Pie2D = 7,

        [EnumMember(Value = "pie3d")]
        Pie3D = 8,

        [EnumMember(Value = "doughnut2d")]
        Doughnut2D = 9,

        [EnumMember(Value = "doughnut3d")]
        Doughnut3D = 10,

        [EnumMember(Value = "scrollcolumn2d")]
        Scrollcolumn2D = 11,

        [EnumMember(Value = "scrollline2d")]
        Scrollline2D = 12,

        [EnumMember(Value = "scrollarea2d")]
        Scrollarea2D = 13,

        [EnumMember(Value = "funnel")]
        Funnel = 14,

        [EnumMember(Value = "pyramid")]
        Pyramid = 15
    }
}
