using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MVCMoviesWithSSRS.Models;
using MVCMoviesWithSSRS.Utility;
using SSRS2005ExecSvc;

namespace MVCMoviesWithSSRS.Controllers
{
    public class SellersController : Controller
    {
        private readonly MvcMovieContext _context;
        private readonly IConfiguration _config;
        public SellersController(MvcMovieContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Sellers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sellers.ToListAsync());
        }

        // GET: Sellers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seller = await _context.Sellers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seller == null)
            {
                return NotFound();
            }

            return View(seller);
        }

        // GET: Sellers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sellers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,URL,Address1,City,State,Zip,Phone")] Seller seller)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seller);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(seller);
        }

        // GET: Sellers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seller = await _context.Sellers.FindAsync(id);
            if (seller == null)
            {
                return NotFound();
            }
            return View(seller);
        }

        // POST: Sellers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,URL,Address1,City,State,Zip,Phone")] Seller seller)
        {
            if (id != seller.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seller);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SellerExists(seller.Id))
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
            return View(seller);
        }

        // GET: Sellers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seller = await _context.Sellers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (seller == null)
            {
                return NotFound();
            }

            return View(seller);
        }

        // POST: Sellers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seller = await _context.Sellers.FindAsync(id);
            _context.Sellers.Remove(seller);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SellerExists(int id)
        {
            return _context.Sellers.Any(e => e.Id == id);
        }

        public enum ExecutionMethod
        {
            ADONET = 0,
            EFCore = 1
        }

        public async Task<IActionResult> DuplicateSellersReport(int sellerId, ExecutionMethod method)
        {
            Seller[] model = null;
            //Show use of ADO to get the seller
            if (method == ExecutionMethod.ADONET)
            {
                model = await GetDuplicateSellersADO(sellerId);
            }
            else
            {
                model = await GetDuplicateSellersEF(sellerId);
            }

            return View(model);
        }

        /// <summary>
        /// Call a stored proc that returns records, using ADO.NET
        /// </summary>
        /// <param name="sellerId"></param>
        /// <returns></returns>
        private async Task<Seller[]> GetDuplicateSellersADO(int sellerId)
        {
            var result = new List<Seller>();
            //Using ADO.NET 
            SqlCommand cmd = new SqlCommand();
            SqlConnection cn = null;
            try
            {
                cn = new SqlConnection(_config.GetConnectionString("MvcMovieContext"));
                await cn.OpenAsync();
                cmd.Connection = cn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "dbo.GetDuplicatesForSeller";

                SqlParameter pSellerId = new SqlParameter("@sellerId", sellerId);
                cmd.Parameters.Add(pSellerId);

                // Capure returned records
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var s = new Seller()
                        {
                            Address1 = TypeConversion.ToAString(dr["Address1"]),
                            City = TypeConversion.ToAString(dr["City"]),
                            Id = TypeConversion.ToInt32(dr["Id"]),
                            Name = TypeConversion.ToAString(dr["Name"]),
                            Phone = TypeConversion.ToAString(dr["Phone"]),
                            State = TypeConversion.ToAString(dr["State"]),
                            URL = TypeConversion.ToAString(dr["Url"]),
                            Zip = TypeConversion.ToAString(dr["Zip"]),
                        };
                        result.Add(s);
                    }
                }

                return result.ToArray();

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

        /// <summary>
        /// Using EF to query a stored proc that returns a list of records from a table, where there is already a model representing that table
        /// </summary>
        /// <param name="sellerId"></param>
        /// <returns></returns>
        private async Task<Seller[]> GetDuplicateSellersEF(int sellerId)
        {
            Seller[] result;

            SqlParameter pSellerId = new SqlParameter("@sellerId", sellerId);
            _context.Database.ExecuteSqlRaw("EXEC dbo.GetDuplicatesForSeller @sellerId");
            result = await _context.Sellers.FromSqlRaw("EXEC dbo.GetDuplicatesForSeller @sellerId", new object[] { pSellerId }).ToArrayAsync();

            return result.ToArray();
        }


        //Stored procedures in SQL Server can be used to return recordsets. However, it is not possible to use EF to query a procedure that returns outputs
        //that do not match a table entity, so you have to define an entity and mark that entity as having a fake key to get around this rule for generating from scaffolding.
        //you can then remove the [key()] attribute after scaffolding is complete.
        public IActionResult SellerMoviesReport(int sellerId)
        {
            if (sellerId == 0)
            {
                //Query a db table-valued function from Entity Framework
                var models = _context.SellerMoviesReport.FromSqlRaw("Select * from dbo.GetSellerMoviesReport()");

                return View(models);
            }
            else
            {
                //Query a stored procedure that returns a recordset from Entity Framework
                SqlParameter pSellerId = new SqlParameter("sellerid", sellerId);
                var sqlParams = new object[] { pSellerId };
                var models = _context.SellerMoviesReport.FromSqlRaw("exec SellerMoviesReportProc @sellerid", sqlParams);
                
                ViewBag.SellerId = sellerId;

                return View(models);
            }

        }


        private SSRS2005ExecSvc.ReportExecutionServiceSoapClient getSSRS2005ExecClient()
        {
            var ssrsSettings = _config.GetSection("SSRS");
            
            //For http urls. If using https, use BasicHttpSecurityMode.Transport
            BasicHttpBinding ssrsBinding = new System.ServiceModel.BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            //sizing of message settings to accomodate large report files.
            int maxReportSize = Convert.ToInt32(ssrsSettings["MaxReportBytes"]);
            ssrsBinding.MaxReceivedMessageSize = maxReportSize;
            ssrsBinding.MaxBufferSize = maxReportSize;
            ssrsBinding.MaxBufferPoolSize = Int32.MaxValue;
            ssrsBinding.ReaderQuotas.MaxDepth = Int32.MaxValue;
            ssrsBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            ssrsBinding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            ssrsBinding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            ssrsBinding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;


            //Build URL of report
           
            string address = string.Format("{0}/{1}", ssrsSettings["ServerURL"].TrimEnd('/'), "ReportExecution2005.asmx");

            EndpointAddress ssrsAddress = new EndpointAddress(address);

            SSRS2005ExecSvc.ReportExecutionServiceSoapClient ssrsClient = new SSRS2005ExecSvc.ReportExecutionServiceSoapClient(ssrsBinding, ssrsAddress);

            ssrsClient.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            ssrsClient.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;

            return ssrsClient;
        }

        public async Task<IActionResult> MovieSellersReport(int movieId)
        {

            var reportName = "MovieSellers";
            //Render a PDF of the sellers for the given movie

            //
            //Configure Report Proxy
            //
            var ssrsClient = getSSRS2005ExecClient();
            try
            {
                await ssrsClient.OpenAsync();
                var ssrsSettings = _config.GetSection("SSRS");

                //
                //Load report
                //
                var loadRequest = new LoadReportRequest();
                loadRequest.Report = string.Format("/{0}/{1}", ssrsSettings["ReportsFolder"].Trim('/'), reportName);
                var loadResponse = await ssrsClient.LoadReportAsync(loadRequest);

                //
                //Setup Parameters
                //
                // this report has one parameter, the MovieId
                var reportParams = new ParameterValue[] { new ParameterValue() { Name = "MovieId", Value = movieId.ToString() } };
                //Note, if you have a parameter that is optional, still include it but set its value to null
                var ssrsParamRequest = new SetExecutionParametersRequest();
                ssrsParamRequest.ExecutionHeader = loadResponse.ExecutionHeader;
                ssrsParamRequest.Parameters = reportParams;
                ssrsParamRequest.ParameterLanguage = "en-us";
                var setParamResponse = await ssrsClient.SetExecutionParametersAsync(ssrsParamRequest);

                //
                //Render Report
                //
                var renderRequest = new RenderRequest(loadResponse.ExecutionHeader, null, "PDF", null);
                var renderResponse = await ssrsClient.RenderAsync(renderRequest);
                var returnedReport = renderResponse.Result;
                await ssrsClient.CloseAsync();

                //Send report to user
                if (returnedReport != null)
                {
                    //return report to client. Browser handler will show (in edge, this will show automatically in browser) OR, if no browser handler for PDF, report will show up as a download.
                    return new FileContentResult(returnedReport, "application/pdf");
                }
                else
                {
                    //Error rendering the report.
                    ViewBag.ErrorMessage = "The report failed to render. Check the report server.";
                }
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;

            }
            finally
            {
               if (ssrsClient.State != CommunicationState.Closed)
                {
                    ssrsClient.Abort();
                }
            }

            return RedirectToAction("ErrorUserFacing", "Home", new { errorMessage = ViewBag.ErrorMessage });
        }
    }
}
