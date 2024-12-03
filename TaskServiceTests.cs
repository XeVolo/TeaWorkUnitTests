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


}