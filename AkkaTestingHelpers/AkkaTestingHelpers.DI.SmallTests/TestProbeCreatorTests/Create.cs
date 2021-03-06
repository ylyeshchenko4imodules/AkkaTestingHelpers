﻿using System;
using Akka.Actor;
using Akka.TestKit;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Concrete;
using FluentAssertions;
using Xunit;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.TestProbeCreatorTests
{
    public class Create : TestBase
    {
        [Fact]
        public void TestProbeCreator_CreateWithNullTestKitBase_ThrowsArgumentNullException()
        {
            //arrange
            TestProbeCreator sut = CreateTestProbeCreator();

            //act
            Action act = () => sut.Create(null);

            //assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void TestProbeCreator_Create_DoesNotThrowException()
        {
            //arrange
            TestProbeCreator sut = CreateTestProbeCreator();

            //act
            Action act = () => sut.Create(this);

            //assert
            act.ShouldNotThrow();
        }

        [Fact]
        public void TestProbeCreator_Create_ReturnsWorkingTestProbe()
        {
            //arrange
            TestProbeCreator sut = CreateTestProbeCreator();

            //act
            TestProbe result = sut.Create(this);

            //assert
            Guid guid = Guid.NewGuid();
            result.Tell(guid);
            result.ExpectMsg(guid);
        }
    }
}