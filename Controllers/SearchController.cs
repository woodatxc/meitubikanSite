using meitubikanSite.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace meitubikanSite.Controllers
{
    public class SearchController : Controller
    {
        private static SearchModel SearchModelInstance = new SearchModel();
        private static int EachStepCount = 30;
        private static int StartPosition = 1;
        private static int TotalCountLimit = 300;
        private static int ExpireMinutes = 7 * 24 * 60;

        public JsonResult ImageSearch()
        {
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int width = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["width"]) ? "0" : Request["width"]));
            int height = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["height"]) ? "0" : Request["height"]));
            string network = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["network"]) ? string.Empty : Request["network"]);

            string filter = GenerateFilter(width, height, network);
            string encodedFilter = StorageModel.UrlEncode(filter);

            string json = "";

            SearchEntity entity = new SearchEntity(query, encodedFilter);
            SearchEntity cachedEntity = SearchModelInstance.GetSearchResult(entity);
            if (IsValid(cachedEntity))
            {
                json = SearchModelInstance.GetSearchResultJson(cachedEntity);
            }
            else
            {
                string baseUrl = "http://www.bing.com/images/async?view=detail&q=" + query + filter;
                json = GenerateSearchResult(baseUrl);
                SearchModelInstance.SaveSearchResult(entity);
                SearchModelInstance.SaveSearchResultJson(entity, json);
            }

            return this.Json(json, JsonRequestBehavior.AllowGet);
        }

        private string GenerateFilter(int width, int height, string network)
        {
            return "";
        }

        private string GenerateSearchResult(string baseUrl)
        {
            string content = "";
            HashSet<string> midSet = new HashSet<string>();
            List<ImageItem> itemList = new List<ImageItem>();
            for (int i = 0; i < TotalCountLimit; i += EachStepCount)
            {
                string url = baseUrl + "&count=" + EachStepCount + "&first=" + (StartPosition + i);
                string singleContent = ControllerHelper.Crawl(url, "utf-8");
                JObject jo = (JObject)JsonConvert.DeserializeObject(singleContent);
                string nextJsonStr = jo["next"].ToString();
                JArray ja = (JArray)JsonConvert.DeserializeObject(nextJsonStr);
                for (int j = 0; j < ja.Count; j++)
                {
                    string mid = ja[j]["mid"].ToString();
                    if (!midSet.Contains(mid))
                    {
                        midSet.Add(mid);
                        ImageItem item = new ImageItem();
                        item.ImgUrl = ja[j]["imgUrl"].ToString();
                        item.LthUrl = ja[j]["lthUrl"].ToString();
                        item.Width = ja[j]["width"].ToString();
                        item.Height = ja[j]["height"].ToString();
                        item.ImgUrl = ja[j]["imgUrl"].ToString();
                        string meta = ja[j]["meta"].ToString();
                        string[] strs = meta.Split(new char[] { '·' });
                        string[] parts = ControllerHelper.NormalizeString(strs[0]).Split(new char[] { 'x' });
                        if (strs.Length == 3 && parts.Length == 2)
                        {
                            item.OWidth = ControllerHelper.NormalizeString(parts[0]);
                            item.OHeight = ControllerHelper.NormalizeString(parts[1]);
                            item.OSize = ControllerHelper.NormalizeString(strs[1]);
                        }
                        itemList.Add(item);
                    }
                }
            }

            JsonSerializer serializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            serializer.Serialize(new JsonTextWriter(sw), itemList);
            content = sw.GetStringBuilder().ToString();

            return content;
        }

        private bool IsValid(SearchEntity cachedEntity)
        {
            if (cachedEntity == null)
            {
                return false;
            }
            else if ((DateTime.Now - cachedEntity.Timestamp).TotalMinutes > ExpireMinutes)
            {
                return false;
            }
            return true;
        }

        private class ImageItem
        {
            public string ImgUrl { get; set; }
            public string LthUrl { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string OWidth { get; set; }
            public string OHeight { get; set; }
            public string OSize { get; set; }
        }
    }
}