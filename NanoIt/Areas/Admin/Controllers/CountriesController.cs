﻿using System;
using System.Linq;
using System.Web.Mvc;
using DevBootstrapper.Models.Context;
using DevBootstrapper.Models.POCO.IdentityCustomization;
using DevBootstrapper.Modules.Cache;

namespace DevBootstrapper.Areas.Admin.Controllers {
    public class CountriesController : Controller {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index() {
            return View(CachedQueriedData.GetCountries().ToList());
        }

        public ActionResult Edit(Int32 id) {
            var zones = CachedQueriedData.GetTimezones(id);
            ViewBag.Timezone = new SelectList(db.UserTimeZones.ToList(), "UserTimeZoneID", "Display");
            ViewBag.CountryID = id;
            ViewBag.CountryName = db.Countries.Find(id).DisplayCountryName + " - " + db.Countries.Find(id).Alpha2Code;
            return View(zones);
        }

        public ActionResult Delete(Int32 id) {
            var timezone = db.UserTimeZones.Find(id);
            db.UserTimeZones.Remove(timezone);
            db.SaveChanges();
            return RedirectToActionPermanent("Edit", new { id });
        }

        [HttpPost]
        public ActionResult Edit(int CountryID, int Timezone, bool hasMultiple) {
            var country = db.Countries.Find(CountryID);

            var foundTimeZone = db.UserTimeZones.Find(Timezone);
            if (foundTimeZone != null) {
                var addRelation = new CountryTimezoneRelation {
                    CountryID = country.CountryID,
                    UserTimeZoneID = foundTimeZone.UserTimeZoneID
                };
                var anyExist =
                    db.CountryTimezoneRelations.Any(
                        n => n.UserTimeZoneID == addRelation.UserTimeZoneID && n.CountryID == addRelation.CountryID);

                if (!anyExist) {
                    //not exist then add
                    db.CountryTimezoneRelations.Add(addRelation);
                    country.RelatedTimeZoneID = addRelation.UserTimeZoneID;
                }

                country.IsSingleTimeZone = !hasMultiple;
                country.RelatedTimeZoneID = addRelation.UserTimeZoneID;
                db.SaveChanges();
            }
            var zones = CachedQueriedData.GetTimezones(CountryID);
            ViewBag.Timezone = new SelectList(db.UserTimeZones.ToList(), "UserTimeZoneID", "Display");
            ViewBag.CountryID = CountryID;
            ViewBag.CountryName = db.Countries.Find(CountryID).DisplayCountryName + " - " +
                                  db.Countries.Find(CountryID).Alpha2Code;

            return View(zones);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}