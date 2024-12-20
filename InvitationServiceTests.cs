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
using Radzen;
using TeaWork.Data.Enums;
using TeaWork.Data;
using TeaWork.Logic.Dto;
using TeaWork.Data.Models;

namespace TeaWorkUnitTests
{
    public class InvitationServiceTests
    {
        private Mock<IUserIdentity> _userIdentityMock;
        private Mock<IConversationService> _conversationServiceMock;
        private Mock<IProjectService> _projectServiceMock;
        private Mock<ILogger<InvitationService>> _loggerMock;
        private Mock<IDbContextFactory<ApplicationDbContext>> _dbContextFactoryMock;

        private InvitationService _invitationService;

        public InvitationServiceTests()
        {

            _userIdentityMock = new Mock<IUserIdentity>();
            _conversationServiceMock = new Mock<IConversationService>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            _projectServiceMock = new Mock<IProjectService>();
            _loggerMock = new Mock<ILogger<InvitationService>>();

            _invitationService = new InvitationService(
                _dbContextFactoryMock.Object,
                _userIdentityMock.Object,
                _conversationServiceMock.Object,
                _projectServiceMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task SendInvitation_ShouldAdd()
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

                var project = new Project 
                { 
                    Id=1
                };
                db.Projects.Add(project);
                await db.SaveChangesAsync();
                var user = new ApplicationUser 
                { 
                Id="123",
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();


                // Act
                await _invitationService.SendInvitation("123", 1);

                // Assert
                var result = await db.Invitations.AsNoTracking().ToListAsync();

                Assert.Single(result);
                Assert.Equal("123", result[0].UserId);
                Assert.Equal(1, result[0].ProjectId);

            }
        }
        [Fact]
        public async Task AcceptInvitation_ShouldAdd()
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


                var conversation = new Conversation 
                { 
                Id=1
                };
                db.Conversations.Add(conversation);
                await db.SaveChangesAsync();

                var project = new Project
                {
                    Id = 1,
                    ProjectConversationId = 1,
                };
                db.Projects.Add(project);
                await db.SaveChangesAsync();

                

                var user = new ApplicationUser { Id = "123" };
                _userIdentityMock
                .Setup(u => u.GetLoggedUser())
                .ReturnsAsync(user);
                    

                var invitation = new Invitation
                {
                    Id = 1,
                    Status = InvitationStatus.Processed,
                    ProjectId=1,

                };
                db.Invitations.Add(invitation);
                await db.SaveChangesAsync();

                _projectServiceMock
                    .Setup(x => x.AddProjectMember(It.IsAny<Project>(), It.IsAny<ApplicationUser>(), It.IsAny<ProjectMemberRole>()))
                    .Callback<Project, ApplicationUser, ProjectMemberRole>((proj, usr, role) =>
                    {
                        db.ProjectMembers.Add(new ProjectMember { ProjectId = proj.Id, UserId = usr.Id, Role = role });
                        db.SaveChanges();
                    });

                _conversationServiceMock
                    .Setup(x => x.AddMember(It.IsAny<Conversation>(), It.IsAny<string>()))
                    .Callback<Conversation, string>((conv, userId) =>
                    {
                        db.ConversationMembers.Add(new ConversationMember { ConversationId = conv.Id, UserId = userId });
                        db.SaveChanges();
                    });



                // Act
                await _invitationService.AcceptInvitation(1);

                // Assert
                var result1 = await db.ProjectMembers.AsNoTracking().ToListAsync();
                var result2 = await db.Invitations.AsNoTracking().ToListAsync();
                var result3 = await db.ConversationMembers.AsNoTracking().ToListAsync();

                Assert.Single(result1);
                Assert.Single(result3);
                Assert.Equal("123", result1[0].UserId);
                Assert.Equal("123", result3[0].UserId);
                Assert.Equal(InvitationStatus.Accepted, result2[0].Status);
            }
        }
        [Fact]
        public async Task IsInvitationExist_ShouldReturnFalse()
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
                var result = await _invitationService.IsInvitationExist("123", 1);

                // Assert
                Assert.False(result);

            }
        }
    }
}

