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
	public class Payload
	{
		public string? DealerCodeID { get; set; }
		public string? DealerName { get; set; }
		public string? Region { get; set; }
		public string? RegionName { get; set; }
		public string? SpinID { get; set; }
		public string? UID { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? MiddleName { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime StartDate { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime EndDate { get; set; }
		public string? EmailID { get; set; }
		public string? WorkPhoneNumber { get; set; }
		public string? WorkPhoneExtensionNumber { get; set; }
		public string? CellPhoneNumber { get; set; }
		public bool TerminationFlag { get; set; }
		public string? DMSID { get; set; }
		public string? ManufacturerID { get; set; }
		public string? CreatedBy { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime CreatedTime { get; set; }
		public string? LastModifiedBy { get; set; }
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime LastModifiedTime { get; set; }
		public bool IsPrimaryRelationshipFlag { get; set; }
		public AssociateContactDetail? AssociateContactDetail { get; set; }
		public List<OrganizationJob>? OrganizationJobs { get; set; }
	}
	#endregion
}
