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

        public IActionResult Index(int? page)
        {
            int pageSize = 5; // Number of items per page
            int pageNumber = page ?? 1; // Default to first page

            var nests = _context.NestEstimates
                .Select(n => new NestEstimate
                {
                    Id = n.Id,
                    EstimatedLatitude = n.EstimatedLatitude,
                    EstimatedLongitude = n.EstimatedLongitude,
                    AccuracyLevel = n.AccuracyLevel,
                    IsDestroyed = n.IsDestroyed
                })
                .OrderBy(n => n.Id)
                .ToPagedList(pageNumber, pageSize);

            return View(nests);
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