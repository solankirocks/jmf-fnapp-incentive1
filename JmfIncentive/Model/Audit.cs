using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	# region properties
	public class Audit
	{
		[JsonProperty("_audit_entity_type")]
		public string? AuditEntityType { get; set; }
		[JsonProperty("_audit_entity_id_definition")]
		public string? AuditEntityIdDefinition { get; set; }
		[JsonProperty("_audit_context")]
		public string? AuditContext { get; set; }
		[JsonProperty("_audit_schema_version")]
		public int AuditSchemaVersion { get; set; }
		[JsonProperty("_audit_active_flag")]
		public bool AuditActiveFlag { get; set; }
		[JsonProperty("_audit_archive_flag")]
		public bool AuditArchiveFlag { get; set; }
		[JsonProperty("_audit_created_by")]
		public string? AuditCreatedBy { get; set; }
		[JsonProperty("_audit_updated_by")]
		public string? AuditUpdatedBy { get; set; }
		[JsonProperty("_audit_created_datetime")]
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime AuditCreatedDatetime { get; set; }
		[JsonProperty("_audit_updated_datetime")]
		[JsonConverter(typeof(IsoDateConverter))]
		public DateTime AuditUpdatedDatetime { get; set; }
	}
	#endregion
}
