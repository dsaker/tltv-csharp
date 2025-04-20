using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Abstractions;
using TalkLikeTv.WebApi.Controllers;
using TalkLikeTv.WebApi.Models;
using TalkLikeTv.WebApi.Mappers;

namespace TalkLikeTv.UnitTests.Tests.WebApi;

public class TitlesControllerTests
    {
        private readonly Mock<ITitleRepository> _mockRepo;
        private readonly Mock<IAudioFileService> _mockAudioFileService;
        private readonly Mock<IAudioProcessingService> _mockAudioProcessingService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly TitlesController _controller;

        public TitlesControllerTests()
        {
            _mockRepo = new Mock<ITitleRepository>();
            _mockAudioFileService = new Mock<IAudioFileService>();
            _mockAudioProcessingService = new Mock<IAudioProcessingService>();
            _mockTokenService = new Mock<ITokenService>();
            var mockLogger = new Mock<ILogger<AudioController>>();
    
            _controller = new TitlesController(
                _mockRepo.Object,
                _mockAudioFileService.Object,
                _mockAudioProcessingService.Object,
                _mockTokenService.Object,
                mockLogger.Object);

            // Set up HttpContext with CancellationToken
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
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
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var titleResponses = Assert.IsAssignableFrom<IEnumerable<TitleMapper.TitleResponse>>(actionResult.Value);
    
            // Compare by ID - verify mapping was done correctly
            Assert.Equal(
                titles.Select(t => t.TitleId), 
                titleResponses.Select(tr => tr.TitleId)
            );
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
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var titleResponses = Assert.IsAssignableFrom<IEnumerable<TitleMapper.TitleResponse>>(actionResult.Value);
    
            Assert.Single(titleResponses);
            Assert.Equal(1, titleResponses.First().TitleId);
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
            var titles = new [] { new Title { TitleId = 1, TitleName = "Test" } };
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
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
            Assert.Contains("Title with ID 999 was not found.", errorResponse.Errors);
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
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("The ID in the URL does not match the ID in the request body.", errorResponse.Errors);
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
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
            Assert.Contains("Title with ID 999 was not found.", errorResponse.Errors);
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
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
            Assert.Contains("Title with ID 999 was not found.", errorResponse.Errors);
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
        
        [Fact]
        public async Task CreateTitleFromFile_ReturnsTitle_WhenSuccessful()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "validToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            // Fix 1: Define token result as the correct type
            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = true, ErrorMessage = null });

            // Fix 2: Create a proper ExtractAndValidateResult object
            var extractResult = new IAudioFileService.ExtractAndValidateResult
            {
                PhraseStrings = new List<string> { "Phrase 1", "Phrase 2" },
                Errors = new List<string>()
            };
            _mockAudioFileService.Setup(s => s.ExtractAndValidatePhraseStrings(It.IsAny<IFormFile>()))
                .Returns(extractResult);

            // Fix 3: Use tuple syntax compatible with Moq
            var language = new Language { LanguageId = 1, Tag = "en", Name = "English" };
            _mockAudioProcessingService.Setup(s => s.DetectLanguageAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((language, new List<string>()));

            var title = new Title { TitleId = 1, TitleName = "Test Title", Description = "Test Description" };
            _mockAudioProcessingService.Setup(s => s.ProcessTitleAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<Language>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(title);

            // Fix 4: Use proper tuple return type compatible with Moq
            _mockAudioProcessingService.Setup(s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, new List<string>()));
            
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTitle = Assert.IsType<Title>(okResult.Value);
            Assert.Equal(title.TitleId, returnedTitle.TitleId);
            Assert.Equal(title.TitleName, returnedTitle.TitleName);
        }
        
        [Fact]
        public async Task CreateTitleFromFile_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "invalidToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = false, ErrorMessage = "Invalid token" });

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Invalid token", errorResponse.Errors.First());
        }

        [Fact]
        public async Task CreateTitleFromFile_ReturnsBadRequest_WhenFileContainsErrors()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "validToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = true, ErrorMessage = null });

            var extractResult = new IAudioFileService.ExtractAndValidateResult
            {
                PhraseStrings = new List<string>(),
                Errors = new List<string> { "Invalid file format", "File is empty" }
            };

            _mockAudioFileService.Setup(s => s.ExtractAndValidatePhraseStrings(It.IsAny<IFormFile>()))
                .Returns(extractResult);

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("Invalid file format", errorResponse.Errors);
            Assert.Contains("File is empty", errorResponse.Errors);
        }

        [Fact]
        public async Task CreateTitleFromFile_ReturnsServerError_WhenLanguageDetectionFails()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "validToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = true, ErrorMessage = null });

            var extractResult = new IAudioFileService.ExtractAndValidateResult
            {
                PhraseStrings = new List<string> { "Phrase 1", "Phrase 2" },
                Errors = new List<string>()
            };
            
            _mockAudioFileService.Setup(s => s.ExtractAndValidatePhraseStrings(It.IsAny<IFormFile>()))
                .Returns(extractResult);

            _mockAudioProcessingService.Setup(s => s.DetectLanguageAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, new List<string> { "Language detection failed" }));

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CreateTitleFromFile_ReturnsBadRequest_WhenTokenMarkingFails()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "validToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = true, ErrorMessage = null });

            var extractResult = new IAudioFileService.ExtractAndValidateResult
            {
                PhraseStrings = new List<string> { "Phrase 1", "Phrase 2" },
                Errors = new List<string>()
            };

            _mockAudioFileService.Setup(s => s.ExtractAndValidatePhraseStrings(It.IsAny<IFormFile>()))
                .Returns(extractResult);

            var language = new Language { LanguageId = 1, Tag = "en", Name = "English" };
            _mockAudioProcessingService.Setup(s => s.DetectLanguageAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((language, new List<string>()));

            var title = new Title { TitleId = 1, TitleName = "Test Title", Description = "Test Description" };
            _mockAudioProcessingService.Setup(s => s.ProcessTitleAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<Language>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(title);

            _mockAudioProcessingService.Setup(s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, new List<string> { "Token already used" }));

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("Token already used", errorResponse.Errors);
        }

        [Fact]
        public async Task CreateTitleFromFile_ReturnsServerError_WhenDbExceptionOccurs()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "validToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = true, ErrorMessage = null });

            var extractResult = new IAudioFileService.ExtractAndValidateResult
            {
                PhraseStrings = new List<string> { "Phrase 1", "Phrase 2" },
                Errors = new List<string>()
            };

            _mockAudioFileService.Setup(s => s.ExtractAndValidatePhraseStrings(It.IsAny<IFormFile>()))
                .Returns(extractResult);

            var language = new Language { LanguageId = 1, Tag = "en", Name = "English" };
            _mockAudioProcessingService.Setup(s => s.DetectLanguageAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((language, new List<string>()));

            _mockAudioProcessingService.Setup(s => s.ProcessTitleAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<Language>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("Database error", new Exception()));

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
            Assert.Contains("An error occurred while saving changes to the database.", errorResponse.Errors);
        }

        [Fact]
        public async Task CreateTitleFromFile_ReturnsServerError_WhenGeneralExceptionOccurs()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                TitleName = "Test Title",
                Description = "Test Description",
                Token = "validToken",
                File = new FormFile(Stream.Null, 0, 0, "file", "test.txt")
            };

            _mockTokenService.Setup(s => s.CheckTokenStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ITokenService.TokenResult { Success = true, ErrorMessage = null });

            var extractResult = new IAudioFileService.ExtractAndValidateResult
            {
                PhraseStrings = new List<string> { "Phrase 1", "Phrase 2" },
                Errors = new List<string>()
            };

            _mockAudioFileService.Setup(s => s.ExtractAndValidatePhraseStrings(It.IsAny<IFormFile>()))
                .Returns(extractResult);

            var language = new Language { LanguageId = 1, Tag = "en", Name = "English" };
            _mockAudioProcessingService.Setup(s => s.DetectLanguageAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((language, new List<string>()));

            _mockAudioProcessingService.Setup(s => s.ProcessTitleAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                    It.IsAny<Language>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Processing error"));

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
            Assert.Contains("Processing error", errorResponse.Errors);
        }

        [Fact]
        public async Task CreateTitleFromFile_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new CreateTitleFromFileApiModel
            {
                // Add required properties with placeholder values
                Token = "placeholder", 
                TitleName = "placeholder",
                File = new FormFile(Stream.Null, 0, 0, "file", "placeholder.txt")
                // Missing required fields intentionally won't matter since we're manually adding ModelState errors
            };

            _controller.ModelState.AddModelError("TitleName", "Title name is required");

            // Act
            var result = await _controller.CreateTitleFromFile(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("Title name is required", errorResponse.Errors);
        }
    }
