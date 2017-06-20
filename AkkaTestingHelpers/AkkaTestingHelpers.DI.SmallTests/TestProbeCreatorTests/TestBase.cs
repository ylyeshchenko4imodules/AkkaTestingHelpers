﻿using Akka.TestKit.Xunit2;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Concrete;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.TestProbeCreatorTests
{
    internal class TestBase : TestKit
    {
        public TestBase() : base(AkkaConfig.Config) { }

        public TestProbeCreator CreateTestProbeCreator() => new TestProbeCreator();
    }
}