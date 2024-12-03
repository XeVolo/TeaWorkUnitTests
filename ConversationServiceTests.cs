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
using TeaWork.Data.Models;
using TeaWork.Data;
using TeaWork.Logic.Dto;
using TeaWork.Data.Enums;

namespace TeaWorkUnitTests
{
    public class ConversationServiceTests
    {
        private Mock<IUserIdentity> _userIdentityMock;
        private Mock<ILogger<ConversationService>> _loggerMock;
        private Mock<IDbContextFactory> _dbContextFactoryMock;

        private ConversationService _conversationService;

        public ConversationServiceTests()
        {

            _userIdentityMock = new Mock<IUserIdentity>();
            _dbContextFactoryMock = new Mock<IDbContextFactory>();
            _loggerMock = new Mock<ILogger<ConversationService>>();

            _conversationService = new ConversationService(
                _userIdentityMock.Object,
                _dbContextFactoryMock.Object,                
                _loggerMock.Object);
        }
        [Fact]
        public async Task Add_ShouldAdd()
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
                await _conversationService.AddConversation(ConversationType.PrivateChat, "123");

                // Assert

                var result = await db.Conversations.AsNoTracking().ToListAsync();
                Assert.Single(result);
                Assert.Equal("123", result[0].Name);

            }
        }

        [Fact]
        public async Task AddMember_ShouldAdd()
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

                var conv = new Conversation
                {
                    Id = 1
                };
                db.Conversations.Add(conv);
                await db.SaveChangesAsync();
                var user = new ApplicationUser { Id = "123" };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                // Act
                await _conversationService.AddMember(conv, "123");

                // Assert

                var result = await db.ConversationMembers.AsNoTracking().ToListAsync();
                Assert.Single(result);
                Assert.Equal("123", result[0].UserId);

            }
        }
        [Theory]
        [InlineData(1, "Group Chat Name", ConversationType.GroupChat, "Group Chat Name")]
        [InlineData(2, null, ConversationType.GroupChat, "2")]
        [InlineData(3, null, ConversationType.PrivateChat, "otheruser@example.com")]
        [InlineData(4, null, ConversationType.PrivateChat, "Unknown User")]
        [InlineData(5, null, ConversationType.PrivateChat, "5")]
        public async Task GetConversationName_ShouldReturn(
                int conversationId,
                string conversationName,
                ConversationType conversationType,
                string expectedName)
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

                var loggedUser = new ApplicationUser { Id = "loggedUser", Email = "loggeduser@example.com" };

                var otherUser = new ApplicationUser { Id = "otherUser", Email = "otheruser@example.com" };

                _userIdentityMock
                    .Setup(u => u.GetLoggedUser())
                    .ReturnsAsync(loggedUser);

                var conversation = new Conversation
                {
                    Id = conversationId,
                    Name = conversationName,
                    ConversationType = conversationType,
                    ConversationMembers = conversationType == ConversationType.PrivateChat
                        ? new List<ConversationMember>
                        {
                            new ConversationMember { UserId = loggedUser.Id, ConversationId = conversationId },
                            new ConversationMember { UserId = otherUser.Id, ConversationId = conversationId }
                        }
                        : new List<ConversationMember>()
                };

                if (conversationType == ConversationType.PrivateChat && conversationId == 4)
                {
                    conversation.ConversationMembers.RemoveAt(1); 
                }
                if (conversationType == ConversationType.PrivateChat && conversationId == 5)
                {
                    conversation.ConversationMembers.Clear();
                }

                db.Conversations.Add(conversation);
                db.Users.AddRange(loggedUser, otherUser);
                await db.SaveChangesAsync();
                // Act
                var result = await _conversationService.GetConversationName(conversationId);

                // Assert
                Assert.Equal(expectedName, result);

            }
        }
    }
}
