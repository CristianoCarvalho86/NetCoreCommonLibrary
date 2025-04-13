using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using NetCoreCommonLibrary.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("S3 bucket name is not configured");
        }

        public async Task<string> UploadFileAsync(string key, Stream fileStream, string contentType)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);
            return GetFileUrl(key);
        }

        public async Task<byte[]> DownloadFileAsync(string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<bool> DeleteFileAsync(string key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public string GetFileUrl(string key)
        {
            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }
    }
} 