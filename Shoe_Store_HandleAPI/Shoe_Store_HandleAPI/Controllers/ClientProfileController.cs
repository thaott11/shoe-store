﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Shoe_Store_HandleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientProfileController : ControllerBase
    {
        private readonly ModelDbContext _db;

        public ClientProfileController(ModelDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var client = await _db.Clients.ToListAsync();
            return Ok(client);
        }

        [HttpGet("Admin")]
        [Authorize(Policy = "AdminPolicy")]

        public async Task<IActionResult> GetAllAdmin()
        {
            var client = await _db.Clients.ToListAsync();
            return Ok(client);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Client>> UpdateClient(int id, [FromBody] Client client)
        {
            try
            {
                var updateClient = await _db.Clients.FindAsync(id);
                if (updateClient == null) return NotFound("Client not found");

                updateClient.Name = client.Name;
                updateClient.Address = client.Address;
                updateClient.Phone = client.Phone;
                updateClient.Email = client.Email;
                updateClient.Password = client.Password;
                updateClient.status = client.status;
                _db.Clients.Update(updateClient);
                await _db.SaveChangesAsync();
                return Ok(updateClient); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var client = await _db.Clients.FindAsync(id);
                if (client == null) return NotFound("Client not found");
                _db.Clients.Remove(client);
                await _db.SaveChangesAsync();
                return NoContent(); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
    }
}
