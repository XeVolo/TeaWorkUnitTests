using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TeaWork.Data.Enums;
using TeaWork.Data.Models;
using TeaWork.Data;
using TeaWork.Logic.DbContextFactory;
using TeaWork.Logic.Dto;
using TeaWork.Logic.Services.Interfaces;
using TeaWork.Logic.Services;
using Xunit;


public class ProjectServiceTests 
{
    private Mock<IUserIdentity> _userIdentityMock;
    private Mock<IConversationService> _conversationServiceMock;
    private Mock<ILogger<ProjectService>> _loggerMock;
    private Mock<IDbContextFactory> _dbContextFactoryMock;

    private ProjectService _projectService;

    public ProjectServiceTests()
    {

        _userIdentityMock = new Mock<IUserIdentity>();
        _conversationServiceMock = new Mock<IConversationService>();
        _dbContextFactoryMock = new Mock<IDbContextFactory>();
        _loggerMock = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _dbContextFactoryMock.Object,
            _userIdentityMock.Object,
            _conversationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Add_ShouldAddProjectAndLogSuccess()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("InMemoryDb");

        using (var db = new ApplicationDbContext(optionsBuilder.Options))
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            // Arrange
            var inMemoryContext = new ApplicationDbContext(optionsBuilder.Options);
            _dbContextFactoryMock
                .Setup(factory => factory.CreateDbContext())
                .Returns(inMemoryContext);

            var projectData = new ProjectAddDto
            {
                Title = "Test Project",
                Deadline = DateTime.Now.AddDays(7),
                Description = "Test Description"
            };

            _userIdentityMock
                .Setup(u => u.GetLoggedUser())
                .ReturnsAsync(new ApplicationUser { Id = "user-id" });

            _conversationServiceMock
                .Setup(c => c.AddConversation(It.IsAny<ConversationType>(), It.IsAny<string>()))
                .ReturnsAsync(new Conversation { Id = 1 });


            // Act
            await _projectService.Add(projectData);

            // Assert

            var actualProjects = await db.Projects.AsNoTracking().ToListAsync();
            Assert.Single(actualProjects);
            Assert.Equal("Test Project", actualProjects[0].Title);
            Assert.Equal(1, actualProjects[0].ProjectConversationId);

        }
    }
    [Fact]
    public async Task GetProjectById_ShouldReturnProject()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("InMemoryDb");

        using (var db = new ApplicationDbContext(optionsBuilder.Options))
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            // Arrange
            var inMemoryContext = new ApplicationDbContext(optionsBuilder.Options);
            _dbContextFactoryMock
                .Setup(factory => factory.CreateDbContext())
                .Returns(inMemoryContext);

            var project = new Project { Id = 1, Title = "Test Project" };
            db.Projects.Add(project);
            await db.SaveChangesAsync();


            // Act
            var result = await _projectService.GetProjectById(1);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Project", result.Title);

        }
    }
    [Fact]
    public async Task GetMyProjects_ShouldReturnUserProjects()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("InMemoryDb");

        using (var db = new ApplicationDbContext(optionsBuilder.Options))
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            // Arrange
            var inMemoryContext = new ApplicationDbContext(optionsBuilder.Options);
            _dbContextFactoryMock
                .Setup(factory => factory.CreateDbContext())
                .Returns(inMemoryContext);

            var currentUser = new ApplicationUser { Id = "123" };
            _userIdentityMock.Setup(identity => identity.GetLoggedUser()).ReturnsAsync(currentUser);

            var projects = new List<Project>
            {
                new Project { Id = 1, Title = "Project 1" },
                new Project { Id = 2, Title = "Project 2" }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();
            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember { Id = 1, ProjectId=1, UserId="123" },
                new ProjectMember { Id = 2, ProjectId=2, UserId="123" }
            };
            db.ProjectMembers.AddRange(projectMembers);
            await db.SaveChangesAsync();

            // Act
            var result = await _projectService.GetMyProjects();

            // Assert

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);

        }
    }
}
