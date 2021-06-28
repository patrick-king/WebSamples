using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCMoviesWithSSRS.Models;

namespace MVCMoviesWithSSRS.Controllers
{
    public class MoviePricesController : Controller
    {
        private readonly MvcMovieContext _context;

        public MoviePricesController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: MoviePrices
        public async Task<IActionResult> Index()
        {
            return View(await _context.MoviePrices.ToListAsync());
        }

        // GET: MoviePrices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moviePrice = await _context.MoviePrices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moviePrice == null)
            {
                return NotFound();
            }

            return View(moviePrice);
        }

        // GET: MoviePrices/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MoviePrices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,DateEntered,SellerId,Price")] MoviePrice moviePrice)
        {
            if (ModelState.IsValid)
            {
                _context.Add(moviePrice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(moviePrice);
        }

        // GET: MoviePrices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moviePrice = await _context.MoviePrices.FindAsync(id);
            if (moviePrice == null)
            {
                return NotFound();
            }
            return View(moviePrice);
        }

        // POST: MoviePrices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,DateEntered,SellerId,Price")] MoviePrice moviePrice)
        {
            if (id != moviePrice.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(moviePrice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoviePriceExists(moviePrice.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(moviePrice);
        }

        // GET: MoviePrices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moviePrice = await _context.MoviePrices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (moviePrice == null)
            {
                return NotFound();
            }

            return View(moviePrice);
        }

        // POST: MoviePrices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var moviePrice = await _context.MoviePrices.FindAsync(id);
            _context.MoviePrices.Remove(moviePrice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MoviePriceExists(int id)
        {
            return _context.MoviePrices.Any(e => e.Id == id);
        }
    }
}
