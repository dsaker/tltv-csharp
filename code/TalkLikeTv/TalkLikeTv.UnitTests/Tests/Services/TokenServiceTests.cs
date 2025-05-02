using Moq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
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
        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

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

        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

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

        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

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

        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

        // Act
        var result = await service.CheckTokenStatus(token);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task MarkTokenAsUsedAsync_ShouldReturnSuccess_WhenUpdateSucceeds()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var token = new Token { TokenId = 1, Hash = "abc", Used = false };
        mockTokenRepository
            .Setup(r => r.UpdateAsync(token.TokenId.ToString(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // <-- Fix: returns Task<bool>

        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

        // Act
        var (success, errors) = await service.MarkTokenAsUsedAsync(token);

        // Assert
        Assert.True(success);
        Assert.Empty(errors);
        Assert.True(token.Used);
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_ShouldReturnError_WhenExceptionThrown()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var token = new Token { TokenId = 1, Hash = "abc", Used = false };
        mockTokenRepository
            .Setup(r => r.UpdateAsync(token.TokenId.ToString(), token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

        // Act
        var (success, errors) = await service.MarkTokenAsUsedAsync(token);

        // Assert
        Assert.False(success);
        Assert.Single(errors);
        Assert.Contains("An error occurred while processing the token.", errors);
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_ShouldSetUsedToTrue()
    {
        // Arrange
        var mockTokenRepository = new Mock<ITokenRepository>();
        var token = new Token { TokenId = 1, Hash = "abc", Used = false };
        mockTokenRepository
            .Setup(r => r.UpdateAsync(token.TokenId.ToString(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new TokenService(mockTokenRepository.Object, Mock.Of<ILogger<TokenService>>());

        // Act
        await service.MarkTokenAsUsedAsync(token);

        // Assert
        Assert.True(token.Used);
    }
}