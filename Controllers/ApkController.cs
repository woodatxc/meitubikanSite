using meitubikanSite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace meitubikanSite.Controllers
{
    public class ApkController : Controller
    {
        private static ApkModel ApkModel = new ApkModel();

        public JsonResult GetTotalDownload()
        {
            string source = string.IsNullOrWhiteSpace(Request["form"]) ? string.Empty : Request["form"];
            source = source.ToLower().Trim();

            Hashtable data = new Hashtable();
            if (!string.IsNullOrEmpty(source))
            {
                data.Add("TotalDownload", ApkModel.GetTotalDownload(source));
            }
            else
            {
                // TODO: get total download from all sources.
                data.Add("TotalDownload", 0);
            }

            return this.Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddOneMoreApkDownload()
        {
            string source = string.IsNullOrWhiteSpace(Request["form"]) ? string.Empty : Request["form"];
            source = source.ToLower().Trim();

            if (!string.IsNullOrEmpty(source))
            {
                ApkModel.AddOneMoreApkDownload(source);
            }
            else
            {
                ApkModel.AddOneMoreApkDownload("unknown");
            }

            
            return File(ApkModel.GetApkFromBlob().ToArray(), "application/vnd.android.package-archive", "MeiTuBiKan.apk");
        }
    }
}