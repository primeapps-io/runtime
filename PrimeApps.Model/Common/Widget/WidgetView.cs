using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Widget
{
    public class WidgetView
    {
        public int Id { get; set; }
        public WidgetType WidgetType { get; set; }

        public string Name { get; set; }

        //For external widget for future usage
        public string LoadUrl { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }

        //Json data that widget carries on.
        public dynamic WidgetData { get; set; }

        public int ? ViewId { get; set; }
        public int ? ReportId { get; set; }
    }
}
