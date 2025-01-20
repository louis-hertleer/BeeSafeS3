using BeeSafeWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BeeSafeWeb.Data;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace BeeSafeWeb.Controllers
{
    public class NestController : Controller
    {
        private readonly BeeSafeContext _context;

        public NestController(BeeSafeContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchLocation, bool? isDestroyed, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var nests = _context.NestEstimates.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchLocation))
            {
                nests = nests.Where(n => n.EstimatedLatitude.ToString().Contains(searchLocation) || 
                                         n.EstimatedLongitude.ToString().Contains(searchLocation));
            }

            if (isDestroyed.HasValue)
            {
                nests = nests.Where(n => n.IsDestroyed == isDestroyed.Value);
            }

            var pagedList = nests
                .OrderBy(n => n.Id)
                .Select(n => new NestEstimate
                {
                    Id = n.Id,
                    EstimatedLatitude = n.EstimatedLatitude,
                    EstimatedLongitude = n.EstimatedLongitude,
                    AccuracyLevel = n.AccuracyLevel,
                    IsDestroyed = n.IsDestroyed
                })
                .ToPagedList(pageNumber, pageSize);

            ViewBag.SearchLocation = searchLocation;
            ViewBag.IsDestroyed = isDestroyed;

            return View(pagedList);
        }


        [HttpPost]
        public IActionResult UpdateStatus(Guid id, bool isDestroyed)
        {
            var nest = _context.NestEstimates.Find(id);
            if (nest == null)
            {
                return NotFound();
            }

            nest.IsDestroyed = isDestroyed;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}