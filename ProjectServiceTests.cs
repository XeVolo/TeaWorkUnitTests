using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TeaWork.Data.Enums;
using TeaWork.Data.Models;
using TeaWork.Data;
using TeaWork.Logic.Dto;
using TeaWork.Logic.Services.Interfaces;
using TeaWork.Logic.Services;
using Xunit;


public class ProjectServiceTests 
{
    private Mock<IUserIdentity> _userIdentityMock;
    private Mock<IConversationService> _conversationServiceMock;
    private Mock<ILogger<ProjectService>> _loggerMock;
    private Mock<IDbContextFactory<ApplicationDbContext>> _dbContextFactoryMock;

    private ProjectService _projectService;

    public ProjectServiceTests()
    {

        _userIdentityMock = new Mock<IUserIdentity>();
        _conversationServiceMock = new Mock<IConversationService>();
        _dbContextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
        _loggerMock = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _dbContextFactoryMock.Object,
            _userIdentityMock.Object,
            _conversationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Add_ShouldAddProject()
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
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        await using (var db = new ApplicationDbContext(optionsBuilder.Options))
        {
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
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        await using (var db = new ApplicationDbContext(optionsBuilder.Options))
        {
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
                new Project { Id = 2, Title = "Project 2" },
                new Project { Id = 3, Title = "Project 3" }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();
            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember { Id = 1, ProjectId=1, UserId="123" },
                new ProjectMember { Id = 2, ProjectId=2, UserId="123" },
                new ProjectMember { Id = 3, ProjectId=3, UserId="321" },
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
    [Fact]
    public async Task AddMrojectMember_ShouldAddMember()
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


            var project = new Project { Id = 1, Title = "Test Project" };
            var user = new ApplicationUser { Id = "123" };
            var role = ProjectMemberRole.User;

            // Act
            await _projectService.AddProjectMember(project, user, role);

            // Assert
            var result = await db.ProjectMembers.AsNoTracking().ToListAsync();
            Assert.Single(result);
            Assert.Equal("123", result[0].UserId);
            Assert.Equal(1, result[0].ProjectId);

        }
    }
    [Fact]
    public async Task DeleteUserFromProject_ShouldDelete()
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

            var project = new Project { Id = 1, Title = "Test Project", ToDoListId=1,ProjectConversationId=1};
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            var user = new ApplicationUser { Id = "123" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var projectMember = new ProjectMember { Id = 1, UserId = user.Id, ProjectId = project.Id,Role=0 };
            db.ProjectMembers.Add(projectMember);
            await db.SaveChangesAsync();

            var conversationMember = new ConversationMember { Id = 1, UserId = "123", ConversationId=1 };
            db.ConversationMembers.Add(conversationMember);
            await db.SaveChangesAsync();

            var projectTasks = new List<ProjectTask>
            {
                new ProjectTask { Id = 1, ToDoListId = 1 },
                new ProjectTask { Id = 2, ToDoListId = 1 }
            };
            db.ProjectTasks.AddRange(projectTasks);
            await db.SaveChangesAsync();

            var projectDistribution = new List<TaskDistribution>
            {
                new TaskDistribution {Id=1, TaskId=1,UserId="123" },
                new TaskDistribution {Id=2, TaskId=2,UserId="123" }
            };
            db.TaskDistributions.AddRange(projectDistribution);
            await db.SaveChangesAsync();

            var invitation = new Invitation { Id = 1, ProjectId = 1, UserId = "123" };
            db.Invitations.Add(invitation);
            await db.SaveChangesAsync();

            // Act
            await _projectService.DeleteUserFromProject(user.Id,project.Id);

            // Assert
            var result1 = await db.Projects.AsNoTracking().ToListAsync();
            var result2 = await db.Users.AsNoTracking().ToListAsync();
            var result3 = await db.ProjectMembers.AsNoTracking().ToListAsync();
            var result4 = await db.ConversationMembers.AsNoTracking().ToListAsync();
            var result5 = await db.ProjectTasks.AsNoTracking().ToListAsync();
            var result6 = await db.TaskDistributions.AsNoTracking().ToListAsync();
            var result7 = await db.Invitations.AsNoTracking().ToListAsync();

            Assert.Single(result1);
            Assert.Single(result2);
            Assert.Empty(result3);
            Assert.Empty(result4);
            Assert.Equal(2,result5.Count());
            Assert.Empty(result6);
            Assert.Empty(result7);

        }
    }
    [Fact]
    public async Task CheckUserAcces_ShuldReturnTrue()
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

            var projectMember = new ProjectMember { Id = 1, UserId = "123", ProjectId = 1, Role = 0 };
            db.ProjectMembers.Add(projectMember);
            await db.SaveChangesAsync();

            // Act
            bool result = await _projectService.CheckUserAccess(1);

            // Assert
            Assert.True(result);
        }
    }
}
