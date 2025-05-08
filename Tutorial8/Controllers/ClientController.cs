using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        // Get client's trips
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            var iClientExist = await _clientsService.DoesClientExist(id);
            try
            {
                if (iClientExist)
                {
                    var trips = await _clientsService.GetTripsByClientId(id);
                    return Ok(trips);
                }

                return StatusCode(404,"Client not found");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        // Create new client
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO clientCreateDTO)
        {
            // Check nullspaces
            if (string.IsNullOrWhiteSpace(clientCreateDTO.FirstName) ||
                string.IsNullOrWhiteSpace(clientCreateDTO.LastName) ||
                string.IsNullOrWhiteSpace(clientCreateDTO.Email) ||
                string.IsNullOrWhiteSpace(clientCreateDTO.Telephone) ||
                string.IsNullOrWhiteSpace(clientCreateDTO.Pesel))
            {
                return BadRequest("All fields are required.");
            }
            
            // email control with regex
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(clientCreateDTO.Email, emailPattern))
            {
                return BadRequest("Invalid email format.");
            }

            // is PESEL exist?
            if (await _clientsService.DoesPeselExist(clientCreateDTO.Pesel))
                return Conflict("Client with this PESEL already exists."); // 409
            
            if (clientCreateDTO.Pesel.Length != 11 || !clientCreateDTO.Pesel.All(char.IsDigit))
            {
                return BadRequest("PESEL must be 11 digits.");
            }

            
            var id = await _clientsService.CreateClientAsync(clientCreateDTO);
            return Created($"api/clients/{id}", null);
        }
        
        // Register client to a trip
        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
        {
            try
            {
                var success = await _clientsService.RegisterClientForTrip(id, tripId);
                if (!success)
                    return StatusCode(404, "Not found");

                return Ok("Client successfully registered");
            }
            catch (Exception e)
            {
                return StatusCode(409, e.Message);
            }
        }
        
        // Remove client from trip
        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId)
        {
            try
            {
                var success = await _clientsService.RemoveClientFromTrip(id, tripId);
                if (!success)
                    return NotFound("Registration not found.");

                return Ok("Client removed from trip successfully.");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}