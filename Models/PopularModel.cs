using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meitubikanSite.Models
{
    public class PopularModel
    {
        // *** Business related functions ***
        public List<PopularQueryEntity> GetPopularQuery(int count)
        {
            List<PopularQueryEntity> entityList = GetAllPopularQueryEntity();
            List<PopularQueryEntity> topEntityList = new List<PopularQueryEntity>(entityList.OrderBy(e => e.Position).ToList<PopularQueryEntity>().Take(count));
            foreach (PopularQueryEntity entity in topEntityList)
            {
                entity.PartitionKey = StorageModel.UrlDecode(entity.PartitionKey);
            }
            return topEntityList;
        }

        public void AddPopularQuery(PopularQueryEntity entity)
        {
            if (entity != null)
            {
                InsertPopularQueryEntity(entity);
            }
        }

        // *** Basic storage operations ***
        // ** Popular Query **
        // Select
        private PopularQueryEntity GetPopularQueryEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.PopularQueryTableName)
                                          .Execute(TableOperation.Retrieve<PopularQueryEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (PopularQueryEntity)res.Result : null;
        }

        private List<PopularQueryEntity> GetPopularQueryEntityByPartition(string pKey)
        {
            TableQuery<PopularQueryEntity> query = new TableQuery<PopularQueryEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.PopularQueryTableName)
                               .ExecuteQuery(query).ToList<PopularQueryEntity>();
        }

        private List<PopularQueryEntity> GetAllPopularQueryEntity()
        {
            CloudTable table = StorageModel.GetTable(StorageModel.PopularQueryTableName);
            TableContinuationToken token = null;
            List<PopularQueryEntity> entityList = new List<PopularQueryEntity>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<PopularQueryEntity>(), token);
                entityList.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entityList;
        }

        // TODO: select all items from table.

        // Insert
        private void InsertPopularQueryEntity(PopularQueryEntity entity)
        {
            StorageModel.GetTable(StorageModel.PopularQueryTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeletePopularQueryEntity(PopularQueryEntity entity)
        {
            StorageModel.GetTable(StorageModel.PopularQueryTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdatePopularQueryEntity(PopularQueryEntity entity)
        {
            StorageModel.GetTable(StorageModel.PopularQueryTableName)
                        .Execute(TableOperation.Replace(entity));
        }
    }

    public class PopularQueryEntity : TableEntity
    {
        public PopularQueryEntity() { }

        public PopularQueryEntity(string encodedQuery, string eventID)
        {
            this.PartitionKey = encodedQuery;
            this.RowKey = eventID;
        }

        public string Url { get; set; }
        public int Position { get; set; }
        public string AddDate { get; set; }
        public string LastUpdateDate { get; set; }
    }
}