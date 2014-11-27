using meitubikanSite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace meitubikanSite.Controllers
{
    public class UserController : Controller
    {
        private static UserModel UserModelInstance = new UserModel();

        private static readonly int TopCountLimit = 100;

        public string LogUserAction()
        {
            string clientID = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["clientid"]) ? string.Empty : Request["clientid"]);
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            // Require encoded with HttpUtility.UrlEncode
            string url = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["imageurl"]) ? string.Empty : Request["imageurl"]);
            string actionType = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["actiontype"]) ? string.Empty : Request["actiontype"]);
            string apkVersion = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["apkversion"]) ? string.Empty : Request["apkversion"]);

            string eventID = StorageModel.CreateEventId();
            UserActionEntity userActionEntity = new UserActionEntity(clientID, eventID);

            if (!string.IsNullOrEmpty(url))
            {
                userActionEntity.Query = query;
                userActionEntity.Url = url;
                userActionEntity.ActionType = actionType;
                userActionEntity.ApkVersion = apkVersion;

                UserModelInstance.LogUserAction(userActionEntity);

                return "Client ID:\t" + userActionEntity.PartitionKey + "\r\n" +
                "Event ID:\t" + userActionEntity.RowKey + "\r\n" +
                "Query:\t" + userActionEntity.Query + "\r\n" +
                "Url:\t" + userActionEntity.Url + "\r\n" +
                "Action type:\t" + userActionEntity.ActionType + "\r\n" +
                "Apk version:\t" + userActionEntity.ApkVersion;
            }
            else
            {
                return "Invalid data!" + "\t" + "ClientID: " + clientID + ", query: " + query + ", url: " + url + ", action type: " + actionType + ", apk version: " + apkVersion;
            }            
        }

        public string LogUserSearch()
        {
            string clientID = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["clientid"]) ? string.Empty : Request["clientid"]);
            // Require encoded with HttpUtility.UrlEncode
            string query = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["query"]) ? string.Empty : Request["query"]);
            string source = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["source"]) ? string.Empty : Request["source"]);
            string apkVersion = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["apkversion"]) ? string.Empty : Request["apkversion"]);

            string eventID = StorageModel.CreateEventId();
            UserSearchEntity userSearchEntity = new UserSearchEntity(clientID, eventID);

            if (!string.IsNullOrEmpty(query))
            {
                userSearchEntity.Query = query;
                userSearchEntity.ApkVersion = apkVersion;
                userSearchEntity.Source = source;

                UserModelInstance.LogUserSearch(userSearchEntity);

                return "Client ID: " + userSearchEntity.PartitionKey + ", Event ID: " + userSearchEntity.RowKey +
                    ", Query: " + userSearchEntity.Query + ", Source: " + userSearchEntity.Source +
                    ", Apk version: " + userSearchEntity.ApkVersion;
            }
            else
            {
                return "Invalid data!" + "\t" + "Client ID: " + clientID +
                    ", Query: " + query + ", Source: " + source + ", Apk version: " + apkVersion;
            }
        }

        public ActionResult TopQuery()
        {
            ViewBag.EntityList = UserModelInstance.GetTopQuery(TopCountLimit);
            return View();
        }

        public ActionResult DailyTopQuery()
        {
            return View();
        }

        public ActionResult TopImage()
        {
            ViewBag.EntityList = UserModelInstance.GetTopImage(TopCountLimit);
            return View();
        }

        public ActionResult DailyTopImage()
        {
            return View();
        }
    }
}