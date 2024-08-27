using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	#region properties
	public class AssociateContactDetail
	{
		public string? LineOne { get; set; }
		public string? LineTwo { get; set; }
		public string? CityName { get; set; }
		public object? CountryID { get; set; }
		public string? PostCode { get; set; }
		public string? PostCodeExtension { get; set; }
		public string? StateOrProvince { get; set; }
	}
	#endregion
}
