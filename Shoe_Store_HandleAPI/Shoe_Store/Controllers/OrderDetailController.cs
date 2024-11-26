using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net;
using Data.Models;
using Newtonsoft.Json;
using System.Text;

namespace Shoe_Store.Controllers
{
    public class OrderDetailController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public OrderDetailController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> UpdateOrderDetail(int id, int quantity)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);

            var payload = new
            {
                newQuantity = quantity 
            };

            var jsonconvert = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonconvert, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"https://localhost:7172/api/OrderDetailApi/{id}", content);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ListOrder", "User");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            return BadRequest(new { Message = errorResponse });
        }



        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["Shoe_Store_Cookie"]);
            var url = $"https://localhost:7172/api/OrderDetailApi/{id}";
            var respons = await client.DeleteAsync(url);
            if (respons.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Forbidden", "Error");
            }
            if (respons.IsSuccessStatusCode)
            {
                return RedirectToAction("ListOrder", "User");
            }
            TempData["ErrorMessage"] = "Error deleting OrderDetail: " + respons.ReasonPhrase;
            return View(respons);

        }



    }
}
