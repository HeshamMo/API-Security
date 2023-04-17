using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]

	public class testController:ControllerBase
	{
		[HttpGet("test")]
		public IActionResult Index()
		{
			return Ok("this is test api"); 
		}
	}
}
