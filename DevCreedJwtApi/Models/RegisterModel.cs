using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Models
{
	public class RegisterModel
	{
		[Required]
		[MaxLength(50)]
		public string FName { get; set; }

		[Required]
		[MaxLength(50)]
		public string LName { get; set; }


		[Required]
		[MaxLength(50)]
		public string UserName { get; set; }


		[Required]
		[MaxLength(50)]
		public string Email { get; set; }



		[Required]
		[MaxLength(50)]
		public string Passowrd { get; set; }


	}
}
