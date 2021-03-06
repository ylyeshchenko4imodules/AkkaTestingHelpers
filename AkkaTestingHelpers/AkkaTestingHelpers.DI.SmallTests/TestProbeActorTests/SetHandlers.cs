﻿using System;
using System.Collections.Generic;
using Akka.TestKit;
using ConnelHooley.AkkaTestingHelpers.DI.Actors.Concrete;
using FluentAssertions;
using Xunit;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.TestProbeActorTests
{
    public class SetHandlers : TestBase
    {
        [Fact]
        public void TestProbeActor_SetHandlersWithNullHandlers_ThrowsArgumentNullException()
        {
            //arrange
            TestActorRef<TestProbeActor> sut = CreateTestProbeActor();

            //act
            Action act = () => sut.UnderlyingActor.SetHandlers(null);

            //assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void TestProbeActor_SetHandlers_SendsCorrectReplies()
        {
            //arrange
            TestActorRef<TestProbeActor> sut = CreateTestProbeActor();

            //act
            sut.UnderlyingActor.SetHandlers(Handlers);

            //assert
            sut.Tell(Message2);
            ExpectMsg(Reply2);
        }

        [Fact]
        public void TestProbeActor_SetHandlers_DoesNotSendReplyWhenNoHandlerMatches()
        {
            //arrange
            TestActorRef<TestProbeActor> sut = CreateTestProbeActor();

            //act
            sut.UnderlyingActor.SetHandlers(Handlers);

            //assert
            sut.Tell(TestUtils.Create<object>());
            ExpectNoMsg();
        }

        [Fact]
        public void TestProbeActor_SetHandlers_UsesLatestHandlers()
        {
            //arrange
            TestActorRef<TestProbeActor> sut = CreateTestProbeActor();
            object newReply = TestUtils.Create<object>();
            sut.UnderlyingActor.SetHandlers(Handlers);

            //act
            sut.UnderlyingActor.SetHandlers(new Dictionary<Type, Func<object, object>>
            {
                { Type1, o => Reply1 },
                { Type2, o => newReply }
            });

            //assert
            sut.Tell(Message2);
            ExpectMsg(newReply);
        }

        [Fact]
        public void TestProbeActor_SetHandlers_KeepsAutoPilotRunning()
        {
            //arrange
            TestActorRef<TestProbeActor> sut = CreateTestProbeActor();

            //act
            sut.UnderlyingActor.SetHandlers(Handlers);

            //assert
            sut.Tell(Message1);
            sut.Tell(Message2);
            ExpectMsgAllOf(Reply1, Reply2);
        }
    }
}