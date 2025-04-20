using Moq;
using System.Security.Cryptography;
using System.Text;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class TokenServiceTests
{
    [Fact]
    public async Task CheckTokenStatus_ShouldThrowArgumentNullException_WhenTokenIsNull()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var service = new TokenService(mockTokenRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CheckTokenStatus(null));
    }

    [Fact]
    public async Task CheckTokenStatus_ShouldReturnError_WhenTokenNotFound()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var token = "test-token";
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        mockTokenRepository
            .Setup(repo => repo.RetrieveByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Token)null);

        var service = new TokenService(mockTokenRepository.Object);

        // Act
        var result = await service.CheckTokenStatus(token);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Token not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckTokenStatus_ShouldReturnError_WhenTokenAlreadyUsed()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var token = "test-token";
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        mockTokenRepository
            .Setup(repo => repo.RetrieveByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Token { Used = true });

        var service = new TokenService(mockTokenRepository.Object);

        // Act
        var result = await service.CheckTokenStatus(token);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Token already used.", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckTokenStatus_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var token = "test-token";
        var tokenHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        mockTokenRepository
            .Setup(repo => repo.RetrieveByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Token { Used = false });

        var service = new TokenService(mockTokenRepository.Object);

        // Act
        var result = await service.CheckTokenStatus(token);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }
}