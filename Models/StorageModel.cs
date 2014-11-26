using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System;
using System.Web;

namespace meitubikanSite.Models
{
    public class StorageModel
    {
        // *** Section to regesitor all storage item name ***

        // ** Image Model **
        // All letters in container name must be lowercase
        public static readonly string ImageContainerName = "meitubikanimages";

        // ** Apk Model **
        public static readonly string ApkDownloadTableName = "ApkDownload";
        public static readonly string ApkDownloadStatsTableName = "ApkDownloadStats";
        public static readonly string ApkContainerName = "app";
        public static readonly string ApkFileName = "meitubikan.apk";

        // ** User Model **
        public static readonly string UserActionTableName = "UserAction";
        public static readonly string ImageStatsTableName = "ImageStats";
        public static readonly string ImageDailyStatsTableName = "ImageDailyStats";
        public static readonly string UserSearchTableName = "UserSearch";
        public static readonly string QueryStatsTableName = "QueryStats";
        public static readonly string QueryDailyStatsTableName = "QueryDailyStats";

        // *** End of storage item name section ***

        // *** Helper functions ***

        // Event id by time and random value
        public static string CreateEventId()
        {
            return "E" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
                + "R" + (new Random()).Next(999999).ToString("000000");
        }

        // Get daily date string
        public static string GetDailyDateString()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }

        // Url encode
        public static string UrlEncode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        // Url decode
        public static string UrlDecode(string encodedUrl)
        {
            return HttpUtility.UrlDecode(encodedUrl);
        }

        // *** End of helper functions ***

        private static CloudStorageAccount StorageAccount { get; set; }

        private static CloudTableClient TableClient { get; set; }

        private static CloudBlobClient BlobClient { get; set; }

        private static Dictionary<string, CloudTable> CloudTableCache = new Dictionary<string, CloudTable>();

        private static Dictionary<string, CloudBlobContainer> CloudBlobContainerCache = new Dictionary<string, CloudBlobContainer>();

        static StorageModel()
        {
            // Retrieve the storage account from the connection string.
            StorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager
                                                .GetSetting(CloudConfigurationManager
                                                .GetSetting("StorageConnectionSelector")));
            // Create the table/blob client.
            TableClient = StorageAccount.CreateCloudTableClient();
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        public static CloudTable GetTable(string tableName)
        {
            CloudTable table = null;

            if (CloudTableCache.ContainsKey(tableName))
            {
                table = CloudTableCache[tableName];
            }
            else
            {
                table = TableClient.GetTableReference(tableName);

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                CloudTableCache.Add(tableName, table);
            }

            return table;
        }

        public static CloudBlobContainer GetBlobContainer(string containerName)
        {
            CloudBlobContainer container = null;

            if (CloudBlobContainerCache.ContainsKey(containerName))
            {
                container = CloudBlobContainerCache[containerName];
            }
            else
            {
                container = BlobClient.GetContainerReference(containerName);

                // Create the table if it doesn't exist.
                container.CreateIfNotExists();

                container.SetPermissions(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

                CloudBlobContainerCache.Add(containerName, container);
            }

            return container;
        }

        public static string GetBlobEndPoint()
        {
            return StorageAccount.BlobEndpoint.ToString();
        }
    }
}
