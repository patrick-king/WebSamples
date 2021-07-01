using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVCMoviesWithSSRS.Models;
using MVCMoviesWithSSRS.ViewModel;

namespace MVCMoviesWithSSRS.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;
        private ILogger _logger;

        private IConfiguration _config;

        public MoviesController(MvcMovieContext context,
            ILogger<MoviesController> logger,
            IConfiguration config
            )
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }


        // GET: Movies/Create
        public IActionResult CreateMovieAndPrice()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovieAndPrice(ViewModel.MovieSellerPriceViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SaveWithStoredProcEFCore || model.SaveWithStoredProcADONET)
                {
                    await CreateMSPWithStoredProc(model);
                }
                else
                {
                    await CreateMSPWithEFEntities(model);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        /// <summary>
        /// Demonstrates how to use a stored proc from EF core.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<int> CreateMSPWithStoredProc(MovieSellerPriceViewModel model)
        {
            //Setup parameters
            var title = new SqlParameter("Title", System.Data.SqlDbType.NVarChar);
            title.Value = stringToDbString(model.Title);

            var releaseDate = new SqlParameter("ReleaseDate", System.Data.SqlDbType.DateTime2);
            releaseDate.Value = model.ReleaseDate;

            var genre = new SqlParameter("Genre", System.Data.SqlDbType.NVarChar);
            genre.Value = stringToDbString(model.Genre);

            var msrp = new SqlParameter("MSRP", System.Data.SqlDbType.Decimal);
            msrp.Value = model.MSRPPrice;

            var sellerName = new SqlParameter("SellerName", System.Data.SqlDbType.VarChar);
            sellerName.Value = stringToDbString(model.SellerName);

            var url = new SqlParameter("URL", System.Data.SqlDbType.NVarChar);
            url.Value = stringToDbString(model.URL);

            var address = new SqlParameter("Address", System.Data.SqlDbType.NVarChar);
            address.Value = stringToDbString(model.Address1);

            var city = new SqlParameter("City", System.Data.SqlDbType.NVarChar);
            city.Value = stringToDbString(model.City);

            var state = new SqlParameter("State", System.Data.SqlDbType.NVarChar);
            state.Value = stringToDbString(model.State);

            var zip = new SqlParameter("Zip", System.Data.SqlDbType.NVarChar);
            zip.Value = stringToDbString(model.Zip);

            var phone = new SqlParameter("Phone", System.Data.SqlDbType.NVarChar);
            phone.Value = stringToDbString(model.Phone);

            var sellerPrice = new SqlParameter("SellerPrice", System.Data.SqlDbType.Decimal);
            sellerPrice.Value = model.SellerPrice;

            if (model.SaveWithStoredProcEFCore)
            {
                var sqlparams = new object[] { title, releaseDate, genre, msrp, sellerName, url, address, city, state, zip, phone, sellerPrice };


                //Call stored procedure to save
                //Here we are using an EF Core convenience method. We could also do this with ADO.NET classes (SQLCommand, SQLConnection, SQLParameter) instead. 
                return await _context.Database.ExecuteSqlRawAsync("EXECUTE dbo.AddNewMovieFromPage @Title, @ReleaseDate, @Genre, @MSRP, @SellerName, @URL, @Address, @City, @State, @Zip, @Phone, @SellerPrice", sqlparams);
            }
            else if (model.SaveWithStoredProcADONET)
            {
                //Classic ADO.NET Logic
                SqlCommand cmd = new SqlCommand();
                SqlConnection cn = null;
                try
                {
                    cn = new SqlConnection(_config.GetConnectionString("MvcMovieContext"));
                    await cn.OpenAsync();
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.AddNewMovieFromPage";

                    cmd.Parameters.AddRange(new SqlParameter[] { title, releaseDate, genre, msrp, sellerName, url, address, city, state, zip, phone, sellerPrice });

                    return await cmd.ExecuteNonQueryAsync();

                }
                finally
                {

                    if (cmd != null)
                    {
                        cmd.Connection = null;
                        await cmd.DisposeAsync();
                    }
                    if (cn != null)
                    {
                        await cn.CloseAsync();
                        await cn.DisposeAsync();
                    }
                }
            }
            else
            {
                throw new ArgumentException("Called to use Stored Proc without a valid selection");
            }

        }

        /// <summary>
        /// Inserts dbnull.value for null strings
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private object stringToDbString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return DBNull.Value;
            }
            return input;
        }

        /// <summary>
        /// Demonstrates how to insert into multiple tables in a single transaction with EF Core linq syntax
        /// </summary>
        /// <remarks>This is the preferable way over a stored proc, since it keeps the logic in the application and works naturally with Entity Framework</remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<int> CreateMSPWithEFEntities(MovieSellerPriceViewModel model)
        {
            //Create movie
            var newMovie = new Movie()
            {
                Title = model.Title,
                ReleaseDate = model.ReleaseDate,
                Genre = model.Genre,
                Price = model.MSRPPrice
            };

            //Add seller and price if indicated
            if (!string.IsNullOrEmpty(model.SellerName))
            {

                var newSeller = new Seller()
                {
                    Address1 = model.Address1,
                    City = model.City,
                    Name = model.SellerName,
                    Phone = model.Phone,
                    State = model.State,
                    URL = model.URL,
                    Zip = model.Zip
                };

                var newSellerPrice = new MoviePrice()
                {
                    Price = model.SellerPrice,
                };

                //Seller Price has a seller, so add seller to that collection
                newSellerPrice.Seller = newSeller;

                //Movies has a Prices collection. Add sellerprice to movie. This creates a graph of Movie->SellerPrice->Seller that
                //EF can traverse to figure out the relationships and populate the Foreign keys during creation
                newMovie.Prices = new List<MoviePrice>();
                newMovie.Prices.Add(newSellerPrice);
            }
            _context.Movie.Add(newMovie);
            return await _context.SaveChangesAsync();

        }


        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            try
            {
                _context.Movie.Remove(movie);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //Logs the exception, plus your formatted message
                var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Error in {0} - Request Id {1}",
                    nameof(DeleteConfirmed), requestId);

                return View("Error", new ErrorViewModel { RequestId = requestId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }

        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                string[] cities = {"Avery", "Boston", "Canadaigua", "Dalton", "Eastwood", "Franck", "Gopherville", "Handley", "Interlachen", "Joesephine", "Kazaam",
            "Lincoln", "Manners", "Nopline", "Oscaloa", "Pinsal",
            "Quantine", "Rufus", "Seldom", "Tankeral", "Uranium",
            "Vesper", "Walden", "Vandolf", "Xuxa", "Yolo", "Zingping"};

                string[] firstNames = {"Action", "AAA", "Bing",
                "Cash", "Cheap", "Elite", "Best",
            "Great", "Good", "First", "Fresh"};

                string[] lastNames = { "Video", "Entertainment", "Vids", "Video Rentals", "Home Cinema", "DvD", "Movies", "Movie Rentals", "Zinema", "Flicks" };

                string[] movies1 = { "A new", "Best", "Candid", "Cute", "Dangerous", "Brightest", "Daring", "My Darling", "First",
            "Fresh"};

                string[] movies2 = { "Alligator", "Alley", "Kitty", "Puppy", " Barbeque Grill", "Night Watchman", "Meter Patrol", " Desert Land", "Castaways", "Pastry Chef" };



                //Ensure that we have basic sample data in database
                bool created = await _context.Database.EnsureCreatedAsync();

                var firstSeller = (from s in _context.Sellers
                                   where s.Id == 1
                                   select s).FirstOrDefault();

                const int SELLERCOUNT = 20;
                const int MOVIECOUNT = 30;

                int j;
                if (firstSeller == null)
                {

                    j = 1;
                    for (int i = 1; i <= SELLERCOUNT; i++)
                    {
                        string name = firstNames[i % 10] + " " + lastNames[j];
                        _context.Sellers.Add(new Seller()
                        {

                            Address1 = string.Format("{0} {1} Street", i, firstNames[i % 10]),
                            City = cities[i % 10],
                            State = "MA",
                            Name = name,
                            Phone = (5551212 + i).ToString(),
                            URL = "http://localhost/sellers/detail/" + i.ToString(),
                            Zip = (10000 + i).ToString()
                        });
                        if (i % 10 == 0)
                        {
                            j += 1;
                        }

                    }



                    j = 1;
                    //movies
                    for (int i = 1; i <= MOVIECOUNT; i++)
                    {
                        string name = $"{movies1[i % 10]} {movies2[i % j]}";
                        _context.Movie.Add(new Movie()
                        {

                            Genre = "Western",
                            Price = (decimal)(10.00 + ((double)i * .40)),
                            ReleaseDate = new DateTime(2020 - i, i % 11 + 1, i % 27 + 1),
                            Title = name
                        });

                        if (i % 10 == 0)
                        {
                            j += 1;
                        }
                    }

                    _context.SaveChanges();

                    int lastMovie = _context.Movie.OrderBy(t=>t.Id).Last().Id;
                    int lastSeller = _context.Sellers.OrderBy(s=>s.Id).Last().Id;


                    var rnd = new System.Random(23);


                    //movie price
                    for (int i = 0; i < 20; i++)
                    {
                        _context.MoviePrices.Add(new MoviePrice()
                        {
                            DateEntered = new DateTime((int)(50 * rnd.NextDouble() + 1970), (int)(11 * rnd.NextDouble() + 1), (int)(27 * rnd.NextDouble() + 1)),

                            MovieId = (int)(lastMovie - rnd.Next(MOVIECOUNT)),
                            SellerId = (int)(lastSeller - rnd.Next(SELLERCOUNT)),
                            Price = rnd.NextDouble() * 50
                        });
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            return View();
        }

    }
}
