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
        private static UserModel UserModelInstance = new UserModel();
        private static ImageModel ImageModelInstance = new ImageModel();
        private static int EachStepCount = 30;
        private static int StartPosition = 2;
        private static int TotalCountLimit = EachStepCount * 5;
        private static int EnlargeCountLimit = EachStepCount * 100;
        private static int ExpireMinutes = 7 * 24 * 60;
        private static int MinimumResultCount = 20;
        private static int MinimumTotalCount = 100;
        private static int OverlapDelta = 0;

        public delegate void AsyncEnlargeImagePoolHandler(SearchEntity entity, string baseUrl);
        public delegate void AsyncDownloadImageHandler(SearchEntity entity, string json);

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
                string imgUrl = StorageModel.UrlDecode(entity.ImgUrl);
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
                    item.ImgUrl = imgUrl;
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
                    subItem.SubImgUrl = imgUrl;
                    subItem.SubPos = position;
                    item.Children.Add(subItem);
                }
            }

            // Sort by position
            for (int i = 0; i < itemList.Count; i++)
            {
                CategoryItem item = itemList[i];
                List<SubCategoryItem> subitemList = item.Children;
                if (subitemList != null)
                {
                    item.Children = new List<SubCategoryItem>(subitemList.OrderBy(s => s.SubPos));
                }
            }
            itemList = new List<CategoryItem>(itemList.OrderBy(i => i.Pos));

            string json = JsonConvert.SerializeObject(itemList);

            return this.Json(json, JsonRequestBehavior.AllowGet);
        }

        public void AddSearchCategory()
        {
            string category = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["category"]) ? string.Empty : Request["category"]);
            string subcategory = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["subcategory"]) ? string.Empty : Request["subcategory"]);
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int position = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["position"]) ? "0" : Request["position"]));
            string imgUrl = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["imgurl"]) ? string.Empty : Request["imgurl"]);

            CategoryEntity entity = new CategoryEntity();
            entity.PartitionKey = StorageModel.UrlEncode(category);
            entity.RowKey = StorageModel.UrlEncode(subcategory);
            entity.Query = StorageModel.UrlEncode(query);
            entity.Position = position;
            entity.ImgUrl = imgUrl;

            SearchModelInstance.SaveSearchCategory(entity);
        }

        public void WarmUpCategory()
        {
            List<CategoryEntity> entityList = SearchModelInstance.GetAllCategoryEntity();
            for (int i = 0; i < entityList.Count; i++)
            {
                string query = StorageModel.UrlDecode(entityList[i].Query);
                this.GetImageSearchJson(query, 0, 0, string.Empty, 1);
            }
        }

        public JsonResult ImageSearch()
        {
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int width = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["width"]) ? "0" : Request["width"]));
            int height = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["height"]) ? "0" : Request["height"]));
            string network = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["network"]) ? string.Empty : Request["network"]);
            int page = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["page"]) ? "1" : Request["page"]));

            string json = JsonConvert.SerializeObject(GetImageSearchJson(query, width, height, network, page));

            return this.Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImageSearchPortal()
        {
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            int width = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["width"]) ? "0" : Request["width"]));
            int height = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["height"]) ? "0" : Request["height"]));
            string network = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["network"]) ? string.Empty : Request["network"]);
            int page = int.Parse(ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["page"]) ? "1" : Request["page"]));

            ViewBag.EntityList = GetImageSearchJson(query, width, height, network, page);

            return View();
        }

        public ActionResult GetImage()
        {
            // Require encoded with HttpUtility.UrlEncode
            string url = ControllerHelper.NormalizeStringKeepCase(string.IsNullOrWhiteSpace(Request["url"]) ? string.Empty : Request["url"]);

            MemoryStream ms = ImageModelInstance.GetImageFromBlob(url);

            return File(ms.ToArray(), "image/jpeg");
        }

        private List<ImageItem> GetImageSearchJson(string query, int width, int height, string network, int page)
        {
            List<ImageItem> itemList = new List<ImageItem>();

            string filter = GenerateFilter(width, height, network);
            string encodedFilter = StorageModel.UrlEncode(filter);
            string baseUrl = "http://www.bing.com/images/async?view=detail&q=" + query + filter;

            SearchEntity entity = new SearchEntity(query, encodedFilter);
            SearchEntity cachedEntity = SearchModelInstance.GetSearchResult(entity);
            // Already cached and not outdated
            if (IsValid(cachedEntity))
            {
                // Get enlarge json
                string enlargeJson = SearchModelInstance.GetSearchResultJson(cachedEntity, true);
                // Has cached results and has enough images.
                if (enlargeJson != null && HasEnoughImages(enlargeJson))
                {
                    // Select from enlarge json
                    itemList = SelectFromJson(enlargeJson, page);
                    return itemList;
                }
            }
            
            // No cache or cache lost / expired, get result from Bing
            // Warm up backend
            //string warmupUrl = "http://www.bing.com/images/search?q=" + query + filter;
            //ControllerHelper.Crawl(warmupUrl, "utf-8", true);
            string json = GenerateSearchResult(entity, baseUrl, TotalCountLimit, false);
            // Select from small json
            itemList = SelectFromJson(json, page);
            // Record cache status
            SearchModelInstance.SaveSearchResult(entity);
            // Save small json file
            SearchModelInstance.SaveSearchResultJson(entity, json, false);
            // Async enlarge image pool
            AsyncEnlargeImagePoolHandler asy = new AsyncEnlargeImagePoolHandler(EnlargeImagePool);
            asy.BeginInvoke(entity, baseUrl, null, null);

            return itemList;
        }

        private List<ImageItem> SelectFromJson(string json, int page)
        {
            List<ImageItem> itemList = new List<ImageItem>();
            if (json != null)
            {
                JArray ja = (JArray)JsonConvert.DeserializeObject(json);

                for (int i = 0; i < ja.Count; i++)
                {
                    ImageItem item = new ImageItem();
                    item.ImgUrl = ja[i]["ImgUrl"].ToString();
                    item.LthUrl = ja[i]["LthUrl"].ToString();
                    item.Width = ja[i]["Width"].ToString();
                    item.Height = ja[i]["Height"].ToString();
                    item.OWidth = ja[i]["OWidth"].ToString();
                    item.OHeight = ja[i]["OHeight"].ToString();
                    item.OSize = ja[i]["OSize"].ToString();
                    item.Pos = int.Parse(ja[i]["Pos"].ToString());

                    if (DateSelect(item.Pos, ja.Count, page))
                    {
                        itemList.Add(item);
                    }
                }
            }
            return itemList;
        }

        private bool DateSelect(int pos, int totalCount, int page)
        {
            // Special case: total count is too few, return all for first page and return null for others
            if (totalCount <= MinimumResultCount)
            {
                return page == 1 ? true : false;
            }
            // Each time we return MinimumResultCount images
            int noDupWindow = totalCount / MinimumResultCount;
            // Date determine the start point
            DateTime baseDate = new DateTime(2014, 1, 1);
            int datePoint = (DateTime.Now - baseDate).Days;
            // Page determine the offset
            if (page > noDupWindow)
            {
                // All images are returned
                return false;
            }
            else
            {
                int target = (datePoint + page) % noDupWindow;
                int cur = pos % noDupWindow;
                if (cur == target)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasEnoughImages(string json)
        {
            if (json != null)
            {
                JArray ja = (JArray)JsonConvert.DeserializeObject(json);

                if (ja.Count >= MinimumTotalCount)
                {
                    return true;
                }
            }
            return false;
        }

        private void EnlargeImagePool(SearchEntity entity, string baseUrl)
        {
            string json = GenerateSearchResult(entity, baseUrl, EnlargeCountLimit, true);
            // Save enlarge json file
            SearchModelInstance.SaveSearchResultJson(entity, json, true);
            // Async download image
            // Just for category queries and hot queries
            if (SearchModelInstance.IsCategoryQuery(entity.PartitionKey) || UserModelInstance.IsHotQuery(entity.PartitionKey))
            {
                AsyncDownloadImageHandler asy = new AsyncDownloadImageHandler(DownloadImage);
                asy.BeginInvoke(entity, json, null, null);
            }
        }

        private void DownloadImage(SearchEntity entity, string json)
        {
            List<ImageItem> itemList = new List<ImageItem>();
            JArray ja = (JArray)JsonConvert.DeserializeObject(json);
            int pos = 1;
            string absoluteUri = HttpContext.Request.Url.AbsoluteUri;
            string absolutePath = HttpContext.Request.Url.AbsolutePath;
            string prefix = absoluteUri.Substring(0, absoluteUri.IndexOf(absolutePath));
            for (int i = 0; i < ja.Count; i++)
            {
                // Raw item
                ImageItem item = new ImageItem();
                item.ImgUrl = ja[i]["ImgUrl"].ToString();
                item.LthUrl = ja[i]["LthUrl"].ToString();
                item.Width = ja[i]["Width"].ToString();
                item.Height = ja[i]["Height"].ToString();
                item.OWidth = ja[i]["OWidth"].ToString();
                item.OHeight = ja[i]["OHeight"].ToString();
                item.OSize = ja[i]["OSize"].ToString();
                item.Pos = int.Parse(ja[i]["Pos"].ToString());
                // Download thumbnail
                string encodedLthUrl = StorageModel.UrlEncode(item.LthUrl);
                string lthUrlPath = ImageModelInstance.SaveImage(encodedLthUrl);
                // Download big image
                if (!string.IsNullOrWhiteSpace(lthUrlPath))
                {
                    string encodedImgUrl = StorageModel.UrlEncode(item.ImgUrl);
                    string imgUrlPath = ImageModelInstance.SaveImage(encodedImgUrl);
                    // Download success for both thumbnail and big image, keep
                    if (!string.IsNullOrWhiteSpace(imgUrlPath))
                    {
                        item.ImgUrl = prefix + "/Search/GetImage?url=" + encodedImgUrl;
                        item.LthUrl = prefix + "/Search/GetImage?url=" + encodedLthUrl;
                        item.Pos = pos++;
                        itemList.Add(item);
                    }
                }
            }
            JsonSerializer serializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            serializer.Serialize(new JsonTextWriter(sw), itemList);
            string content = sw.GetStringBuilder().ToString();
            // Update enlarge json file
            SearchModelInstance.SaveSearchResultJson(entity, content, true);
        }

        private string GenerateFilter(int width, int height, string network)
        {
            return "&qft=+filterui:imagesize-large";
        }

        private string GenerateSearchResult(SearchEntity entity, string baseUrl, int limit, bool isEnlarge)
        {
            string content = "";
            HashSet<string> midSet = new HashSet<string>();
            List<ImageItem> itemList = new List<ImageItem>();
            int position = 1;
            for (int i = 0; i < limit; i += (EachStepCount - OverlapDelta))
            {
                string url = baseUrl + "&count=" + EachStepCount + "&first=" + (StartPosition + i - OverlapDelta);
                // Warm up
                //string warmupUrl = url.Replace("http://www.bing.com/images/async?view=detail&", "http://www.bing.com/images/search?");
                //ControllerHelper.Crawl(url, "utf-8", true);
                string singleContent = ControllerHelper.Crawl(url, "utf-8");
                if (!isEnlarge)
                {
                    // Debug data
                    //SearchModelInstance.SaveSearchResultDebugJson(entity, singleContent, StartPosition + i, EachStepCount);
                }
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
            // No record, invalid.
            if (cachedEntity == null)
            {
                return false;
            }
            // Outdated, invalid.
            else if ((DateTime.Now - cachedEntity.Timestamp).TotalMinutes > ExpireMinutes)
            {
                return false;
            }
            return true;
        }

        public class ImageItem
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
            public string ImgUrl { get; set; }
            public List<SubCategoryItem> Children { get; set; }
        }

        private class SubCategoryItem
        {
            public string SubName { get; set; }
            public string SubQuery { get; set; }
            public int SubPos { get; set; }
            public string SubImgUrl { get; set; }
        }
    }
}