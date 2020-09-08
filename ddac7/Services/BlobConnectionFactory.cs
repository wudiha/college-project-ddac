using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace ddac7.Services
{
    public interface IAzureBlobConnectionFactory
    {
        Task<CloudBlobContainer> GetBlobContainer();
        Task<CloudBlobContainer> GetBlobContainer2();
    }

    public class AzureBlobConnectionFactory : IAzureBlobConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private CloudBlobContainer _blobContainer2;

        public AzureBlobConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<CloudBlobContainer> GetBlobContainer()
        {
            if (_blobContainer != null)
                return _blobContainer;

            var containerName = _configuration.GetValue<string>("ContainerName");
            var containerName2 = _configuration.GetValue<string>("ContainerName2");
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("Configuration must contain ContainerName");
            }

            var blobClient = GetClient();

            _blobContainer = blobClient.GetContainerReference(containerName);
            _blobContainer2 = blobClient.GetContainerReference(containerName2);
            if (await _blobContainer.CreateIfNotExistsAsync())
            {
                await _blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            return _blobContainer;
        }
        public async Task<CloudBlobContainer> GetBlobContainer2()
        {
            if (_blobContainer2 != null)
                return _blobContainer2;

            var containerName = _configuration.GetValue<string>("ContainerName2");
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("Configuration must contain ContainerName");
            }

            var blobClient = GetClient();

            _blobContainer = blobClient.GetContainerReference(containerName);
            if (await _blobContainer.CreateIfNotExistsAsync())
            {
                await _blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            return _blobContainer;
        }
        private CloudBlobClient GetClient()
        {
            if (_blobClient != null)
                return _blobClient;

            var storageConnectionString = _configuration.GetValue<string>("StorageConnectionString");
            if (string.IsNullOrWhiteSpace(storageConnectionString))
            {
                throw new ArgumentException("Configuration must contain StorageConnectionString");
            }

            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                throw new Exception("Could not create storage account with StorageConnectionString configuration");
            }

            _blobClient = storageAccount.CreateCloudBlobClient();
            return _blobClient;
        }
    }
}
