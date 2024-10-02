using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using Data.Models;
using System.Net.Http.Headers;

namespace Shoe_Store.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public new IActionResult User()
        {
            return View();
        }

        


        public IActionResult Order()
        {
            return View();
        }

        
    }
}