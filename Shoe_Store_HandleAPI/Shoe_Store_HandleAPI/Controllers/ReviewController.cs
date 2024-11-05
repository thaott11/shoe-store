using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        public readonly ModelDbContext _db;
        public ReviewController(ModelDbContext db)
        {
            _db = db;
        }

        //[HttpPost]
        //public async Task<ActionResult<Review>> CreateReview(Review review)
        //{
        //    try
        //    {

        //    }
        //}
    }
}
