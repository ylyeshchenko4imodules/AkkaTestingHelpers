﻿using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.TestKit;

namespace ConnelHooley.AkkaTestingHelpers.DI
{
    public abstract class ResolverBase : IDependencyResolver
    {
        protected readonly TestKitBase TestKit;
        private readonly object _waitLock;
        private TestLatch _waitForChildren;
        
        protected ResolverBase(TestKitBase testKit)
        {
            TestKit = testKit;
            TestKit.Sys.AddDependencyResolver(this);
            _waitLock = new object();
        }

        public TestActorRef<TActor> CreateSut<TActor>(int expectedChildrenCount = 1) where TActor : ActorBase => 
            CreateSut<TActor>(Props.Create<TActor>(), expectedChildrenCount);

        public TestActorRef<TActor> CreateSut<TActor>(Props props, int expectedChildrenCount = 1) where TActor : ActorBase
        {
            TestActorRef<TActor> sut = null;
            if (expectedChildrenCount < 1)
            {
                sut = TestKit.ActorOfAsTestActorRef<TActor>(props);
            }
            else
            {
                WaitForChildren(() =>
                {
                    sut = TestKit.ActorOfAsTestActorRef<TActor>(props);
                }, expectedChildrenCount);
            }
            return sut;
        }

        public void WaitForChildren(Action act, int expectedChildrenCount)
        {
            if (expectedChildrenCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedChildrenCount), "Cannot be less than 1");
            }
            lock (_waitLock)
            {
                _waitForChildren = TestKit.CreateTestLatch(expectedChildrenCount);
                act();
                _waitForChildren.Ready();
                _waitForChildren = null;
            }
        }
        
        protected abstract ActorBase Resolve(Type actorType);

        #region IDependencyResolver implementation

        Func<ActorBase> IDependencyResolver.CreateActorFactory(Type actorType) => 
            () =>
            {
                ActorBase actor = Resolve(actorType);
                _waitForChildren?.CountDown();
                return actor;
            };

        Type IDependencyResolver.GetType(string actorName) => 
            throw new NotImplementedException();

        Props IDependencyResolver.Create<TActor>() => 
            throw new NotImplementedException();

        Props IDependencyResolver.Create(Type actorType) => 
            throw new NotImplementedException();

        void IDependencyResolver.Release(ActorBase actor)
        {
             //No implementation needed
        }

        #endregion
    }
}