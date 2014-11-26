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
            string apkVersion = ControllerHelper.NormalizeString(string.IsNullOrWhiteSpace(Request["apkversion"]) ? string.Empty : Request["apkversion"]);

            string eventID = StorageModel.CreateEventId();
            UserSearchEntity userSearchEntity = new UserSearchEntity(clientID, eventID);

            if (!string.IsNullOrEmpty(query))
            {
                userSearchEntity.Query = query;
                userSearchEntity.ApkVersion = apkVersion;

                UserModelInstance.LogUserSearch(userSearchEntity);

                return "Client ID: " + userSearchEntity.PartitionKey + ", Event ID: " + userSearchEntity.RowKey +
                    ", Query: " + userSearchEntity.Query + ", Apk version: " + userSearchEntity.ApkVersion;
            }
            else
            {
                return "Invalid data!" + "\t" + "Client ID: " + userSearchEntity.PartitionKey + 
                    ", Query: " + userSearchEntity.Query + ", Apk version: " + userSearchEntity.ApkVersion;
            }
        }
    }
}