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

        private const long BaseDownload = 74228;

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
                data.Add("TotalDownload", BaseDownload + ApkModel.GetTotalDownload("ucresultpage") + ApkModel.GetTotalDownload("uclandingpage") + ApkModel.GetTotalDownload("ucdetailpage"));
            }
            
            // Allow access from other domain
            this.ControllerContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");

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


            return File(ApkModel.GetApkFromBlob().ToArray(), "application/vnd.android.package-archive", "%E7%BE%8E%E5%9B%BE%E5%BF%85%E7%9C%8B.apk");
        }

        public ActionResult AddOneMoreApkDownloadChs()
        {
            string source = string.IsNullOrWhiteSpace(Request["channel"]) ? string.Empty : Request["channel"];
            source = source.ToLower().Trim();

            if (!string.IsNullOrEmpty(source))
            {
                ApkModel.AddOneMoreApkDownload(source);
            }
            else
            {
                ApkModel.AddOneMoreApkDownload("unknown");
            }

            return File(ApkModel.GetApkFromBlob().ToArray(), "application/vnd.android.package-archive", "%E7%BE%8E%E5%9B%BE%E5%BF%85%E7%9C%8B.apk");
        }

    }
}