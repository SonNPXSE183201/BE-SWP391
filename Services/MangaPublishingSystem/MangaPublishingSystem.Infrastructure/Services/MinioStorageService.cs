using System;
using System.IO;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace MangaPublishingSystem.Infrastructure.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _settings;
        private bool _bucketReady;

        public MinioStorageService(IOptions<MinioSettings> settings)
        {
            _settings = settings.Value;

            var builder = new MinioClient()
                .WithEndpoint(_settings.Endpoint)
                .WithCredentials(_settings.AccessKey, _settings.SecretKey);

            if (_settings.Secure)
            {
                builder = builder.WithSSL();
            }

            _minioClient = builder.Build();
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);

            await EnsureBucketReadyAsync().ConfigureAwait(false);

            // Upload file
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(uniqueFileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

            // Trả về url truy cập tệp
            var scheme = _settings.Secure ? "https" : "http";
            return $"{scheme}://{_settings.Endpoint}/{_settings.BucketName}/{uniqueFileName}";
        }

        private async Task EnsureBucketReadyAsync()
        {
            if (_bucketReady) return;

            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
            var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs).ConfigureAwait(false);
            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs).ConfigureAwait(false);
            }

            // Dev: cho phép browser đọc file qua URL trực tiếp (img src)
            var policy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [
                    {
                      "Effect": "Allow",
                      "Principal": {"AWS": ["*"]},
                      "Action": ["s3:GetObject"],
                      "Resource": ["arn:aws:s3:::{{_settings.BucketName}}/*"]
                    }
                  ]
                }
                """;
            var setPolicyArgs = new SetPolicyArgs()
                .WithBucket(_settings.BucketName)
                .WithPolicy(policy);
            await _minioClient.SetPolicyAsync(setPolicyArgs).ConfigureAwait(false);

            _bucketReady = true;
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl)) return false;

                // Trích xuất tên object từ URL
                var uri = new Uri(fileUrl);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length < 2) return false;

                // Segment cuối là tên file
                var objectName = segments[segments.Length - 1];

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
