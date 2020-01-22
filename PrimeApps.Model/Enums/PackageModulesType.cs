using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
	public enum PackageModulesType
	{

		[EnumMember(Value = "all_modules")]
		AllModules = 1,

		[EnumMember(Value = "selected_modules")]
		SelectedModules = 2,

		[EnumMember(Value = "dont_transfer")]
		DontTransfer = 3,

	}
}
