 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitiesManager.Infrastructure.DatabaseContext;
using CitiesManager.Core.Entities;
using Microsoft.AspNetCore.Cors;

namespace CitiesManager.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    //[EnableCors("4100Client")] //It doesn't apply default policy | It accepts only Get requests as defined in custom CORS policy
    public class CitiesController : CustomControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities

        // GET: api/Cities/5
        /// <summary>
        /// To get list of cities (only city name) from 'cities' table
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet]
        //[Produces("application/xml")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            return await _context.Cities.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetCity(Guid id)
        {

            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return Problem(detail: "Invalid CityID", statusCode: 400, title: "City Search");
            }

            return city; //Automaticall will be converted as JSON format
        }

        // PUT: api/Cities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{cityID}")]
        public async Task<IActionResult> PutCity(Guid cityID, [Bind(nameof(City.CityID), nameof(City.CityName))] City city)
        {
            if (cityID != city.CityID)
            {
                return BadRequest();
            }

            //_context.Entry(city).State = EntityState.Modified; //Updates all values and should be carefully executed

            var existingCity = await _context.Cities.FindAsync(cityID);

            if (existingCity == null)
            {
                return NotFound(); //HTTP 404
            }

            existingCity.CityName = city.CityName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(cityID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Cities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<City>> PostCity([Bind(nameof(City.CityID), nameof(City.CityName))] City city)
            {
            //Internally this code is created by AspNetCore

            //if (ModelState.IsValid == false)
            //{
            //    return ValidationProblem(ModelState);
            //}


            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { cityID = city.CityID }, city); //Creates a json object includes newly created object 
        }

        // DELETE: api/Cities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(Guid id)
        {

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent(); //HTTP 200
        }

        private bool CityExists(Guid id)
        {
            return (_context.Cities?.Any(e => e.CityID == id)).GetValueOrDefault();
        }
    }
}
