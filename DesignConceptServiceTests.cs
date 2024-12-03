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
using TeaWork.Logic.Dto;
using TeaWork.Data.Models;

namespace TeaWorkUnitTests
{
    public class DesignConceptServiceTests
    {
        private Mock<IUserIdentity> _userIdentityMock;
        private Mock<ILogger<DesignConceptService>> _loggerMock;
        private Mock<IDbContextFactory> _dbContextFactoryMock;

        private DesignConceptService _designConceptService;

        public DesignConceptServiceTests()
        {

            _userIdentityMock = new Mock<IUserIdentity>();
            _dbContextFactoryMock = new Mock<IDbContextFactory>();
            _loggerMock = new Mock<ILogger<DesignConceptService>>();

            _designConceptService = new DesignConceptService(
                _dbContextFactoryMock.Object,
                _userIdentityMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task Add_ShouldAddDesignConcept()
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

                var designConceptData = new DesignConceptDto
                {
                    Title = "Test",
                    Description = "Test Description",
                };
                var project = new Project { Id = 1, ToDoListId = 1 };
                db.Projects.Add(project);
                await db.SaveChangesAsync();

                _userIdentityMock
                    .Setup(u => u.GetLoggedUser())
                    .ReturnsAsync(new ApplicationUser { Id = "user-id" });

                // Act
                await _designConceptService.Add(designConceptData, 1);
                // Assert

                var result = await db.OwnDesignConcepts.AsNoTracking().ToListAsync();
                Assert.Single(result);
                Assert.Equal(1, result[0].ProjectId);
            }
        }
        [Fact]
        public async Task GetDesignConcepts_ShouldReturnProject()
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

                var designConcepts = new List<OwnDesignConcept>
                {
                    new OwnDesignConcept{ Id=1, ProjectId=1},
                    new OwnDesignConcept{ Id=2, ProjectId=1},
                    new OwnDesignConcept{ Id=3, ProjectId=2},
                };

                db.OwnDesignConcepts.AddRange(designConcepts);
                await db.SaveChangesAsync();

                // Act
                var result = await _designConceptService.GetDesignConcepts(1);

                // Assert

                Assert.Equal(2, result.Count());
            }
        }
    }
}
