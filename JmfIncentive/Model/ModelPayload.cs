using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	public class ModelPayload
	{
		public string? DistCd { get; set; }
		public Model? Model { get; set; }
		public int Year { get; set; }
		public string? SeriesCode { get; set; }
		public string? MarketingSeries { get; set; }
		public string? BodyStyleDesc { get; set; }
		public int Seating { get; set; }
		public string? Brand { get; set; }
		public string? Grade { get; set; }
		public bool ModelYearCurrentFlag { get; set; }
		public Engine Engine { get; set; }
		public Transmission Transmission { get; set; }
		public int DPH { get; set; }
	}
}
