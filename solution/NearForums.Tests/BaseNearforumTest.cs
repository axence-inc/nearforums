using Microsoft.VisualStudio.TestTools.UnitTesting;
using NearForums.Tests.TestCleanup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Tests
{
    public class BaseNearforumTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            Cleaner.Instance.Cleanup();
        }
    }
}
