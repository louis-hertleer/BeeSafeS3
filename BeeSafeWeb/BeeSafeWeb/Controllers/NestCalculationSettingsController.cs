using BeeSafeWeb.Models;
using BeeSafeWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeSafeWeb.Controllers
{
    [Authorize]
    public class NestCalculationSettingsController : Controller
    {
        /// <summary>
        /// Displays the current nest calculation settings.
        /// </summary>
        public IActionResult Index()
        {
            var model = new NestCalculationSettingsViewModel
            {
                HornetSpeed = NestCalculationSettings.HornetSpeed,
                CorrectionFactor = NestCalculationSettings.CorrectionFactor,
                GeoThreshold = NestCalculationSettings.GeoThreshold,
                DirectionBucketSize = NestCalculationSettings.DirectionBucketSize,
                DirectionThreshold = NestCalculationSettings.DirectionThreshold,
                OverlapThreshold = NestCalculationSettings.OverlapThreshold
            };
            return View(model);
        }

        /// <summary>
        /// Updates the nest calculation settings.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(NestCalculationSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If the model state is invalid, re-display the page.
                return View("Index", model);
            }

            // Update the static settings with the new values.
            NestCalculationSettings.HornetSpeed = model.HornetSpeed;
            NestCalculationSettings.CorrectionFactor = model.CorrectionFactor;
            NestCalculationSettings.GeoThreshold = model.GeoThreshold;
            NestCalculationSettings.DirectionBucketSize = model.DirectionBucketSize;
            NestCalculationSettings.DirectionThreshold = model.DirectionThreshold;
            NestCalculationSettings.OverlapThreshold = model.OverlapThreshold;

            TempData["SuccessMessage"] = "Calculation settings updated successfully!";
            return RedirectToAction("Index");
        }
    }
}
