using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Models
{
	public class LogInModel
	{

		[Required]
		[MaxLength(50)]
		public string Email { get; set; }



		[Required]
		[MaxLength(50)]
		public string Passowrd { get; set; }

	}
}
