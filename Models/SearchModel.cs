using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace meitubikanSite.Models
{
    public class SearchModel
    {
        // *** Business related functions ***
        // Try to get cached search result.
        public SearchEntity GetSearchResult(SearchEntity entity)
        {
            SearchEntity cachedEntity = null;

            if (entity != null)
            {
                cachedEntity = GetSearchEntity(entity.PartitionKey, entity.RowKey);
            }

            return cachedEntity;
        }

        // Save search result to cache.
        public void SaveSearchResult(SearchEntity entity)
        {
            if (entity != null)
            {
                SearchEntity cachedEntity = GetSearchEntity(entity.PartitionKey, entity.RowKey);
                if (cachedEntity == null)
                {
                    InsertSearchEntity(entity);
                }
                else
                {
                    UpdateSearchEntity(entity);
                }
            }
        }

        // Get json from blob
        public string GetSearchResultJson(SearchEntity entity)
        {
            if (entity != null)
            {
                string filename = GetSearchResultFilename(entity);
                MemoryStream ms = new MemoryStream();
                CloudBlockBlob blob = StorageModel.GetBlobContainer(StorageModel.SearchResultContainerName).GetBlockBlobReference(filename);
                blob.DownloadToStream(ms);
                string json = System.Text.Encoding.Default.GetString(ms.ToArray());
                return json;
            }
            return null;
        }

        // Save json to blob
        public void SaveSearchResultJson(SearchEntity entity, string json)
        {
            if (entity != null)
            {
                string filename = GetSearchResultFilename(entity);
                CloudBlockBlob blob = StorageModel.GetBlobContainer(StorageModel.SearchResultContainerName).GetBlockBlobReference(filename);
                blob.DeleteIfExists();
                blob.UploadText(json);
            }
        }

        public string GetSearchResultFilename(SearchEntity entity)
        {
            string filename = "";
            if (entity != null)
            {
                filename = entity.PartitionKey + "_" + entity.RowKey;
            }
            return filename;
        }

        // *** Basic storage operations ***
        // ** Search **
        // Select
        private SearchEntity GetSearchEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.SearchResultTableName)
                                          .Execute(TableOperation.Retrieve<SearchEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (SearchEntity)res.Result : null;
        }

        private List<SearchEntity> GetSearchEntityByPartition(string pKey)
        {
            TableQuery<SearchEntity> query = new TableQuery<SearchEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.SearchResultTableName)
                               .ExecuteQuery(query).ToList<SearchEntity>();
        }

        private List<SearchEntity> GetAllSearchEntity()
        {
            CloudTable table = StorageModel.GetTable(StorageModel.SearchResultTableName);
            TableContinuationToken token = null;
            List<SearchEntity> entityList = new List<SearchEntity>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<SearchEntity>(), token);
                entityList.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entityList;
        }

        // TODO: select all items from table.

        // Insert
        private void InsertSearchEntity(SearchEntity entity)
        {
            StorageModel.GetTable(StorageModel.SearchResultTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteSearchEntity(SearchEntity entity)
        {
            StorageModel.GetTable(StorageModel.SearchResultTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateSearchEntity(SearchEntity entity)
        {
            StorageModel.GetTable(StorageModel.SearchResultTableName)
                        .Execute(TableOperation.Replace(entity));
        }
    }

    public class SearchEntity : TableEntity
    {
        public SearchEntity() { }

        public SearchEntity(string encodedQuery, string filter)
        {
            this.PartitionKey = encodedQuery;
            this.RowKey = filter;
        }
    }
}