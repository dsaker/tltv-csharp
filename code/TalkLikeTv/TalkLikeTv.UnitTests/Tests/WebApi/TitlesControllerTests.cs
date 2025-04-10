using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;
using TalkLikeTv.WebApi.Controllers;
using TalkLikeTv.WebApi.Models;

namespace TalkLikeTv.UnitTests.Tests.WebApi
{
    public class TitlesControllerTests
    {
        private readonly Mock<ITitleRepository> _mockRepo;
        private readonly Mock<ITitleValidationService> _mockValidationService;
        private readonly TitlesController _controller;

        public TitlesControllerTests()
        {
            _mockRepo = new Mock<ITitleRepository>();
            _mockValidationService = new Mock<ITitleValidationService>();
            _controller = new TitlesController(_mockRepo.Object, _mockValidationService.Object);
        }

        [Fact]
        public async Task GetTitles_ReturnsAllTitles_WhenNoFilter()
        {
            // Arrange
            var titles = new[] { new Title { TitleId = 1, TitleName = "Test Title" } };
            _mockRepo.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(titles);

            // Act
            var result = await _controller.GetTitles(null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Title>>>(result);
            Assert.Equal(titles, actionResult.Value);
        }

        [Fact]
        public async Task GetTitles_ReturnsFilteredTitles_WhenOriginalLanguageIdProvided()
        {
            // Arrange
            var titles = new[] {
                new Title { TitleId = 1, TitleName = "Title 1", OriginalLanguageId = 1 },
                new Title { TitleId = 2, TitleName = "Title 2", OriginalLanguageId = 2 }
            };
            _mockRepo.Setup(repo => repo.RetrieveAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(titles);

            // Act
            var result = await _controller.GetTitles("1");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Title>>>(result);
            var filteredTitles = actionResult.Value?.ToArray();
            Assert.NotNull(filteredTitles);
            Assert.Single(filteredTitles);
            Assert.Equal(1, filteredTitles[0].TitleId);
        }

        [Fact]
        public async Task GetTitles_ReturnsBadRequest_WhenInvalidLanguageIdFormat()
        {
            // Act
            var result = await _controller.GetTitles("invalid");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task SearchTitles_ReturnsPagedResults()
        {
            // Arrange
            var titles = new Title[] { new Title { TitleId = 1, TitleName = "Test" } };
            var returnValue = (titles, totalCount: 1);

            _mockRepo.Setup(repo => repo.SearchTitlesAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnValue);

            // Act
            var result = await _controller.SearchTitles("1", "test");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<PaginatedResult<Title>>(okResult.Value);
            Assert.Equal(titles, pagedResult.Items);
            Assert.Equal(1, pagedResult.TotalCount);
            Assert.Equal(1, pagedResult.PageNumber);
            Assert.Equal(10, pagedResult.PageSize);
        }

        [Fact]
        public async Task GetTitle_ReturnsTitle_WhenFound()
        {
            // Arrange
            var title = new Title { TitleId = 1, TitleName = "Test Title" };
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(title);

            // Act
            var result = await _controller.GetTitle("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(title, okResult.Value);
        }

        [Fact]
        public async Task GetTitle_Returns404_WhenNotFound()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "999"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            // Act
            var result = await _controller.GetTitle("999");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtRoute_WhenValidTitleSubmitted()
        {
            // Arrange
            var title = new Title { TitleName = "New Title", OriginalLanguageId = 1 };
            var createdTitle = new Title { TitleId = 1, TitleName = "New Title", OriginalLanguageId = 1 };

            var errors = new List<string>();
            var validationResult = (IsValid: true, Errors: errors);
            _mockValidationService.Setup(service => service.ValidateAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _mockRepo.Setup(repo => repo.CreateAsync(
                    It.IsAny<Title>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTitle);

            // Act
            var result = await _controller.Create(title);

            // Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetTitle", createdAtRouteResult.RouteName);
            Assert.Equal("1", createdAtRouteResult.RouteValues["id"]);
            Assert.Equal(createdTitle, createdAtRouteResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("TitleName", "Required");
            var title = new Title();

            // Act
            var result = await _controller.Create(title);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var title = new Title { TitleName = "New Title" };
            var errorsList = new List<string> { "Original language is required" };
            var validationResult = (IsValid: false, Errors: errorsList);

            _mockValidationService.Setup(service => service.ValidateAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.Create(title);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorsList, badRequestResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            var title = new Title { TitleName = "New Title", OriginalLanguageId = 1 };

            var errors = new List<string>();
            var validationResult = (IsValid: true, Errors: errors);
            _mockValidationService.Setup(service => service.ValidateAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _mockRepo.Setup(repo => repo.CreateAsync(
                    It.IsAny<Title>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Create(title);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var title = new Title { TitleId = 1, TitleName = "Updated Title" };
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(title);
            
            _mockRepo.Setup(repo => repo.UpdateAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<Title>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Update("1", title);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var title = new Title { TitleId = 2, TitleName = "Title" };

            // Act
            var result = await _controller.Update("1", title);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenTitleDoesNotExist()
        {
            // Arrange
            var title = new Title { TitleId = 999, TitleName = "Title" };
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "999"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            // Act
            var result = await _controller.Update("999", title);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var title = new Title { TitleId = 1, TitleName = "Title" };
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(title);
            
            _mockRepo.Setup(repo => repo.DeleteAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenTitleDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "999"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            // Act
            var result = await _controller.Delete("999");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenDeleteFails()
        {
            // Arrange
            var title = new Title { TitleId = 1, TitleName = "Title" };
            _mockRepo.Setup(repo => repo.RetrieveAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(title);
            
            _mockRepo.Setup(repo => repo.DeleteAsync(
                    It.Is<string>(s => s == "1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}