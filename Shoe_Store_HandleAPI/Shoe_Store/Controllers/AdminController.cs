using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using Data.Models;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net;
using System.Security.AccessControl;

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
            var token = Request.Cookies["Shoe_Store_Cookie"];
            var userTypeCookie = Request.Cookies["UserType"];
            if (userTypeCookie != "Admin")
            {
                return RedirectToAction("Forbidden", "Error"); 
            }
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }

        public async Task<IActionResult> User()
        {
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            var client = _client.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);

            var url = "https://localhost:7172/api/ClientProfile/Admin";
            var respons = await client.GetAsync(url);
            if (respons.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
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
            var token = Request.Cookies["Shoe_Store_Cookie"];
            if (token == null)
            {
                return RedirectToAction("Login", "AuthMiddleware");
            }
            return View();
        }
    }
}