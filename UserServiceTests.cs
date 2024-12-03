using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaWork.Logic.Services.Interfaces;
using TeaWork.Logic.Services;
using TeaWork.Logic.DbContextFactory;
using Microsoft.EntityFrameworkCore;
using TeaWork.Data;

namespace TeaWorkUnitTests
{
    public class UserServiceTests
    {
        private Mock<IUserIdentity> _userIdentityMock;
        private Mock<ILogger<UserService>> _loggerMock;
        private Mock<IDbContextFactory> _dbContextFactoryMock;

        private UserService _userService;

        public UserServiceTests()
        {

            _userIdentityMock = new Mock<IUserIdentity>();
            _dbContextFactoryMock = new Mock<IDbContextFactory>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _dbContextFactoryMock.Object,
                _userIdentityMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task GetLoggedUserId_ShouldReturn()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            await using (var db = new ApplicationDbContext(optionsBuilder.Options))
            {
                // Arrange
                var inMemoryContext = new ApplicationDbContext(optionsBuilder.Options);
                _dbContextFactoryMock
                    .Setup(factory => factory.CreateDbContext())
                    .Returns(inMemoryContext);

                _userIdentityMock
                    .Setup(u => u.GetLoggedUser())
                    .ReturnsAsync(new ApplicationUser { Id = "123" });

                // Act
                var result = await _userService.GetLoggedUserId();
                // Assert;

                Assert.Equal("123", result);
            }
        }
        [Fact]
        public async Task FindUserByEmail_ShouldNotReturn()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            await using (var db = new ApplicationDbContext(optionsBuilder.Options))
            {
                // Arrange
                var inMemoryContext = new ApplicationDbContext(optionsBuilder.Options);
                _dbContextFactoryMock
                    .Setup(factory => factory.CreateDbContext())
                    .Returns(inMemoryContext);


                // Act
                var result = await _userService.FindUserByEmail("email@gmail.com");
                // Assert;

                Assert.Null(result);
            }
        }
        [Fact]
        public async Task FindUserEmailById_ShouldReturn()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            await using (var db = new ApplicationDbContext(optionsBuilder.Options))
            {
                // Arrange
                var inMemoryContext = new ApplicationDbContext(optionsBuilder.Options);
                _dbContextFactoryMock
                    .Setup(factory => factory.CreateDbContext())
                    .Returns(inMemoryContext);
                var user = new ApplicationUser
                {
                    Id = "123",
                    Email = "email@gmail.com"
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                // Act
                var result = await _userService.FindUserEmailById("123");
                // Assert;

                Assert.Equal("email@gmail.com", result);
            }
        }
    }
}
