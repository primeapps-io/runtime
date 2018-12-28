using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
	public enum AppTemplateType
	{
		[EnumMember(Value = "email")]
		Email = 1,

		[EnumMember(Value = "document")]
		Document = 2
	}
}
