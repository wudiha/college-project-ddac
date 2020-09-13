using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ddac7.Services
{



    public interface IBlobService
    {
        Task<IEnumerable<Uri>> ListAsync(String frm, Models.BlobModel doctor);
        Task UploadAsync(IFormFileCollection files);
        Task UploadAsync2(IFormFileCollection files);
        Task DeleteAsync(string fileUri);
        Task DeleteAsync2(string fileUri);
        Task DeleteAllAsync();

    }

    public class AzureBlobService : IBlobService
    {
        private readonly IAzureBlobConnectionFactory _azureBlobConnectionFactory;
        public static String blob_files;
        public static String temp;
        public static Stream Stream;
        public AzureBlobService(IAzureBlobConnectionFactory azureBlobConnectionFactory)
        {
            _azureBlobConnectionFactory = azureBlobConnectionFactory;
        }

        public async Task DeleteAllAsync()
        {
            var blobContainer = await _azureBlobConnectionFactory.GetBlobContainer();

            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var response = await blobContainer.ListBlobsSegmentedAsync(blobContinuationToken); //not overload
                foreach (IListBlobItem blob in response.Results)
                {
                    if (blob.GetType() == typeof(CloudBlockBlob))
                        await ((CloudBlockBlob)blob).DeleteIfExistsAsync();
                }
                blobContinuationToken = response.ContinuationToken;
            } while (blobContinuationToken != null);
        }

        public async Task DeleteAsync(string fileUri)
        {
            var blobContainer = await _azureBlobConnectionFactory.GetBlobContainer();

            Uri uri = new Uri(fileUri);
            string filename = Path.GetFileName(uri.LocalPath);

            var blob = blobContainer.GetBlockBlobReference(filename);
            await blob.DeleteIfExistsAsync();
        }
        public async Task DeleteAsync2(string fileUri)
        {
            var blobContainer = await _azureBlobConnectionFactory.GetBlobContainer2();

            Uri uri = new Uri(fileUri);
            string filename = Path.GetFileName(uri.LocalPath);

            var blob = blobContainer.GetBlockBlobReference(filename);
            await blob.DeleteIfExistsAsync();
        }
        public async Task<IEnumerable<Uri>> ListAsync(String frm, Models.BlobModel doctor)
        {

            IEnumerable<string> file = null;
             var blobContainer = await _azureBlobConnectionFactory.GetBlobContainer();
            if (frm.Equals("view"))
            {
                blobContainer = await _azureBlobConnectionFactory.GetBlobContainer2();
                file = doctor.doctor.Select(x => x.imgurl);
            }
            var allBlobs = new List<Uri>();
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var response = await blobContainer.ListBlobsSegmentedAsync(blobContinuationToken);
                foreach (IListBlobItem blob in response.Results)
                {

                    if (blob.GetType() == typeof(CloudBlockBlob))
                    {
                        var filename = ((CloudBlob)blob).Name;
                        if (frm.Equals("view"))
                        {
                            foreach(string url in file)
                                if(url.Equals(filename))
                                allBlobs.Add(blob.Uri);
                        }
                        else if (frm.Equals("add")) allBlobs.Add(blob.Uri);

                    }
                }
                blobContinuationToken = response.ContinuationToken;
            } while (blobContinuationToken != null);
            return allBlobs;
        }

        public async Task UploadAsync(IFormFileCollection files)
        {


            var blobContainer = await _azureBlobConnectionFactory.GetBlobContainer();
            var blobContainer2 = await _azureBlobConnectionFactory.GetBlobContainer2();

            for (int i = 0; i < files.Count; i++)
            {
                blob_files = GetRandomBlobName(files[i].FileName);
                var blob = blobContainer.GetBlockBlobReference(blob_files);
                var blob2 = blobContainer2.GetBlockBlobReference(blob_files);
                using (var stream = files[i].OpenReadStream())
                {
                    Stream = stream;
                    await blob.UploadFromStreamAsync(stream);
                }
                using (var stream = files[i].OpenReadStream())
                {
                    Stream = stream;
                    await blob2.UploadFromStreamAsync(stream);
                }
            }
        }
        public async Task UploadAsync2(IFormFileCollection files)
        {


            var blobContainer = await _azureBlobConnectionFactory.GetBlobContainer2();

            for (int i = 0; i < files.Count; i++)
            {

                var blob = blobContainer.GetBlockBlobReference(blob_files);


                await blob.UploadFromStreamAsync(Stream);


            }
        }
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }

}
