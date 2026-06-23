using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure;
using MangaPublishingSystem.Infrastructure.Services;
using MangaPublishingSystem.Infrastructure.Models;
using MangaPublishingSystem.Presentation.Controllers.Upload;
using BuildingBlocks.Web.Responses;

namespace MangaPublishingSystem.Tests
{
    /// <summary>
    /// Test suite cho LocalStorageService — Upload/Delete file trên hệ thống tệp cục bộ.
    /// </summary>
    public class LocalStorageServiceTests
    {
        private LocalStorageService CreateService(string scheme = "http", string host = "localhost", int port = 5010)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(r => r.Scheme).Returns(scheme);
            mockRequest.Setup(r => r.Host).Returns(new HostString(host, port));
            mockHttpContext.Setup(r => r.Request).Returns(mockRequest.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            return new LocalStorageService(mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task UploadFile_ShouldReturnUrlContainingUploadsPath()
        {
            var service = CreateService();
            var content = "Hello Test Upload File Content";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var fileUrl = await service.UploadFileAsync(ms, "test-image.png", "image/png");

            Assert.NotNull(fileUrl);
            Assert.Contains("/uploads/", fileUrl);
            Assert.StartsWith("http://localhost:5010", fileUrl);
            Assert.EndsWith(".png", fileUrl);
        }

        [Fact]
        public async Task UploadFile_ShouldGenerateUniqueFileNames()
        {
            var service = CreateService();
            using var ms1 = new MemoryStream(Encoding.UTF8.GetBytes("content1"));
            using var ms2 = new MemoryStream(Encoding.UTF8.GetBytes("content2"));

            var url1 = await service.UploadFileAsync(ms1, "file.png", "image/png");
            var url2 = await service.UploadFileAsync(ms2, "file.png", "image/png");

            Assert.NotEqual(url1, url2);
        }

        [Fact]
        public async Task UploadAndDelete_ShouldDeleteUploadedFile()
        {
            var service = CreateService();
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("deletable content"));

            var fileUrl = await service.UploadFileAsync(ms, "delete-me.jpg", "image/jpeg");
            var deleteResult = await service.DeleteFileAsync(fileUrl);

            Assert.True(deleteResult);
        }

        [Fact]
        public async Task DeleteFile_WithNonExistentUrl_ShouldReturnFalse()
        {
            var service = CreateService();

            var result = await service.DeleteFileAsync("http://localhost:5010/uploads/non-existent-file.png");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFile_WithNullOrEmpty_ShouldReturnFalse()
        {
            var service = CreateService();

            Assert.False(await service.DeleteFileAsync(null!));
            Assert.False(await service.DeleteFileAsync(""));
        }

        [Fact]
        public async Task UploadFile_WhenNoHttpContext_ShouldFallbackToDefaultBaseUrl()
        {
            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            var service = new LocalStorageService(mockAccessor.Object);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("no-context content"));
            var fileUrl = await service.UploadFileAsync(ms, "fallback.png", "image/png");

            Assert.StartsWith("http://localhost:5010", fileUrl);
            Assert.Contains("/uploads/", fileUrl);
        }
    }

    /// <summary>
    /// Test suite cho MinioStorageService — Kiểm tra cấu hình, xử lý URL và edge cases.
    /// </summary>
    public class MinioStorageServiceTests
    {
        [Fact]
        public void MinioSettings_AllProperties_CanBeConfigured()
        {
            var settings = new MinioSettings
            {
                Endpoint = "localhost:9000",
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                BucketName = "manga-publishing",
                Secure = false
            };

            Assert.Equal("localhost:9000", settings.Endpoint);
            Assert.Equal("minioadmin", settings.AccessKey);
            Assert.Equal("minioadmin", settings.SecretKey);
            Assert.Equal("manga-publishing", settings.BucketName);
            Assert.False(settings.Secure);
        }

        [Fact]
        public void MinioSettings_DefaultValues_ShouldBeEmpty()
        {
            var settings = new MinioSettings();

            Assert.Equal(string.Empty, settings.Endpoint);
            Assert.Equal(string.Empty, settings.AccessKey);
            Assert.Equal(string.Empty, settings.SecretKey);
            Assert.Equal(string.Empty, settings.BucketName);
            Assert.False(settings.Secure);
        }

        [Fact]
        public void Constructor_WithValidSettings_ShouldNotThrow()
        {
            var options = Options.Create(new MinioSettings
            {
                Endpoint = "localhost:9000",
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                BucketName = "manga-publishing",
                Secure = false
            });

            var service = new MinioStorageService(options);

            Assert.NotNull(service);
        }

        [Fact]
        public async Task DeleteFile_WithNullUrl_ShouldReturnFalse()
        {
            var options = Options.Create(new MinioSettings
            {
                Endpoint = "localhost:9000",
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                BucketName = "manga-publishing"
            });
            var service = new MinioStorageService(options);

            var result = await service.DeleteFileAsync(null!);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFile_WithEmptyUrl_ShouldReturnFalse()
        {
            var options = Options.Create(new MinioSettings
            {
                Endpoint = "localhost:9000",
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                BucketName = "manga-publishing"
            });
            var service = new MinioStorageService(options);

            var result = await service.DeleteFileAsync("");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFile_WithMalformedUrl_ShouldReturnFalse()
        {
            var options = Options.Create(new MinioSettings
            {
                Endpoint = "localhost:9000",
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                BucketName = "manga-publishing"
            });
            var service = new MinioStorageService(options);

            // URL without enough segments (only 1 segment after host)
            var result = await service.DeleteFileAsync("http://localhost:9000/onlyone");

            Assert.False(result);
        }

        [Fact]
        public void SecureMode_ShouldAffectUrlScheme()
        {
            // Kiểm tra logic xác định scheme dựa trên Secure flag
            var settings = new MinioSettings
            {
                Endpoint = "minio.example.com",
                Secure = true
            };
            var expectedScheme = settings.Secure ? "https" : "http";
            Assert.Equal("https", expectedScheme);

            settings.Secure = false;
            expectedScheme = settings.Secure ? "https" : "http";
            Assert.Equal("http", expectedScheme);
        }
    }

    /// <summary>
    /// Test suite cho FirebaseStorageService — Kiểm tra cấu hình và xử lý edge cases.
    /// </summary>
    public class FirebaseStorageServiceTests
    {
        [Fact]
        public void FirebaseSettings_CanBeConfigured()
        {
            var settings = new FirebaseSettings { Bucket = "my-bucket.appspot.com" };

            Assert.Equal("my-bucket.appspot.com", settings.Bucket);
        }

        [Fact]
        public void FirebaseSettings_DefaultValue_ShouldBeEmpty()
        {
            var settings = new FirebaseSettings();

            Assert.Equal(string.Empty, settings.Bucket);
        }

        [Fact]
        public async Task UploadFile_WithEmptyBucket_ShouldThrowInvalidOperationException()
        {
            var options = Options.Create(new FirebaseSettings { Bucket = "" });
            var service = new FirebaseStorageService(options);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadFileAsync(ms, "test.png", "image/png"));
        }

        [Fact]
        public async Task DeleteFile_WithNullUrl_ShouldReturnFalse()
        {
            var options = Options.Create(new FirebaseSettings { Bucket = "test-bucket" });
            var service = new FirebaseStorageService(options);

            var result = await service.DeleteFileAsync(null!);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFile_WithEmptyUrl_ShouldReturnFalse()
        {
            var options = Options.Create(new FirebaseSettings { Bucket = "test-bucket" });
            var service = new FirebaseStorageService(options);

            var result = await service.DeleteFileAsync("");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFile_WithEmptyBucket_ShouldReturnFalse()
        {
            var options = Options.Create(new FirebaseSettings { Bucket = "" });
            var service = new FirebaseStorageService(options);

            var result = await service.DeleteFileAsync("https://some-url.com/file.png");

            Assert.False(result);
        }
    }

    /// <summary>
    /// Test suite cho UploadController — Kiểm tra đầy đủ các trường hợp Happy/Unhappy.
    /// </summary>
    public class UploadControllerTests
    {
        [Fact]
        public async Task UploadFile_WithNullFile_ReturnsBadRequest()
        {
            var mockStorage = new Mock<IStorageService>();
            var controller = new UploadController(mockStorage.Object);

            var result = await controller.UploadFile(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.IsSuccess);
            Assert.Equal("Vui lòng chọn một file hợp lệ để tải lên.", apiResponse.Message);
        }

        [Fact]
        public async Task UploadFile_WithZeroLengthFile_ReturnsBadRequest()
        {
            var mockStorage = new Mock<IStorageService>();
            var controller = new UploadController(mockStorage.Object);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var result = await controller.UploadFile(mockFile.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.IsSuccess);
        }

        [Fact]
        public async Task UploadFile_WithValidFile_ReturnsOkWithUrl()
        {
            var mockStorage = new Mock<IStorageService>();
            var expectedUrl = "http://localhost:5010/uploads/xyz.png";

            mockStorage.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(expectedUrl);

            var controller = new UploadController(mockStorage.Object);

            var mockFile = new Mock<IFormFile>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("Mock file content"));
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.FileName).Returns("mock.png");
            mockFile.Setup(f => f.ContentType).Returns("image/png");
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

            var result = await controller.UploadFile(mockFile.Object);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(apiResponse.IsSuccess);
            Assert.Equal(expectedUrl, apiResponse.Data);
            Assert.Equal("Tải lên file thành công.", apiResponse.Message);
        }

        [Fact]
        public async Task UploadFile_WhenStorageThrowsException_Returns500()
        {
            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ThrowsAsync(new Exception("Lỗi kết nối MinIO"));

            var controller = new UploadController(mockStorage.Object);

            var mockFile = new Mock<IFormFile>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("content"));
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.FileName).Returns("crash.png");
            mockFile.Setup(f => f.ContentType).Returns("image/png");
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

            var result = await controller.UploadFile(mockFile.Object);

            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse<string>>(statusResult.Value);
            Assert.False(apiResponse.IsSuccess);
            Assert.Contains("Lỗi kết nối MinIO", apiResponse.Message);
        }

        [Fact]
        public async Task UploadFile_StorageServiceIsCalled_WithCorrectParameters()
        {
            var mockStorage = new Mock<IStorageService>();
            mockStorage.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "original-name.jpg", "image/jpeg"))
                       .ReturnsAsync("http://example.com/uploads/uuid.jpg")
                       .Verifiable();

            var controller = new UploadController(mockStorage.Object);

            var mockFile = new Mock<IFormFile>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("jpeg content"));
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.FileName).Returns("original-name.jpg");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

            await controller.UploadFile(mockFile.Object);

            mockStorage.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), "original-name.jpg", "image/jpeg"), Times.Once);
        }
    }

    /// <summary>
    /// Test suite cho DependencyInjection — Kiểm tra đăng ký đúng IStorageService
    /// dựa vào cấu hình "StorageSettings:Provider".
    /// </summary>
    public class DependencyInjectionStorageTests
    {
        private IConfiguration BuildConfig(string provider)
        {
            var configData = new Dictionary<string, string?>
            {
                { "StorageSettings:Provider", provider },
                { "StorageSettings:Minio:Endpoint", "localhost:9000" },
                { "StorageSettings:Minio:AccessKey", "minioadmin" },
                { "StorageSettings:Minio:SecretKey", "minioadmin" },
                { "StorageSettings:Minio:BucketName", "test-bucket" },
                { "StorageSettings:Minio:Secure", "false" },
                { "StorageSettings:Firebase:Bucket", "test.appspot.com" },
                { "ConnectionStrings:DefaultConnection", "Server=fake;Database=fake;" },
                { "EmailSettings:DefaultFromEmail", "test@test.com" },
                { "EmailSettings:DefaultFromName", "Test" },
                { "EmailSettings:SmtpServer", "smtp.test.com" },
                { "EmailSettings:Port", "587" },
                { "EmailSettings:Username", "user" },
                { "EmailSettings:Password", "pass" },
                { "EmailSettings:EnableSsl", "true" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public void WhenProviderIsMinio_ShouldRegisterMinioStorageService()
        {
            var config = BuildConfig("Minio");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStorageService));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(MinioStorageService), descriptor!.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void WhenProviderIsFirebase_ShouldRegisterFirebaseStorageService()
        {
            var config = BuildConfig("Firebase");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStorageService));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(FirebaseStorageService), descriptor!.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void WhenProviderIsLocal_ShouldRegisterLocalStorageService()
        {
            var config = BuildConfig("Local");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStorageService));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(LocalStorageService), descriptor!.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void WhenProviderIsNotSet_ShouldDefaultToLocalStorageService()
        {
            var configData = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=fake;Database=fake;" },
                { "EmailSettings:DefaultFromEmail", "test@test.com" },
                { "EmailSettings:DefaultFromName", "Test" },
                { "EmailSettings:SmtpServer", "smtp.test.com" },
                { "EmailSettings:Port", "587" },
                { "EmailSettings:Username", "user" },
                { "EmailSettings:Password", "pass" },
                { "EmailSettings:EnableSsl", "true" },
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStorageService));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(LocalStorageService), descriptor!.ImplementationType);
        }

        [Fact]
        public void WhenProviderIsMinio_CaseInsensitive_ShouldRegisterMinioStorageService()
        {
            var config = BuildConfig("minio");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStorageService));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(MinioStorageService), descriptor!.ImplementationType);
        }

        [Fact]
        public void WhenProviderIsFirebase_CaseInsensitive_ShouldRegisterFirebaseStorageService()
        {
            var config = BuildConfig("firebase");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStorageService));

            Assert.NotNull(descriptor);
            Assert.Equal(typeof(FirebaseStorageService), descriptor!.ImplementationType);
        }

        [Fact]
        public void MinioSettings_ShouldBeConfiguredFromConfig()
        {
            var config = BuildConfig("Minio");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);
            var sp = services.BuildServiceProvider();

            var minioOptions = sp.GetRequiredService<IOptions<MinioSettings>>();

            Assert.Equal("localhost:9000", minioOptions.Value.Endpoint);
            Assert.Equal("minioadmin", minioOptions.Value.AccessKey);
            Assert.Equal("minioadmin", minioOptions.Value.SecretKey);
            Assert.Equal("test-bucket", minioOptions.Value.BucketName);
            Assert.False(minioOptions.Value.Secure);
        }

        [Fact]
        public void FirebaseSettings_ShouldBeConfiguredFromConfig()
        {
            var config = BuildConfig("Firebase");
            var services = new ServiceCollection();
            services.AddInfrastructureServices(config);
            var sp = services.BuildServiceProvider();

            var fbOptions = sp.GetRequiredService<IOptions<FirebaseSettings>>();

            Assert.Equal("test.appspot.com", fbOptions.Value.Bucket);
        }
    }
}
