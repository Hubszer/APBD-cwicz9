using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using zad7.Data;

namespace zad7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripController(ApbdContext context)
    {
        _context = context;
    }
    
    
    
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _context.Trips.Select(e=>new
        {
            Name = e.Name,
            Description = e.Description,
            DateFrom = e.DateFrom,
            DateTo = e.DateTo,
            MaxPeople = e.MaxPeople,
            Countries = e.IdCountries.Select(e => new
            {
                Name = e.Name
            
        })
        }).ToListAsync();
        
        return Ok(trips);
    }

    [HttpDelete("/api/clients/{idClient}")]
    public async Task<IActionResult> DeleteClient()
    {
        return Ok();
    }

    [HttpGet("/api/trips/{idTrip}/clients")]
    public async Task<IActionResult> SignClient()
    {
        return Ok();
    }
}