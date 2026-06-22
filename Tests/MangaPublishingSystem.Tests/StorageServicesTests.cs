using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure.Services;
using MangaPublishingSystem.Infrastructure.Models;
using MangaPublishingSystem.Presentation.Controllers.Upload;
using BuildingBlocks.Web.Responses;

namespace MangaPublishingSystem.Tests
{
    public class StorageServicesTests
    {
        [Fact]
        public async Task LocalStorageService_UploadAndMockDelete_ShouldWork()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            
            mockRequest.Setup(r => r.Scheme).Returns("http");
            mockRequest.Setup(r => r.Host).Returns(new HostString("localhost", 5010));
            mockHttpContext.Setup(r => r.Request).Returns(mockRequest.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var storageService = new LocalStorageService(mockHttpContextAccessor.Object);
            var content = "Hello Test Upload File Content";
            var fileName = "test-image.png";
            var contentType = "image/png";
            
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act: Upload File
            var fileUrl = await storageService.UploadFileAsync(ms, fileName, contentType);

            // Assert Upload
            Assert.NotNull(fileUrl);
            Assert.Contains("/uploads/", fileUrl);
            Assert.StartsWith("http://localhost:5010", fileUrl);

            // Act: Delete File
            var deleteResult = await storageService.DeleteFileAsync(fileUrl);

            // Assert Delete
            Assert.True(deleteResult);
        }

        [Fact]
        public async Task UploadController_WithNullFile_ReturnsBadRequest()
        {
            // Arrange
            var mockStorage = new Mock<IStorageService>();
            var controller = new UploadController(mockStorage.Object);

            // Act
            var result = await controller.UploadFile(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.IsSuccess);
            Assert.Equal("Vui lòng chọn một file hợp lệ để tải lên.", apiResponse.Message);
        }

        [Fact]
        public async Task UploadController_WithValidFile_ReturnsSuccessUrl()
        {
            // Arrange
            var mockStorage = new Mock<IStorageService>();
            var expectedUrl = "http://localhost:5010/uploads/xyz.png";
            
            mockStorage.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(expectedUrl);

            var controller = new UploadController(mockStorage.Object);

            var mockFile = new Mock<IFormFile>();
            var content = "Mock file content";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.FileName).Returns("mock.png");
            mockFile.Setup(f => f.ContentType).Returns("image/png");
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

            // Act
            var result = await controller.UploadFile(mockFile.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(apiResponse.IsSuccess);
            Assert.Equal(expectedUrl, apiResponse.Data);
            Assert.Equal("Tải lên file thành công.", apiResponse.Message);
        }

        [Fact]
        public void MinioSettings_CanBeConfigured()
        {
            // Arrange & Act
            var settings = new MinioSettings 
            { 
                Endpoint = "localhost:9000",
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                BucketName = "manga-publishing",
                Secure = false
            };

            // Assert
            Assert.Equal("localhost:9000", settings.Endpoint);
            Assert.Equal("minioadmin", settings.AccessKey);
            Assert.Equal("minioadmin", settings.SecretKey);
            Assert.Equal("manga-publishing", settings.BucketName);
            Assert.False(settings.Secure);
        }

        [Fact]
        public void FirebaseSettings_CanBeConfigured()
        {
            // Arrange & Act
            var settings = new FirebaseSettings { Bucket = "my-bucket.appspot.com" };

            // Assert
            Assert.Equal("my-bucket.appspot.com", settings.Bucket);
        }
    }
}
