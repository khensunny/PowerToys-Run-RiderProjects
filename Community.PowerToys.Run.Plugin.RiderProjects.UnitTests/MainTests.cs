// Copyright (c) Mpho Jele. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.RiderProjects.UnitTests
{
    [TestClass]
    public class MainTests
    {
        private Main _main;

        [TestInitialize]
        public void TestInitialize()
        {
            _main = new Main();
        }

        [TestMethod]
        public void Query_should_return_results()
        {
            // TODO: Implement a way to mock the RiderProjectsService
            // var results = _main.Query(new("search"));

            // Assert.IsNotNull(results.First());
        }

        [TestMethod]
        public void LoadContextMenus_should_return_results()
        {
            // var results = _main.LoadContextMenus(new Result { ContextData = "search" });

            // Assert.IsNotNull(results.First());
        }
    }
}
