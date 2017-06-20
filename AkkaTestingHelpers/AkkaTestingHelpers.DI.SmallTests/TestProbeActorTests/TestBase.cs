﻿using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using ConnelHooley.AkkaTestingHelpers.DI.Actors.Concrete;
using Xunit;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.TestProbeActorTests
{
    internal class TestBase : TestKit
    {
        protected object Message1;
        protected Type Type1;
        protected object Reply1;
        protected object Message2;
        protected Type Type2;
        protected object Reply2;
        protected Dictionary<Type, Func<object, object>> Handlers;

        public TestBase() : base(AkkaConfig.Config)
        {
            
            Message1 = new ExampleMessage1();
            Type1 = typeof(ExampleMessage1);
            Reply1 = TestUtils.Create<object>();
            Message2 = new ExampleMessage2();
            Type2 = typeof(ExampleMessage2);
            Reply2 = TestUtils.Create<object>();
            Handlers = new Dictionary<Type, Func<object, object>>
            {
                { Type1, o => Reply1 },
                { Type2, o => Reply2 }
            };
        }
        
        protected TestActorRef<TestProbeActor> CreateTestProbeActor() => 
            ActorOfAsTestActorRef<TestProbeActor>(
                Props.Create(() => new TestProbeActor(this)),
                TestUtils.Create<string>());

        private class ExampleMessage1 { }
        private class ExampleMessage2 { }
    }
}