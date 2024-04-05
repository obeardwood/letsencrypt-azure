using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetsEncrypt.Azure.Core.V2
{
    public class AzureBlobStorage : IFileSystem
    {
        private BlobServiceClient client;

        public AzureBlobStorage(string connectionString)
        {
            this.client = new BlobServiceClient(connectionString);
        }

        public async Task<bool> Exists(string v)
        {
            BlockBlobClient blob = await GetBlob(v);
            return await blob.ExistsAsync();
        }

        private async Task<BlockBlobClient> GetBlob(string v)
        {
            var container = client.GetBlobContainerClient("letsencrypt");
            await container.CreateIfNotExistsAsync();            
            return container.GetBlockBlobClient(v);
        }

        public async Task<string> ReadAllText(string v)
        {
            var blob = await GetBlob(v);
            return (await blob.DownloadContentAsync()).Value?.Content.ToString() ?? string.Empty; 
        }

        public async Task WriteAllText(string v, string pemKey)
        {
            var blob = await GetBlob(v);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pemKey)))
            {
                await blob.UploadAsync(stream);
            }
        }

        public async Task<byte[]> Read(string v)
        {
            var blob = await GetBlob(v);
            using (var ms = new MemoryStream())
            using (var data = await blob.OpenReadAsync())
            {
                await data.CopyToAsync(ms);
                return ms.ToArray();
            }           
        }

        public async Task Write(string v, byte[] data)
        {
            var blob = await GetBlob(v);
            using (var ms = new MemoryStream(data))
                await blob.UploadAsync(ms);
        }
    }
}
