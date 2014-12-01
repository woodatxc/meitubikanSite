using meitubikanSite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace meitubikanSite.Controllers
{
    public class PopularController : Controller
    {
        private static PopularModel PopularModelInstance = new PopularModel();
        private static readonly int TopCountLimit = 100;

        public JsonResult PopularQuery()
        {
            List<PopularQueryEntity> entityList = PopularModelInstance.GetPopularQuery(TopCountLimit);

            Hashtable data = new Hashtable();

            foreach (PopularQueryEntity entity in entityList)
            {
                data.Add(entity.PartitionKey, entity);
            }

            return this.Json(data, JsonRequestBehavior.AllowGet);
        }

        // Just for test, don't use this in real production!
        public string AddPopularQuery()
        {
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            // Require encoded with HttpUtility.UrlEncode
            string url = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["imageurl"]) ? string.Empty : Request["imageurl"]);
            int position = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["position"]) ? "0" : Request["position"]));

            string eventID = StorageModel.CreateEventId();
            string curDateStr = StorageModel.GetDailyDateString();

            if (!string.IsNullOrEmpty(query))
            {
                PopularQueryEntity entity = new PopularQueryEntity(query, eventID);
                entity.Url = url;
                entity.Position = position;
                entity.AddDate = curDateStr;
                entity.LastUpdateDate = curDateStr;

                PopularModelInstance.AddPopularQuery(entity);

                return "Add success! Query: " + entity.PartitionKey + ", Url: " + entity.Url;
            }
            else
            {
                return "Invaid data! Query: " + query + ", Url: " + url;
            }
        }
    }
}