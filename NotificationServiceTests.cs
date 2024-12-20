using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaWork.Logic.Services.Interfaces;
using TeaWork.Logic.Services;
using Microsoft.EntityFrameworkCore;
using TeaWork.Data.Models;
using TeaWork.Data;
using TeaWork.Logic.Dto;
using TeaWork.Data.Enums;

namespace TeaWorkUnitTests
{
    public class NotificationServiceTests
    {
        private Mock<IUserIdentity> _userIdentityMock;
        private Mock<ILogger<NotificationService>> _loggerMock;
        private Mock<IDbContextFactory<ApplicationDbContext>> _dbContextFactoryMock;

        private NotificationService _notificationService;

        public NotificationServiceTests()
        {

            _userIdentityMock = new Mock<IUserIdentity>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            _loggerMock = new Mock<ILogger<NotificationService>>();

            _notificationService = new NotificationService(
                _dbContextFactoryMock.Object,
                _userIdentityMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task NewNotification_ShouldAdd()
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

                var notification = new NotificationDto
                {
                    UserId="123",
                    Title="Title",
                    Description="Description",
                    NotifiType=NotificationType.Message
                };


                // Act
                await _notificationService.NewNotification(notification);

                // Assert
                var result = await db.Notifications.AsNoTracking().ToListAsync();

                Assert.Single(result);
                Assert.Equal("Title", result[0].Title);
                Assert.Equal(NotificationType.Message, result[0].NotificationType);

            }
        }
        [Fact]
        public async Task NotificationDisplayed_ShouldEdit()
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

                var notification = new Notification
                {
                    Id=1,
                    UserId = "123",
                    Title = "Title",
                    Description = "Description",
                    Status = NotificationonStatus.New
                };
                db.Notifications.Add(notification);
                await db.SaveChangesAsync();


                // Act
                await _notificationService.NotificationDisplayed(notification);

                // Assert
                var result = await db.Notifications.AsNoTracking().ToListAsync();

                Assert.Single(result);
                Assert.Equal("Title", result[0].Title);
                Assert.Equal(NotificationonStatus.Seen, result[0].Status);

            }
        }
        [Fact]
        public async Task GetMyNewNotification_ShouldReturn()
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

                var notifications = new List<Notification>
                {
                    new Notification{ Id=1,UserId="123",Status=NotificationonStatus.New},
                    new Notification{ Id=2,UserId="123",Status=NotificationonStatus.New},
                    new Notification{ Id=3,UserId="123",Status=NotificationonStatus.Old},
                    new Notification{ Id=4,UserId="123",Status=NotificationonStatus.Seen},
                    new Notification{ Id=5,UserId="321",Status=NotificationonStatus.New},
                };
                db.Notifications.AddRange(notifications);
                await db.SaveChangesAsync();


                // Act
                var result = await _notificationService.GetMyNewNotifications();

                // Assert
                Assert.Equal(2, result.Count);

            }
        }


    }
}
