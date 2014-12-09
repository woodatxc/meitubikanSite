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
        private static int TotalCountLimit = 100;
        private static int EnlargeCountLimit = 3000;
        private static int ExpireMinutes = 7 * 24 * 60;

        public delegate void AsyncEnlargeImagePoolHandler(SearchEntity entity, string baseUrl);

        public JsonResult SearchCategory()
        {
            List<CategoryEntity> entityList = SearchModelInstance.GetAllCategoryEntity();
            List<CategoryItem> itemList = new List<CategoryItem>();

            for (int i = 0; i < entityList.Count; i++)
            {
                CategoryEntity entity = entityList[i];
                string category = StorageModel.UrlDecode(entity.PartitionKey);
                string subCategory = StorageModel.UrlDecode(entity.RowKey);
                string query = StorageModel.UrlDecode(entity.Query);
                int position = entity.Position;
                CategoryItem item = null;
                for (int j = 0; j < itemList.Count; j++)
                {
                    CategoryItem curItem = itemList[j];
                    if (curItem.Name.Equals(category))
                    {
                        item = curItem;
                        break;
                    }
                }
                if (item == null)
                {
                    item = new CategoryItem();
                    itemList.Add(item);
                }
                // Top setting
                if (string.IsNullOrWhiteSpace(subCategory))
                {
                    item.Name = category;
                    item.Query = query;
                    item.Pos = position;
                }
                // Sub setting
                else
                {
                    if (item.Children == null)
                    {
                        item.Children = new List<SubCategoryItem>();
                    }
                    SubCategoryItem subItem = new SubCategoryItem();
                    subItem.SubName = subCategory;
                    subItem.SubQuery = query;
                    subItem.SubPos = position;
                    item.Children.Add(subItem);
                }
            }

            string json = JsonConvert.SerializeObject(itemList);

            return this.Json(json, JsonRequestBehavior.AllowGet);
        }

        public void AddSearchCategory()
        {
            string category = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["category"]) ? string.Empty : Request["category"]);
            string subcategory = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["subcategory"]) ? string.Empty : Request["subcategory"]);
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int position = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["position"]) ? "0" : Request["position"]));

            CategoryEntity entity = new CategoryEntity();
            entity.PartitionKey = StorageModel.UrlEncode(category);
            entity.RowKey = StorageModel.UrlEncode(subcategory);
            entity.Query = StorageModel.UrlEncode(query);
            entity.Position = position;

            SearchModelInstance.SaveSearchCategory(entity);
        }

        public JsonResult ImageSearch()
        {
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int width = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["width"]) ? "0" : Request["width"]));
            int height = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["height"]) ? "0" : Request["height"]));
            string network = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["network"]) ? string.Empty : Request["network"]);

            string json = GetImageSearchJson(query, width, height, network);

            return this.Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImageSearchPortal()
        {
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int width = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["width"]) ? "0" : Request["width"]));
            int height = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["height"]) ? "0" : Request["height"]));
            string network = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["network"]) ? string.Empty : Request["network"]);

            string json = GetImageSearchJson(query, width, height, network);

            JArray ja = (JArray)JsonConvert.DeserializeObject(json);

            ViewBag.EntityList = ja;

            return View();
        }

        private string GetImageSearchJson(string query, int width, int height, string network)
        {
            string filter = GenerateFilter(width, height, network);
            string encodedFilter = StorageModel.UrlEncode(filter);
            string baseUrl = "http://www.bing.com/images/async?view=detail&q=" + query + filter;

            string json = "";

            SearchEntity entity = new SearchEntity(query, encodedFilter);
            SearchEntity cachedEntity = SearchModelInstance.GetSearchResult(entity);
            if (IsValid(cachedEntity))
            {
                json = SearchModelInstance.GetSearchResultJson(cachedEntity, false);
            }
            else
            {
                json = GenerateSearchResult(baseUrl, TotalCountLimit);
                SearchModelInstance.SaveSearchResult(entity);
                SearchModelInstance.SaveSearchResultJson(entity, json, false);
                // Async enlarge image pool
                AsyncEnlargeImagePoolHandler asy = new AsyncEnlargeImagePoolHandler(EnlargeImagePool);
                asy.BeginInvoke(entity, baseUrl, null, null);
            }

            return json;
        }

        private void EnlargeImagePool(SearchEntity entity, string baseUrl)
        {
            string json = GenerateSearchResult(baseUrl, EnlargeCountLimit);
            SearchModelInstance.SaveSearchResultJson(entity, json, true);
        }

        private string GenerateFilter(int width, int height, string network)
        {
            return "&qft=+filterui:imagesize-wallpaper";
        }

        private string GenerateSearchResult(string baseUrl, int limit)
        {
            string content = "";
            HashSet<string> midSet = new HashSet<string>();
            List<ImageItem> itemList = new List<ImageItem>();
            int position = 1;
            for (int i = 0; i < limit; i += EachStepCount)
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
                        item.Pos = position++;
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
            public int Pos { get; set; }
        }

        private class CategoryItem
        {
            public string Name { get; set; }
            public string Query { get; set; }
            public int Pos { get; set; }
            public List<SubCategoryItem> Children { get; set; }
        }

        private class SubCategoryItem
        {
            public string SubName { get; set; }
            public string SubQuery { get; set; }
            public int SubPos { get; set; }
        }
    }
}