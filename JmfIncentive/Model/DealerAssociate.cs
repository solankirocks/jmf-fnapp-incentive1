using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Model
{
	#region properties
	public class DealerAssociate
	{
		[JsonProperty("_id")]
		public string? Id { get; set; }
		public Audit? Audit { get; set; }
		public Payload? Payload { get; set; }
	}
	#endregion
}
