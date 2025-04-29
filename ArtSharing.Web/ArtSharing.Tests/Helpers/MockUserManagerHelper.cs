using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using MockQueryable.Moq;

using System.Collections.Generic;
using System.Linq;

namespace ArtSharing.Tests.Helpers
{
    public static class MockUserManagerHelper
    {
        public static Mock<UserManager<User>> CreateMockUserManager(List<User> users)
        {
            var store = new Mock<IUserStore<User>>();
            var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            var usersQueryable = users.AsQueryable().BuildMock();
            mgr.Setup(x => x.Users).Returns(usersQueryable.Object);

            mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id == id));

            mgr.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) =>
                {
                    var roles = new List<string> { "User" };
                    if (user.UserName == "mod") roles.Add("Moderator");
                    return roles;
                });

            mgr.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), "Moderator")).ReturnsAsync(true);
            mgr.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }
    }
}
