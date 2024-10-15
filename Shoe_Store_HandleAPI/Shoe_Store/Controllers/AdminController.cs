using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using Data.Models;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shoe_Store.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _client;
        private int? page;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory;
        }

        public IActionResult Dashboard()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }

        public async Task<IActionResult> User()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }

            var client = _client.CreateClient();
            var url = "https://localhost:7172/api/ClientProfile";
            var respons = await client.GetAsync(url);
            if (respons.IsSuccessStatusCode)
            {
                var jsonstring = await respons.Content.ReadAsStringAsync();
                var category = JsonConvert.DeserializeObject<List<Client>>(jsonstring);
                int pagesize = 5;
                int pagecount = (page ?? 1);
                var pagelist = category.ToPagedList(pagecount, pagesize);
                return View(pagelist);
            }

            return View(new List<Client>().ToPagedList(1, 5));
        }

        


        public IActionResult Order()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }
    }
}