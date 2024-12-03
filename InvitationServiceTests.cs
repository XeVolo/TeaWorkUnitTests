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

namespace TeaWorkUnitTests
{
    public class InvitationServiceTests
    {
        private Mock<IUserIdentity> _userIdentityMock;
        private Mock<IConversationService> _conversationServiceMock;
        private Mock<IProjectService> _projectServiceMock;
        private Mock<ILogger<InvitationService>> _loggerMock;
        private Mock<IDbContextFactory> _dbContextFactoryMock;

        private InvitationService _invitationService;

        public InvitationServiceTests()
        {

            _userIdentityMock = new Mock<IUserIdentity>();
            _conversationServiceMock = new Mock<IConversationService>();
            _dbContextFactoryMock = new Mock<IDbContextFactory>();
            _projectServiceMock = new Mock<IProjectService>();
            _loggerMock = new Mock<ILogger<InvitationService>>();

            _invitationService = new InvitationService(
                _dbContextFactoryMock.Object,
                _userIdentityMock.Object,
                _conversationServiceMock.Object,
                _projectServiceMock.Object,
                _loggerMock.Object);
        }




    }
}

