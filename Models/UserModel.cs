using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meitubikanSite.Models
{
    public class UserModel
    {
        private static string DownloadActionType = "download";
        private static string CloudSaveActionType = "cloudsave";

        private static ImageModel ImageModelInstance = new ImageModel();

        // *** Business related functions ***
        // Log user action and update image related stats.
        public void LogUserAction(UserActionEntity entity)
        {
            if (entity != null)
            {
                InsertUserActionEntity(entity);
                string eventID = entity.RowKey;
                string dailyDateStr = StorageModel.GetDailyDateString();
                string encodedUrl = StorageModel.UrlEncode(entity.Url);
                string actionType = entity.ActionType;

                // Image stats
                List<ImageStatsEntity> imageStatsEntityList = GetImageStatsEntityByPartition(encodedUrl);
                if (imageStatsEntityList == null || imageStatsEntityList.Count == 0)
                {
                    ImageStatsEntity imageStatsEntity = new ImageStatsEntity(encodedUrl, eventID);
                    imageStatsEntity.TotalCount = 1;
                    if (actionType != null)
                    {
                        if (actionType.Equals(DownloadActionType))
                        {
                            imageStatsEntity.DownloadCount = 1;
                            imageStatsEntity.CloudSaveCount = 0;
                        }
                        else if (actionType.Equals(CloudSaveActionType))
                        {
                            imageStatsEntity.DownloadCount = 0;
                            imageStatsEntity.CloudSaveCount = 1;
                        }
                    }
                    InsertImageStatsEntity(imageStatsEntity);
                    // Crawl and save the image to blob
                    ImageModelInstance.SaveImage(encodedUrl);
                }
                else
                {
                    ImageStatsEntity imageStatsEntity = imageStatsEntityList[0];
                    imageStatsEntity.TotalCount++;
                    if (actionType != null)
                    {
                        if (actionType.Equals(DownloadActionType))
                        {
                            imageStatsEntity.DownloadCount++;
                        }
                        else if (actionType.Equals(CloudSaveActionType))
                        {
                            imageStatsEntity.CloudSaveCount++;
                        }
                    }
                    UpdateImageStatsEntity(imageStatsEntity);
                }

                // Image daily stats
                List<ImageDailyStatsEntity> imageDailyStatsEntityList = GetImageDailyStatsEntityByPartition(encodedUrl);
                if (imageDailyStatsEntityList == null || imageDailyStatsEntityList.Count == 0)
                {
                    ImageDailyStatsEntity imageDailyStatsEntity = new ImageDailyStatsEntity(encodedUrl, dailyDateStr);
                    imageDailyStatsEntity.TotalCount = 1;
                    if (actionType != null)
                    {
                        if (actionType.Equals(DownloadActionType))
                        {
                            imageDailyStatsEntity.DownloadCount = 1;
                            imageDailyStatsEntity.CloudSaveCount = 0;
                        }
                        else if (actionType.Equals(CloudSaveActionType))
                        {
                            imageDailyStatsEntity.DownloadCount = 0;
                            imageDailyStatsEntity.CloudSaveCount = 1;
                        }
                    }
                    InsertImageDailyStatsEntity(imageDailyStatsEntity);
                }
                else
                {
                    ImageDailyStatsEntity imageDailyStatsEntity = imageDailyStatsEntityList[0];
                    imageDailyStatsEntity.TotalCount++;
                    if (actionType != null)
                    {
                        if (actionType.Equals(DownloadActionType))
                        {
                            imageDailyStatsEntity.DownloadCount++;
                        }
                        else if (actionType.Equals(CloudSaveActionType))
                        {
                            imageDailyStatsEntity.CloudSaveCount++;
                        }
                    }
                    UpdateImageDailyStatsEntity(imageDailyStatsEntity);
                }
            }
        }

        // *** Basic storage operations ***
        // ** User Action **
        // Select
        private UserActionEntity GetUserActionEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.UserActionTableName)
                                          .Execute(TableOperation.Retrieve<UserActionEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (UserActionEntity)res.Result : null;
        }

        private List<UserActionEntity> GetUserActionEntityByPartition(string pKey)
        {
            TableQuery<UserActionEntity> query = new TableQuery<UserActionEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.UserActionTableName)
                               .ExecuteQuery(query).ToList<UserActionEntity>();
        }

        // TODO: select all items from table.

        // Insert
        private void InsertUserActionEntity(UserActionEntity entity)
        {
            StorageModel.GetTable(StorageModel.UserActionTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteUserActionEntity(UserActionEntity entity)
        {
            StorageModel.GetTable(StorageModel.UserActionTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateUserActionEntity(UserActionEntity entity)
        {
            StorageModel.GetTable(StorageModel.UserActionTableName)
                        .Execute(TableOperation.Replace(entity));
        }

        // ** Image Stats **
        // Select
        private ImageStatsEntity GetImageStatsEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.ImageStatsTableName)
                                          .Execute(TableOperation.Retrieve<ImageStatsEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (ImageStatsEntity)res.Result : null;
        }

        private List<ImageStatsEntity> GetImageStatsEntityByPartition(string pKey)
        {
            TableQuery<ImageStatsEntity> query = new TableQuery<ImageStatsEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.ImageStatsTableName)
                               .ExecuteQuery(query).ToList<ImageStatsEntity>();
        }

        // TODO: select all items from table.

        // Insert
        private void InsertImageStatsEntity(ImageStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ImageStatsTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteImageStatsEntity(ImageStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ImageStatsTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateImageStatsEntity(ImageStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ImageStatsTableName)
                        .Execute(TableOperation.Replace(entity));
        }

        // ** Image Daily Stats **
        // Select
        private ImageDailyStatsEntity GetImageDailyStatsEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.ImageDailyStatsTableName)
                                          .Execute(TableOperation.Retrieve<ImageDailyStatsEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (ImageDailyStatsEntity)res.Result : null;
        }

        private List<ImageDailyStatsEntity> GetImageDailyStatsEntityByPartition(string pKey)
        {
            TableQuery<ImageDailyStatsEntity> query = new TableQuery<ImageDailyStatsEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.ImageDailyStatsTableName)
                               .ExecuteQuery(query).ToList<ImageDailyStatsEntity>();
        }

        // TODO: select all items from table.

        // Insert
        private void InsertImageDailyStatsEntity(ImageDailyStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ImageDailyStatsTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteImageDailyStatsEntity(ImageDailyStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ImageDailyStatsTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateImageDailyStatsEntity(ImageDailyStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ImageDailyStatsTableName)
                        .Execute(TableOperation.Replace(entity));
        }
    }
    
    public class UserActionEntity : TableEntity
    {
        public UserActionEntity() { }

        public UserActionEntity(string clientID, string eventID)
        {
            this.PartitionKey = clientID;
            this.RowKey = eventID;
        }

        public string Query { get; set; }
        public string Url { get; set; }
        public string ActionType { get; set; }
        public string ApkVersion { get; set; }
    }

    public class ImageStatsEntity : TableEntity
    {
        public ImageStatsEntity() { }

        public ImageStatsEntity(string encodedUrl, string eventID)
        {
            this.PartitionKey = encodedUrl;
            this.RowKey = eventID;
        }

        public int TotalCount { get; set; }
        public int DownloadCount { get; set; }
        public int CloudSaveCount { get; set; }
    }

    public class ImageDailyStatsEntity : TableEntity
    {
        public ImageDailyStatsEntity() { }

        public ImageDailyStatsEntity(string encodedUrl, string dateStr)
        {
            this.PartitionKey = encodedUrl;
            this.RowKey = dateStr;
        }

        public int TotalCount { get; set; }
        public int DownloadCount { get; set; }
        public int CloudSaveCount { get; set; }
    }
}