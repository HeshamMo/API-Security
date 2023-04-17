using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Models
{
	[Owned]
	public class RefreshToken
	{
		public string Token { get; set; }
		public DateTime ExpiresOn { get; set; }
		public bool isExpired => DateTime.UtcNow >= ExpiresOn;
		public DateTime CreatedOn { get; set; }
		public DateTime? RevokenOn { get; set; }
		public bool IsActive => RevokenOn == null && !isExpired;
	}

}
