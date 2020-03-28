using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using PGO.Identity.SignedSessions.Session;
using PGO.Identity.SignedSessions.Signature;
using Xunit;

namespace PGO.Identity.SignedSessions.Tests.Session
{
     public class CustomRedisCacheTests
    {
        private Mock<IDistributedCache> _baseCacheMock;
        private Mock<ISignatureHelper> _signatureHelper;

        private readonly CustomRedisCache _customCache;
        private string sessionKey = Guid.NewGuid().ToString();
        private const string unsignedValue = "dGVzdHZhbHVl"; // must be base64 encoded
        private const string signature = "tCSsoVusDeFis9E5QEtDXSsoOrnqiPQeheDXkkVkQv0="; // must be base64 encoded
        private DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();
        private byte[] sessionData = Encoding.UTF8.GetBytes("test session data");
        private byte[] signedSessionData = Encoding.UTF8.GetBytes(unsignedValue + "." + signature);

        public CustomRedisCacheTests()
        {
            _baseCacheMock = new Mock<IDistributedCache>();
            _signatureHelper = new Mock<ISignatureHelper>();
            _customCache = new CustomRedisCache(_baseCacheMock.Object, _signatureHelper.Object);
        }

        [Fact]
        public async Task GetSessionDataFromRedis_WhenSignatureIsValid_ReturnsSessionData()
        {
            //Arrange
            _baseCacheMock.Setup(m => m.GetAsync(sessionKey, default)).ReturnsAsync(signedSessionData);
            _signatureHelper.Setup(m => m.VerifySignature(unsignedValue, signature)).Returns(true);

            //Act
            var result = await _customCache.GetAsync(sessionKey);

            // Assert
            Assert.Equal(Convert.FromBase64String(unsignedValue), result);
        }

        [Fact]
        public void GetSessionDataFromRedis_WhenSignatureIsInValid_ThrowsException()
        {   
            //Arrange
            _baseCacheMock.Setup(m => m.GetAsync(sessionKey, default)).ReturnsAsync(signedSessionData);
            _signatureHelper.Setup(m => m.VerifySignature(unsignedValue, signature)).Returns(false);

            //Act
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _customCache.GetAsync(sessionKey)).Result;

            // Assert
            Assert.Equal("Session Signature is invalid", exception.Message);
        }

        [Fact]
        public async Task GetSessionDataFromRedis_WhenSessionIsEmpty()
        {
            sessionData = null;

            //Arrange
            _baseCacheMock.Setup(m => m.GetAsync(sessionKey, default)).ReturnsAsync(sessionData);

            //Act
            var result = await _customCache.GetAsync(sessionKey);

            // Assert
            Assert.Equal(sessionData, result);
        }

        [Fact]
        public async Task SetSessionDataInRedisAsync()
        {            
            var sessionDataWithSignature = Encoding.UTF8.GetBytes(Convert.ToBase64String(sessionData) + "." + signature);
            // Arrange
            _signatureHelper.Setup(m => m.CreateSignature(sessionData)).Returns(signature);

            // Act
            await _customCache.SetAsync(sessionKey, sessionData, distributedCacheEntryOptions);

            // Assert
            _baseCacheMock.Verify(m => m.SetAsync(sessionKey, sessionDataWithSignature, distributedCacheEntryOptions, default), Times.Once);
        }

        [Fact]
        public async Task SetSessionDataInRedisAsync_WhenSessionDataIsNull()
        {
            // Act
            await _customCache.SetAsync(sessionKey, null, distributedCacheEntryOptions);

            // Assert
            _baseCacheMock.Verify(m => m.SetAsync(sessionKey, null, distributedCacheEntryOptions, default), Times.Once);
        }

        [Fact]
        public void GetSessionDataFromRedis()
        {
            //Act
            var exception = Assert.Throws<NotImplementedException>(() => _customCache.Get(sessionKey));
            //Assert
            Assert.IsType<NotImplementedException>(exception);
        }

        [Fact]
        public void RefreshDataInRedis()
        {
            //Act
            var exception = Assert.Throws<NotImplementedException>(() => _customCache.Refresh(sessionKey));
            //Assert
            Assert.IsType<NotImplementedException>(exception);
        }

        [Fact]
        public async Task RefreshDataInRedisAsync()
        {
            //Act
            await _customCache.RefreshAsync(sessionKey, default);
            //Assert
            _baseCacheMock.Verify(m => m.RefreshAsync(sessionKey, default), Times.Once);
        }

        [Fact]
        public void RemoveDataInRedis()
        {
            //Act
            var exception = Assert.Throws<NotImplementedException>(() => _customCache.Remove(sessionKey));
            //Assert
            Assert.IsType<NotImplementedException>(exception);
        }

        [Fact]
        public async Task RemoveDataInRedisAsync()
        {
            //Act
            await _customCache.RemoveAsync(sessionKey, default);
            //Assert
            _baseCacheMock.Verify(m => m.RemoveAsync(sessionKey, default), Times.Once);
        }

        [Fact]
        public void SetDataInRedis()
        {
            //Act
            var exception = Assert.Throws<NotImplementedException>(() => _customCache.Set(sessionKey, sessionData, distributedCacheEntryOptions));
            //Assert
            Assert.IsType<NotImplementedException>(exception);
        }
    }
}