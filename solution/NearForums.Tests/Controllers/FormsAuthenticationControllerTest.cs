using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.SessionState;
using NearForums.Web.State;
using NearForums.Web.Controllers;
using System.Web.Mvc;
using System.Collections;
using NearForums.Configuration;
using System.Web.Security;
using NearForums.Tests.Fakes;

namespace NearForums.Tests.Controllers
{
	[TestClass]
	public class FormsAuthenticationControllerTest
	{
        [TestMethod]
        public void FormsAuthentication_ResetPasswordTest()
        {
            Assert.Inconclusive("Changing password is possible from Helpdesk only.");
            FormsAuthenticationController controller = TestHelper.Resolve<FormsAuthenticationController>();
            controller.ControllerContext = new FakeControllerContext(controller, "http://localhost/forums/");
            controller.Url = new UrlHelper(controller.ControllerContext.RequestContext);

            User testUser = TestData.CreateTestuser(true);

            var result = controller.ResetPassword(testUser.Email);
            Assert.IsTrue(controller.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

	}
}
