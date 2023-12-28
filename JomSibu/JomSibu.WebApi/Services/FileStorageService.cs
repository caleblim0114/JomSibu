using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace JomSibu.WebApi.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns>file url</returns>
        Task<string> CreateFileAsync(string prefix, string fileName, Stream stream, string? contentType, bool useCdn = false);
        Task DeleteFileIfExistsAsync(string filePath);
        Task<Stream> GetFileStreamAsync(string filePath);
    }

    public class AzureBlobStorageService : IFileStorageService
    {
        //todo change the container name
        private string _containerName = "ecopro";
        private IConfiguration _configuration;
        private BlobContainerClient GetBlobContainerClient()
        {
            return new BlobContainerClient(_configuration.GetConnectionString("BlobStorageConnectionString"), _containerName);
        }
        public AzureBlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreateFileAsync(string prefix, string fileName, Stream stream, string? contentType, bool useCdn = false)
        {
            var container = GetBlobContainerClient();
            var fileNameOnAzure = $"{prefix}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            BlobClient blob = container.GetBlobClient(fileNameOnAzure);

            if (contentType == null) 
                contentType = "";
            var headers = new BlobHttpHeaders()
            {
                ContentType = contentType,
            };
            await blob.UploadAsync(stream, headers);
            var baseUrl = container.Uri.ToString();
            if (useCdn)
            {
                baseUrl = baseUrl.Replace("blob.core.windows", "azureedge");
            }
            return $"{baseUrl}/{fileNameOnAzure}";
        }

        public async Task DeleteFileIfExistsAsync(string filePath)
        {
            var container = GetBlobContainerClient();
            //var uri = new Uri(filePath);
            //var fileName = Path.GetFileName(uri.AbsolutePath);
            //BlobClient blockBlob = container.GetBlobClient(fileName);
            //await blockBlob.DeleteIfExistsAsync();
            if (filePath.Contains($"{_containerName}/ "))
            {
                BlobClient blobToDelete = container.GetBlobClient(filePath.Split($"{_containerName}/")[1]);
                await blobToDelete.DeleteIfExistsAsync();
            }
        }

        public Task<Stream> GetFileStreamAsync(string filePath)
        {
            var container = GetBlobContainerClient();
            if (filePath.Contains($"{_containerName}/"))
            {
                BlobClient blobToDelete = container.GetBlobClient(filePath.Split($"{_containerName}/")[1]);
                return blobToDelete.OpenReadAsync();
            }

            return Task.FromResult(Stream.Null);
        }
    }

    public class LocalStorageService : IFileStorageService
    {
        public async Task<string> CreateFileAsync(string prefix, string fileName, Stream stream, string? contentType, bool useCdn = false)
        {
            prefix = prefix.Replace("/", @"\");
            var directories = prefix.Split(@"\");
            if (directories.Length > 1)
            {
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", prefix);
                System.IO.Directory.CreateDirectory(directoryPath);
            }
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", $"{prefix}_{Guid.NewGuid()}{Path.GetExtension(fileName)}");
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            filePath = filePath.Replace(@"C:\home\site\wwwroot\wwwroot\", @"https://smcrecyclewebapi.azurewebsites.net/");
            filePath = filePath.Replace(@"D:\home\site\wwwroot\wwwroot\", @"https://smcrecyclewebapi.azurewebsites.net/");
            filePath = filePath.Replace(@"\", @"/");
            return filePath;
        }

        public Task DeleteFileIfExistsAsync(string filePath)
        {
            filePath = filePath.Replace(@"https://smcrecyclewebapi.azurewebsites.net/", @"C:\home\site\wwwroot\wwwroot\");
            filePath = filePath.Replace(@"/", @"\");
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            filePath = filePath.Replace(@"https://smcrecyclewebapi.azurewebsites.net/", @"C:\home\site\wwwroot\wwwroot\");
            filePath = filePath.Replace(@"/", @"\");
            if (System.IO.File.Exists(filePath))
            {
                return System.IO.File.OpenRead(filePath);
            }

            return Stream.Null;
        }
    }
}
