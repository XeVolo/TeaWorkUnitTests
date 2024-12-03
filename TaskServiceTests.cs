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
using System.ComponentModel;

public class TaskServiceTests
{
    private Mock<IUserIdentity> _userIdentityMock;
    private Mock<ILogger<TaskService>> _loggerMock;
    private Mock<IDbContextFactory> _dbContextFactoryMock;

    private TaskService _taskService;

    public TaskServiceTests()
    {

        _userIdentityMock = new Mock<IUserIdentity>();
        _dbContextFactoryMock = new Mock<IDbContextFactory>();
        _loggerMock = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(
            _dbContextFactoryMock.Object,
            _userIdentityMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Add_ShouldAddProjectTask()
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

            var taskData = new ProjectTaskAddDto
            {
                Title = "Test ProjectTask",
                Deadline = DateTime.Now.AddDays(7),
                Start = DateTime.Now.AddDays(7),
                Description = "Test Description",
                State=0,
                Priority=0,
            };
            var project = new Project { Id = 1 , ToDoListId=1 };
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            
            _userIdentityMock
                .Setup(u => u.GetLoggedUser())
                .ReturnsAsync(new ApplicationUser { Id = "user-id" });

            // Act
            await _taskService.Add(taskData,1);
            // Assert

            var result = await db.ProjectTasks.AsNoTracking().ToListAsync();
            Assert.Single(result);
            Assert.Equal(1, result[0].ToDoListId);
        }
    }
    [Fact]
    public async Task GetProjectTasks_ShouldReturnTasks()
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

           
            var project = new Project { Id = 1, ToDoListId = 1 };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1},
                new ProjectTask{ Id=2, ToDoListId=1},
                new ProjectTask{ Id=3, ToDoListId=1},
                new ProjectTask{ Id=4, ToDoListId=2},
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();

            _userIdentityMock
                .Setup(u => u.GetLoggedUser())
                .ReturnsAsync(new ApplicationUser { Id = "123" });

            // Act
            var result = await _taskService.GetProjectTasks(1);
            // Assert

            Assert.Equal(3, result.Count());
        }
    }
    [Fact]
    public async Task GetMyProjectTasks_ShouldReturnTasks()
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


            var projects = new List<Project> 
            {
                new Project { Id = 1, ToDoListId = 1 },
                new Project { Id = 2, ToDoListId = 2 }
            }; 
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1},
                new ProjectTask{ Id=2, ToDoListId=1},
                new ProjectTask{ Id=3, ToDoListId=1},
                new ProjectTask{ Id=4, ToDoListId=2},
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();

            var taskDistribution = new List<TaskDistribution>
            {
                new TaskDistribution { Id=1, TaskId=1,UserId="123"},
                new TaskDistribution { Id=2, TaskId=3,UserId="123"},
                new TaskDistribution { Id=3, TaskId=4,UserId="123"},
                new TaskDistribution { Id=4, TaskId=2,UserId="321"},
            };
            db.TaskDistributions.AddRange(taskDistribution);
            await db.SaveChangesAsync();

            _userIdentityMock
                .Setup(u => u.GetLoggedUser())
                .ReturnsAsync(new ApplicationUser { Id = "123" });

            // Act
            var result = await _taskService.GetMyProjectTasks();
            // Assert

            Assert.Equal(3, result.Count());
        }
    }
    [Fact]
    public async Task GetProjectId_ShouldReturnId()
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


            var projects = new List<Project>
            {
                new Project { Id = 1, ToDoListId = 1 },
                new Project { Id = 2, ToDoListId = 2 }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1},
                new ProjectTask{ Id=2, ToDoListId=1},
                new ProjectTask{ Id=3, ToDoListId=1},
                new ProjectTask{ Id=4, ToDoListId=2},
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();

            // Act
            var result = await _taskService.GetProjectId(2);

            // Assert

            Assert.Equal(1, result);
        }
    }

    [Fact]
    public async Task AddTaskDistribution_ShouldAdd()
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


            var projects = new List<Project>
            {
                new Project { Id = 1, ToDoListId = 1 },
                new Project { Id = 2, ToDoListId = 2 }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1},
                new ProjectTask{ Id=2, ToDoListId=1},
                new ProjectTask{ Id=3, ToDoListId=1},
                new ProjectTask{ Id=4, ToDoListId=2},
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();

            var user= new ApplicationUser { Id = "123" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Act
            await _taskService.AddTaskDistribution(2, "123");
            // Assert
            var result =await db.TaskDistributions.AsNoTracking().ToListAsync();

            Assert.Single(result);
            Assert.Equal("123", result[0].UserId);
            Assert.Equal(2, result[0].TaskId);
        }
    }
    [Fact]
    public async Task AddTaskDistribution_ShouldNotAdd()
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


            var projects = new List<Project>
            {
                new Project { Id = 1, ToDoListId = 1 },
                new Project { Id = 2, ToDoListId = 2 }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1},
                new ProjectTask{ Id=2, ToDoListId=1},
                new ProjectTask{ Id=3, ToDoListId=1},
                new ProjectTask{ Id=4, ToDoListId=2},
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();

            var user = new ApplicationUser { Id = "123" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var taskDistribution = new TaskDistribution { Id = 1, TaskId = 2, UserId = "123" };
            db.TaskDistributions.Add(taskDistribution);
            await db.SaveChangesAsync();

            // Act
            await _taskService.AddTaskDistribution(2, "123");
            // Assert
            var result = await db.TaskDistributions.AsNoTracking().ToListAsync();

            Assert.Single(result);
            Assert.Equal("123", result[0].UserId);
            Assert.Equal(2, result[0].TaskId);
        }
    }
    [Fact]
    public async Task AddComment_ShouldAdd()
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

            var comment = new DesignConceptDto
            {
                Description = "Comment"
            };


            var projects = new List<Project>
            {
                new Project { Id = 1, ToDoListId = 1 }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1},
                new ProjectTask{ Id=2, ToDoListId=1}
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();

            _userIdentityMock
                .Setup(u => u.GetLoggedUser())
                .ReturnsAsync(new ApplicationUser { Id = "123" });

            // Act
            await _taskService.AddComment(comment, 1);
            // Assert
            var result = await db.TaskComments.AsNoTracking().ToListAsync();

            Assert.Single(result);
            Assert.Equal("123", result[0].UserId);
            Assert.Equal("Comment", result[0].Description);
        }
    }
    [Fact]
    public async Task ChangePriorityTask_ShouldChange()
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

            var projects = new List<Project>
            {
                new Project { Id = 1, ToDoListId = 1 }
            };
            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var tasks = new List<ProjectTask>
            {
                new ProjectTask{ Id=1, ToDoListId=1,Priority=TaskPriority.Low},
                new ProjectTask{ Id=2, ToDoListId=1,Priority=TaskPriority.Medium}
            };
            db.ProjectTasks.AddRange(tasks);
            await db.SaveChangesAsync();


            // Act
            await _taskService.ChangePriorityTask(1, TaskPriority.High);
            // Assert
            var result = await db.ProjectTasks.AsNoTracking().ToListAsync();

            Assert.Equal(TaskPriority.High, result[0].Priority);
            Assert.Equal(TaskPriority.Medium, result[1].Priority);
        }
    }
}