using Newtonsoft.Json;
using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	#region properties
	public class OrganizationJob
	{
		public string? JobCode { get; set; }
		public string? DepartmentCode { get; set; }
		public string? DepartmentDescription { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime StartDate { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime EndDate { get; set; }
		public bool? IsPrimaryJobFlag { get; set; }
		public bool? ActiveFlag { get; set; }
		public string? CreatedBy { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime CreatedTime { get; set; }
		public string? LastModifiedBy { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime LastModifiedTime { get; set; }
	}
	#endregion
}
