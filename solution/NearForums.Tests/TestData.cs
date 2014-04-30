using NearForums.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Tests
{
    public class TestData
    {
        internal static User CreateTestuser(bool createMembershipEntry = false)
        {
            User user = new User()
            {
                Email = "admin@test.com",
                Role = UserRole.Admin,
                Signature = "Test admin",
                UserName = Guid.NewGuid().ToString(),
                BirthDate = DateTime.Now
            };

            IUsersService userService = TestHelper.Resolve<IUsersService>();
            AuthenticationProvider provider = AuthenticationProvider.CustomDb;
            string providerId = Guid.NewGuid().ToString("N");
            if (createMembershipEntry)
            {
                 System.Web.Security.MembershipUser membershipUser = System.Web.Security.Membership.CreateUser(
                    user.UserName, 
                    "testPassword", 
                    user.Email);
                
                provider = AuthenticationProvider.Membership;
                providerId = membershipUser.ProviderUserKey.ToString();
            }


            user = userService.Add(user, provider, providerId);
            
            return user;
        }

        internal static Forum CreateTestForum(User forumCreator)
        {
            return TestData.CreateTestForum(
                forumCreator,
                TestData.CreateTestCategory(forumCreator));
        }

        internal static Forum CreateTestForum(User forumCreator, ForumCategory category)
        {
            Forum forum = new Forum()
            {
                Category = category,
                Name = "Test forum",
                ShortName = Guid.NewGuid().ToString("N"),
                Description = "This is a test forum created in " + GetTestName()
            };

            TestHelper.Resolve<IForumsService>().Add(forum, forumCreator.Id);

            return forum;
        }

        internal static ForumCategory CreateTestCategory(User categoryCreator)
        {
            ForumCategory category = new ForumCategory()
            {
                Name = "Test category created in " + GetTestName()
            };

            TestHelper.Resolve<IForumCategoriesService>().Add(category);

            return category;
        }

        internal static Topic CreateTestTopic(Forum parent, User user)
        {
            Topic topic = new Topic()
            {
                Forum = parent,
                Title = "Test topic",
                ShortName = "test-topic",
                Description = "This is test topic created in " + GetTestName(),
                Tags = new TagList("some new tags"),
            };

            TestHelper.Resolve<ITopicsService>().Create(topic, "192.168.0.1", user);

            return topic;
        }

        private static string GetTestName()
        {
            string methodName = new System.Diagnostics.StackTrace()
                .GetFrames()
                .First(sf => 
                    sf.GetMethod()
                        .GetCustomAttributes(typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute), false)
                        .Length > 0)
                .GetMethod()
                .Name;

            return methodName;
        }
    }
}
