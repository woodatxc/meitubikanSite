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
            if (string.IsNullOrWhiteSpace(source))
            {
                string channel = string.IsNullOrWhiteSpace(Request["channel"]) ? string.Empty : Request["channel"];
                source = channel.ToLower().Trim();
            }

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

        // Get download link click count based on time, time format:201501010101.
        public JsonResult GetDownloadOnTime()
        {
            Hashtable data = new Hashtable();

            string source = string.IsNullOrWhiteSpace(Request["form"]) ? string.Empty : Request["form"];
            source = source.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                string channel = string.IsNullOrWhiteSpace(Request["channel"]) ? string.Empty : Request["channel"];
                source = channel.ToLower().Trim();
            }

            string time = string.IsNullOrWhiteSpace(Request["time"]) ? string.Empty : Request["time"];

            if (!string.IsNullOrEmpty(source))
            {
                data.Add("DownloadCount", ApkModel.GetTotalDownloadOnTime(source, time));
            }
            else
            {
                data.Add("DownloadCount", BaseDownload + ApkModel.GetTotalDownloadOnTime("ucresultpage", time) + ApkModel.GetTotalDownloadOnTime("uclandingpage", time) + ApkModel.GetTotalDownloadOnTime("ucdetailpage", time));
            }

            // Allow access from other domain
            this.ControllerContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");

            return this.Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddOneMoreApkDownload()
        {
            string source = string.IsNullOrWhiteSpace(Request["form"]) ? string.Empty : Request["form"];
            source = source.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                string channel = string.IsNullOrWhiteSpace(Request["channel"]) ? string.Empty : Request["channel"];
                source = channel.ToLower().Trim();
            }

            if (!string.IsNullOrEmpty(source))
            {
                ApkModel.AddOneMoreApkDownload(source);
            }
            else
            {
                ApkModel.AddOneMoreApkDownload("unknown");
            }

            return File(ApkModel.GetApkFromBlobPerChannel(source).ToArray(), "application/vnd.android.package-archive", "meitubikan.apk");
        }

        public ActionResult AddOneMoreApkDownload_backup()
        {
            string source = string.IsNullOrWhiteSpace(Request["form"]) ? string.Empty : Request["form"];
            source = source.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                string channel = string.IsNullOrWhiteSpace(Request["channel"]) ? string.Empty : Request["channel"];
                source = channel.ToLower().Trim();
            }

            if (!string.IsNullOrEmpty(source))
            {
                ApkModel.AddOneMoreApkDownload(source);
            }
            else
            {
                ApkModel.AddOneMoreApkDownload("unknown");
            }

            return File(ApkModel.GetApkFromBlob().ToArray(), "application/vnd.android.package-archive", "meitubikan.apk");
        }

        public ActionResult AddOneMoreApkDownloadPerChannel()
        {
            string source = string.IsNullOrWhiteSpace(Request["form"]) ? string.Empty : Request["form"];
            source = source.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                string channel = string.IsNullOrWhiteSpace(Request["channel"]) ? string.Empty : Request["channel"];
                source = channel.ToLower().Trim();
            }

            if (!string.IsNullOrEmpty(source))
            {
                ApkModel.AddOneMoreApkDownload(source);
            }
            else
            {
                ApkModel.AddOneMoreApkDownload("unknown");
            }

            return File(ApkModel.GetApkFromBlobPerChannel(source).ToArray(), "application/vnd.android.package-archive", "meitubikan.apk");
        }

    }
}