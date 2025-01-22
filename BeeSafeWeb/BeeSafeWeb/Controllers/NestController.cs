using BeeSafeWeb.Utility.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BeeSafeWeb.Data;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;
using Microsoft.AspNetCore.Authorization;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class NestController : Controller
    {
        private readonly IRepository<NestEstimate> _nestRepository;

        public NestController(IRepository<NestEstimate> nestRepository)
        {
            _nestRepository = nestRepository;
        }

        public async Task<IActionResult> Index(bool? isDestroyed, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var nests = _nestRepository.GetQueryable();

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

            ViewBag.IsDestroyed = isDestroyed;

            return View(pagedList);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, bool isDestroyed)
        {
            var nest = await _nestRepository.GetByIdAsync(id);
            if (nest == null)
            {
                return NotFound();
            }

            nest.IsDestroyed = isDestroyed;
            await _nestRepository.UpdateAsync(nest);

            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> BatchUpdate(Dictionary<Guid, NestUpdateModel> nestUpdates)
        {
            if (nestUpdates != null && nestUpdates.Any())
            {
                foreach (var update in nestUpdates.Values)
                {
                    var nest = await _nestRepository.GetByIdAsync(update.Id);
                    if (nest != null)
                    {
                        nest.IsDestroyed = update.IsDestroyed;
                        await _nestRepository.UpdateAsync(nest);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
        
        public class NestUpdateModel
        {
            public Guid Id { get; set; }
            public bool IsDestroyed { get; set; }
        }

    }
}