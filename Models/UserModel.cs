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
                ImageDailyStatsEntity imageDailyStatsEntity = GetImageDailyStatsEntity(encodedUrl, dailyDateStr);
                if (imageDailyStatsEntity == null)
                {
                    imageDailyStatsEntity = new ImageDailyStatsEntity(encodedUrl, dailyDateStr);
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

        // Log user search and update related query stats
        public void LogUserSearch(UserSearchEntity entity)
        {
            if (entity != null)
            {
                InsertUserSearchEntity(entity);
                string eventID = entity.RowKey;
                string dailyDateStr = StorageModel.GetDailyDateString();
                string encodedQuery = StorageModel.UrlEncode(entity.Query);

                // Query stats
                List<QueryStatsEntity> queryStatsEntityList = GetQueryStatsEntityByPartition(encodedQuery);
                if (queryStatsEntityList == null || queryStatsEntityList.Count == 0)
                {
                    QueryStatsEntity queryStatsEntity = new QueryStatsEntity(encodedQuery, eventID);
                    queryStatsEntity.TotalCount = 1;
                    InsertQueryStatsEntity(queryStatsEntity);
                }
                else
                {
                    QueryStatsEntity queryStatsEntity = queryStatsEntityList[0];
                    queryStatsEntity.TotalCount++;
                    UpdateQueryStatsEntity(queryStatsEntity);
                }

                // QueryDaily stats
                QueryDailyStatsEntity queryDailyStatsEntity = GetQueryDailyStatsEntity(encodedQuery, dailyDateStr);
                if (queryDailyStatsEntity == null)
                {
                    queryDailyStatsEntity = new QueryDailyStatsEntity(encodedQuery, dailyDateStr);
                    queryDailyStatsEntity.TotalCount = 1;
                    InsertQueryDailyStatsEntity(queryDailyStatsEntity);
                }
                else
                {
                    queryDailyStatsEntity.TotalCount++;
                    UpdateQueryDailyStatsEntity(queryDailyStatsEntity);
                }
            }
        }

        // Get top query
        public List<QueryStatsEntity> GetTopQuery(int count)
        {
            List<QueryStatsEntity> entityList = GetAllQueryStatsEntity();
            List <QueryStatsEntity> topEntityList = new List<QueryStatsEntity>(entityList.OrderByDescending(e => e.TotalCount).ToList<QueryStatsEntity>().Take(count));
            foreach (QueryStatsEntity entity in topEntityList)
            {
                entity.PartitionKey = StorageModel.UrlDecode(entity.PartitionKey);
            }
            return topEntityList;
        }

        // Get top image
        public List<ImageStatsEntity> GetTopImage(int count)
        {
            List<ImageStatsEntity> entityList = GetAllImageStatsEntity();
            List<ImageStatsEntity> topEntityList = new List<ImageStatsEntity>(entityList.OrderByDescending(e => e.TotalCount).ToList<ImageStatsEntity>().Take(count));
            foreach (ImageStatsEntity entity in topEntityList)
            {
                entity.PartitionKey = StorageModel.UrlDecode(entity.PartitionKey);
            }
            return topEntityList;
        }

        // *** Helper functions ***


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

        private List<ImageStatsEntity> GetAllImageStatsEntity()
        {
            CloudTable table = StorageModel.GetTable(StorageModel.ImageStatsTableName);
            TableContinuationToken token = null;
            List<ImageStatsEntity> entityList = new List<ImageStatsEntity>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<ImageStatsEntity>(), token);
                entityList.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entityList;
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

        // ** User Search **
        // Select
        private UserSearchEntity GetUserSearchEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.UserSearchTableName)
                                          .Execute(TableOperation.Retrieve<UserSearchEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (UserSearchEntity)res.Result : null;
        }

        private List<UserSearchEntity> GetUserSearchEntityByPartition(string pKey)
        {
            TableQuery<UserSearchEntity> query = new TableQuery<UserSearchEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.UserSearchTableName)
                               .ExecuteQuery(query).ToList<UserSearchEntity>();
        }

        // TODO: select all items from table.

        // Insert
        private void InsertUserSearchEntity(UserSearchEntity entity)
        {
            StorageModel.GetTable(StorageModel.UserSearchTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteUserSearchEntity(UserSearchEntity entity)
        {
            StorageModel.GetTable(StorageModel.UserSearchTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateUserSearchEntity(UserSearchEntity entity)
        {
            StorageModel.GetTable(StorageModel.UserSearchTableName)
                        .Execute(TableOperation.Replace(entity));
        }

        // ** Query Stats **
        // Select
        private QueryStatsEntity GetQueryStatsEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.QueryStatsTableName)
                                          .Execute(TableOperation.Retrieve<QueryStatsEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (QueryStatsEntity)res.Result : null;
        }

        private List<QueryStatsEntity> GetQueryStatsEntityByPartition(string pKey)
        {
            TableQuery<QueryStatsEntity> query = new TableQuery<QueryStatsEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.QueryStatsTableName)
                               .ExecuteQuery(query).ToList<QueryStatsEntity>();
        }

        private List<QueryStatsEntity> GetAllQueryStatsEntity()
        {
            CloudTable table = StorageModel.GetTable(StorageModel.QueryStatsTableName);
            TableContinuationToken token = null;
            List<QueryStatsEntity> entityList = new List<QueryStatsEntity>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<QueryStatsEntity>(), token);
                entityList.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entityList;
        }

        // TODO: select all items from table.

        // Insert
        private void InsertQueryStatsEntity(QueryStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.QueryStatsTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteQueryStatsEntity(QueryStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.QueryStatsTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateQueryStatsEntity(QueryStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.QueryStatsTableName)
                        .Execute(TableOperation.Replace(entity));
        }

        // ** Query Daily Stats **
        // Select
        private QueryDailyStatsEntity GetQueryDailyStatsEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.QueryDailyStatsTableName)
                                          .Execute(TableOperation.Retrieve<QueryDailyStatsEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (QueryDailyStatsEntity)res.Result : null;
        }

        private List<QueryDailyStatsEntity> GetQueryDailyStatsEntityByPartition(string pKey)
        {
            TableQuery<QueryDailyStatsEntity> query = new TableQuery<QueryDailyStatsEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.QueryDailyStatsTableName)
                               .ExecuteQuery(query).ToList<QueryDailyStatsEntity>();
        }

        // TODO: select all items from table.

        // Insert
        private void InsertQueryDailyStatsEntity(QueryDailyStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.QueryDailyStatsTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteQueryDailyStatsEntity(QueryDailyStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.QueryDailyStatsTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateQueryDailyStatsEntity(QueryDailyStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.QueryDailyStatsTableName)
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

    public class UserSearchEntity : TableEntity
    {
        public UserSearchEntity() { }

        public UserSearchEntity(string clientID, string eventID)
        {
            this.PartitionKey = clientID;
            this.RowKey = eventID;
        }

        public string Query { get; set; }
        public string Source { get; set; }
        public string ApkVersion { get; set; }
    }

    public class QueryStatsEntity : TableEntity
    {
        public QueryStatsEntity() { }

        public QueryStatsEntity(string encodedQuery, string eventID)
        {
            this.PartitionKey = encodedQuery;
            this.RowKey = eventID;
        }

        public int TotalCount { get; set; }
    }

    public class QueryDailyStatsEntity : TableEntity
    {
        public QueryDailyStatsEntity() { }

        public QueryDailyStatsEntity(string encodedQuery, string dateStr)
        {
            this.PartitionKey = encodedQuery;
            this.RowKey = dateStr;
        }

        public int TotalCount { get; set; }
    }
}