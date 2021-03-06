﻿using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Abstract;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Concrete;
using Moq;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.ChildTellerTests
{
    public class TestBase : TestKit
    {
        internal Mock<IChildWaiter> ChildWaiterMock;
        internal Mock<IActorRef> RecipientMock;

        internal List<string> CallOrder;

        internal IChildWaiter ChildWaiter;
        internal int ExpectedChildrenCount;
        internal object Message;
        internal IActorRef Recipient;
        internal IActorRef Sender;

        public TestBase() : base(AkkaConfig.Config)
        {
            // Create mocks
            ChildWaiterMock = new Mock<IChildWaiter>();
            RecipientMock = new Mock<IActorRef>();

            // Create objects used by mocks
            CallOrder = new List<string>();

            // Create objects passed into sut
            ChildWaiter = ChildWaiterMock.Object;
            ExpectedChildrenCount = TestUtils.Create<int>();
            Message = TestUtils.Create<object>();
            Recipient = RecipientMock.Object;
            Sender = CreateTestProbe();

            // Set up mocks
            ChildWaiterMock
                .Setup(waiter => waiter.Start(this, ExpectedChildrenCount))
                .Callback(() => CallOrder.Add(nameof(IChildWaiter.Start)));
            ChildWaiterMock
                .Setup(waiter => waiter.Wait())
                .Callback(() => CallOrder.Add(nameof(IChildWaiter.Wait)));
            ChildWaiterMock
                .Setup(waiter => waiter.ResolvedChild())
                .Callback(() => CallOrder.Add(nameof(IChildWaiter.ResolvedChild)));

            RecipientMock
                .Setup(waiter => waiter.Tell(Message, TestActor))
                .Callback(() => CallOrder.Add(nameof(IActorRef.Tell)));
            RecipientMock
                .Setup(waiter => waiter.Tell(Message, Sender))
                .Callback(() => CallOrder.Add(nameof(IActorRef.Tell) + "Sender"));
        }
        
        internal ChildTeller CreateChildTeller() => new ChildTeller();
    }
}