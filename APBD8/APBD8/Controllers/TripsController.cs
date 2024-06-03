using APBD8.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using APBD8.Models;




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
    public async Task<IActionResult> GetTrips(int page = 1, int pageSize = 10)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

        var trips = await _context.Trips
            .OrderByDescending(e => e.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                Name = e.Name,
                Description = e.Description,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                MaxPeople = e.MaxPeople,
                Countries = e.IdCountries.Select(c => new
                {
                    Name = c.Name
                }),
                /*Clients = e.ClientTrips.Select(c => new
                {
                    ClientNames = e.ClientNames                 
                })*/
            }).ToListAsync();
        var clients = _context.Clients.Select(c => new
        {
            FirstName = c.FirstName,
            LastName = c.LastName
        }).ToListAsync();
        
        var response = new
        {
            pageNum = page,
            pageSize = pageSize,
            allPages = totalPages,
            trips = trips,
            clients = clients
        };

        return Ok(response);
    }
   
    [HttpPost("/api/trips/{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] Client clientDto)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        if (trip == null)
        {
            return NotFound(new { message = "Wycieczka nie została znaleziona." });
        }
        if (trip.DateFrom <= DateTime.Now)
        {
            return BadRequest(new { message = "Nie można zapisać się na wycieczkę, która już się odbyła." });
        }
        
        var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientDto.Pesel);
        if (existingClient != null)
        {
            return BadRequest(new { message = "Klient o podanym numerze PESEL już istnieje." });
        }
        
        var isClientAlreadyAssigned = await _context.ClientTrips
        .Include(ct => ct.IdTripNavigation)
        .Include(ct => ct.IdClientNavigation)
        .AnyAsync(ct => ct.IdTrip == idTrip && ct.IdClientNavigation.Pesel == clientDto.Pesel);
        if (isClientAlreadyAssigned)
        {
            return BadRequest(new { message = "Klient jest już zapisany na tę wycieczkę." });
        }
        
            var newClient = new Client
            {
            FirstName = clientDto.FirstName,
            LastName = clientDto.LastName,
            Email = clientDto.Email,
            Telephone = clientDto.Telephone,
            Pesel = clientDto.Pesel
            };

        var clientTrip = new ClientTrip
        {
            IdClientNavigation = newClient,
            IdTripNavigation = trip,
            RegisteredAt = DateTime.Now,
            /*PaymentDate = paymentDate*/
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClient), new { id = newClient.IdClient }, newClient);

        }
    [HttpGet("/api/clients/{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
         var client = await _context.Clients.FindAsync(id);

        if (client == null)
        {
             return NotFound();
        }

        return Ok(client);
    }
}