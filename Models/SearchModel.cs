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
                    entity.ETag = "*";
                    UpdateSearchEntity(entity);
                }
            }
        }

        // Get json from blob
        public string GetSearchResultJson(SearchEntity entity, bool isEnlarge)
        {
            if (entity != null)
            {
                string filename = GetSearchResultFilename(entity, isEnlarge);
                MemoryStream ms = new MemoryStream();
                CloudBlockBlob blob = StorageModel.GetBlobContainer(StorageModel.SearchResultContainerName).GetBlockBlobReference(filename);
                if (!blob.Exists())
                {
                    return null;
                }
                blob.DownloadToStream(ms);
                string json = System.Text.Encoding.Default.GetString(ms.ToArray());
                return json;
            }
            return null;
        }

        // Save json to blob
        public void SaveSearchResultJson(SearchEntity entity, string json, bool isEnlarge)
        {
            if (entity != null)
            {
                string filename = GetSearchResultFilename(entity, isEnlarge);
                CloudBlockBlob blob = StorageModel.GetBlobContainer(StorageModel.SearchResultContainerName).GetBlockBlobReference(filename);
                blob.DeleteIfExists();
                blob.UploadText(json);
            }
        }

        // Save debug json to blob
        public void SaveSearchResultDebugJson(SearchEntity entity, string json, int first, int count)
        {
            if (entity != null)
            {
                string filename = entity.PartitionKey + "_" + entity.RowKey + "_" + first + "_" + count;
                CloudBlockBlob blob = StorageModel.GetBlobContainer(StorageModel.SearchResultContainerName).GetBlockBlobReference(filename);
                blob.DeleteIfExists();
                blob.UploadText(json);
            }
        }

        public string GetSearchResultFilename(SearchEntity entity, bool isEnlarge)
        {
            string filename = "";
            if (entity != null)
            {
                filename = entity.PartitionKey + "_" + entity.RowKey;
                if (isEnlarge)
                {
                    filename += "_enlarge";
                }
            }
            return filename;
        }

        // Save search category to cache.
        public void SaveSearchCategory(CategoryEntity entity)
        {
            if (entity != null)
            {
                CategoryEntity cachedEntity = GetCategoryEntity(entity.PartitionKey, entity.RowKey);
                if (cachedEntity == null)
                {
                    InsertCategoryEntity(entity);
                }
                else
                {
                    UpdateCategoryEntity(entity);
                }
            }
        }

        public bool IsCategoryQuery(string encodedQuery)
        {
            if (!string.IsNullOrWhiteSpace(encodedQuery))
            {
                List<CategoryEntity> entityList = GetAllCategoryEntity();
                for (int i = 0; i < entityList.Count; i++)
                {
                    // In case the query is not encoded
                    string doubleEncodedQuery = StorageModel.UrlEncode(encodedQuery);
                    if (encodedQuery.Equals(entityList[i].Query) || doubleEncodedQuery.Equals(entityList[i].Query))
                    {
                        return true;
                    }
                }
            }
            return false;
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

        // ** Category **
        // Select
        public CategoryEntity GetCategoryEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.SearchCategoryTableName)
                                          .Execute(TableOperation.Retrieve<CategoryEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (CategoryEntity)res.Result : null;
        }

        public List<CategoryEntity> GetCategoryEntityByPartition(string pKey)
        {
            TableQuery<CategoryEntity> query = new TableQuery<CategoryEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.SearchCategoryTableName)
                               .ExecuteQuery(query).ToList<CategoryEntity>();
        }

        public List<CategoryEntity> GetAllCategoryEntity()
        {
            CloudTable table = StorageModel.GetTable(StorageModel.SearchCategoryTableName);
            TableContinuationToken token = null;
            List<CategoryEntity> entityList = new List<CategoryEntity>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<CategoryEntity>(), token);
                entityList.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entityList;
        }

        // Insert
        public void InsertCategoryEntity(CategoryEntity entity)
        {
            StorageModel.GetTable(StorageModel.SearchCategoryTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        public void DeleteCategoryEntity(CategoryEntity entity)
        {
            StorageModel.GetTable(StorageModel.SearchCategoryTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        public void UpdateCategoryEntity(CategoryEntity entity)
        {
            StorageModel.GetTable(StorageModel.SearchCategoryTableName)
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

    public class CategoryEntity : TableEntity
    {
        public CategoryEntity() { }

        public CategoryEntity(string encodedCategory, string encodedSubCategory)
        {
            this.PartitionKey = encodedCategory;
            this.RowKey = encodedSubCategory;
        }

        public string Query { get; set; }
        public int Position { get; set; }
        public string ImgUrl { get; set; }
    }
}