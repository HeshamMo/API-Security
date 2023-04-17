using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi
{
	public class JWT
	{
		public string secret { get; set; }
		public string Issuer { get; set; }

		public string Audience { get; set; }

		public string DurationInDays { get; set; }

	}
}
