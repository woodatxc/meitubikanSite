using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace meitubikanSite.Models
{
    public class ApkModel
    {
        // *** Business related functions ***
        // Add one more apk download
        public void AddOneMoreApkDownload(string source)
        {
            string eventId = StorageModel.CreateEventId();
            // Insert new apk download entity
            ApkDownloadEntity apkDownloadEntity = new ApkDownloadEntity(source, eventId);
            InsertApkDownloadEntity(apkDownloadEntity);
            // Update apk download stats entity
            List<ApkDownloadStatsEntity> statsList = GetApkDownloadStatsEntityByPartition(source);
            if (statsList == null || statsList.Count == 0)
            {
                // No stats for this source before, create one
                ApkDownloadStatsEntity statsEntity = new ApkDownloadStatsEntity(source, eventId);
                statsEntity.TotalDownload = 1;
                InsertApkDownloadStatsEntity(statsEntity);
            }
            else
            {
                // Already stats for this source, update it
                ApkDownloadStatsEntity statsEntity = statsList[0];
                statsEntity.TotalDownload++;
                UpdateApkDownloadStatsEntity(statsEntity);
            }
        }

        // Get download for specific source per time
        public long GetTotalDownloadOnTime(string source, string time)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(time))
            {
                return 0;
            }

            List<ApkDownloadEntity> apkDownloadList = GetApkDownloadEntityByPartition(source);

            long count = 0;
            foreach(var i in apkDownloadList)
            {
                if (i.RowKey.Contains(time))
                {
                    count++;
                }
            }

            return count;
        }

        // Get total download for specific source
        public long GetTotalDownload(string source)
        {
            List<ApkDownloadStatsEntity> statsList = GetApkDownloadStatsEntityByPartition(source);
            if (statsList == null || statsList.Count == 0)
            {
                return 0;
            }
            else
            {
                return statsList[0].TotalDownload;
            }
        }

        // TODO: get total download from all sources.

        // Get apk from blob
        public MemoryStream GetApkFromBlob()
        {
            MemoryStream ms = new MemoryStream();
            StorageModel.GetBlobContainer(StorageModel.ApkContainerName)
                            .GetBlockBlobReference(StorageModel.ApkFileName)
                            .DownloadToStream(ms);
            return ms;
        }

        // Get apk from blob per channel
        public MemoryStream GetApkFromBlobPerChannel(String channel)
        {
            MemoryStream ms = new MemoryStream();
            if (string.Equals(channel, StorageModel.UCWebDetailPageChannel, StringComparison.OrdinalIgnoreCase))
            {
                StorageModel.GetBlobContainer(StorageModel.ApkContainerName)
                            .GetBlockBlobReference(StorageModel.UCWebDetailPageApkFileName)
                            .DownloadToStream(ms);
            } 
            else if (string.Equals(channel, StorageModel.UCWebResultPageChannel, StringComparison.OrdinalIgnoreCase))
            {
                StorageModel.GetBlobContainer(StorageModel.ApkContainerName)
                            .GetBlockBlobReference(StorageModel.UCWebResultPageApkFileName)
                            .DownloadToStream(ms);
            }
            else if (string.Equals(channel, StorageModel.UCWebLandingPageChannel, StringComparison.OrdinalIgnoreCase))
            {
                StorageModel.GetBlobContainer(StorageModel.ApkContainerName)
                            .GetBlockBlobReference(StorageModel.UCWebLandingPageApkFileName)
                            .DownloadToStream(ms);
            } else {
                StorageModel.GetBlobContainer(StorageModel.ApkContainerName)
                            .GetBlockBlobReference(StorageModel.ApkFileName)
                            .DownloadToStream(ms);
            }
            
            return ms;
        }
        
        // *** Basic storage operations ***
        // Select
        private ApkDownloadEntity GetApkDownloadEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.ApkDownloadTableName)
                                          .Execute(TableOperation.Retrieve<ApkDownloadEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (ApkDownloadEntity)res.Result : null;
        }

        private List<ApkDownloadEntity> GetApkDownloadEntityByPartition(string pKey)
        {
            TableQuery<ApkDownloadEntity> query = new TableQuery<ApkDownloadEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.ApkDownloadTableName)
                               .ExecuteQuery(query).ToList<ApkDownloadEntity>();
        }

        private ApkDownloadStatsEntity GetApkDownloadStatsEntity(string partitionKey, string rowKey)
        {
            TableResult res = StorageModel.GetTable(StorageModel.ApkDownloadTableName)
                                          .Execute(TableOperation.Retrieve<ApkDownloadStatsEntity>(partitionKey, rowKey));

            return (null != res.Result) ? (ApkDownloadStatsEntity)res.Result : null;
        }

        private List<ApkDownloadStatsEntity> GetApkDownloadStatsEntityByPartition(string pKey)
        {
            TableQuery<ApkDownloadStatsEntity> query = new TableQuery<ApkDownloadStatsEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pKey));

            return StorageModel.GetTable(StorageModel.ApkDownloadStatsTableName)
                               .ExecuteQuery(query).ToList<ApkDownloadStatsEntity>();
        }

        // TODO: select all items from table.

        // Insert
        private void InsertApkDownloadEntity(ApkDownloadEntity entity)
        {
            StorageModel.GetTable(StorageModel.ApkDownloadTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        private void InsertApkDownloadStatsEntity(ApkDownloadStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ApkDownloadStatsTableName)
                        .Execute(TableOperation.Insert(entity));
        }

        // Delete
        private void DeleteApkDownloadEntity(ApkDownloadEntity entity)
        {
            StorageModel.GetTable(StorageModel.ApkDownloadTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        private void DeleteApkDownloadStatsEntity(ApkDownloadStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ApkDownloadStatsTableName)
                        .Execute(TableOperation.Delete(entity));
        }

        // Update
        private void UpdateApkDownloadEntity(ApkDownloadEntity entity)
        {
            StorageModel.GetTable(StorageModel.ApkDownloadTableName)
                        .Execute(TableOperation.Replace(entity));
        }

        private void UpdateApkDownloadStatsEntity(ApkDownloadStatsEntity entity)
        {
            StorageModel.GetTable(StorageModel.ApkDownloadStatsTableName)
                        .Execute(TableOperation.Replace(entity));
        }
    }

    public class ApkDownloadEntity : TableEntity
    {
        public ApkDownloadEntity() { }

        public ApkDownloadEntity(string source, string eventID)
        {
            this.PartitionKey = source;
            this.RowKey = eventID;
        }
    }

    public class ApkDownloadStatsEntity : TableEntity
    {
        public ApkDownloadStatsEntity() { }

        public ApkDownloadStatsEntity(string source, string eventID)
        {
            this.PartitionKey = source;
            this.RowKey = eventID;
        }

        public long TotalDownload { get; set; }
    }
}
