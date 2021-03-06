﻿using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.TestKit;
using ConnelHooley.AkkaTestingHelpers.DI.Actors.Abstract;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Abstract;

namespace ConnelHooley.AkkaTestingHelpers.DI
{
    public sealed class TestProbeResolver
    {
        private readonly ISutCreator _sutCreator;
        private readonly IChildTeller _childTeller;
        private readonly IChildWaiter _childWaiter;
        private readonly IResolvedTestProbeStore _resolvedProbeStore;
        private readonly ITestProbeActorCreator _actorCreator;
        private readonly TestKitBase _testKit;
        private readonly ImmutableDictionary<Type, ImmutableDictionary<Type, Func<object, object>>> _handlers;

        internal TestProbeResolver(
            IDependencyResolverAdder resolverAdder, 
            ISutCreator sutCreator, 
            IChildTeller childTeller, 
            IChildWaiter childWaiter, 
            IResolvedTestProbeStore resolvedProbeStore, 
            ITestProbeCreator testProbeCreator, 
            ITestProbeActorCreator testProbeActorCreator, 
            ITestProbeHandlersMapper handlersMapper, 
            TestKitBase testKit, 
            ImmutableDictionary<(Type, Type), Func<object, object>> handlers)
        {
            _sutCreator = sutCreator;
            _childTeller = childTeller;
            _childWaiter = childWaiter;
            _resolvedProbeStore = resolvedProbeStore;
            _actorCreator = testProbeActorCreator;
            _testKit = testKit;
            _handlers = handlersMapper.Map(handlers);
            Supervisor = testProbeCreator.Create(testKit);

            resolverAdder.Add(testKit, Resolve);
        }

        /// <summary>
        /// The TestProbe that is the parent/superivsor for all actors created using the CreateSut method.
        /// </summary>
        public TestProbe Supervisor { get; }

        /// <summary>
        /// Finds the test probe created by the given actor ref with the given child name.
        /// </summary>
        /// <param name="parentActor">The actor this is parent to the child</param>
        /// <param name="childName">The name of the child</param>
        /// <returns></returns>
        public TestProbe ResolvedTestProbe(IActorRef parentActor, string childName) =>
            _resolvedProbeStore.FindResolvedTestProbe(parentActor, childName);

        /// <summary>
        /// Finds the Type of actor the given actor ref requested for the given child name.
        /// </summary>
        /// <param name="parentActor">The actor this is parent to the child</param>
        /// <param name="childName">The name of the child</param>
        /// <returns></returns>
        public Type ResolvedType(IActorRef parentActor, string childName) =>
            _resolvedProbeStore.FindResolvedType(parentActor, childName);

        /// <summary>
        /// Creates an actor whilst waiting for the expected number of children to be resolved before returning.
        /// </summary>
        /// <typeparam name="TActor">The type of actor to create</typeparam>
        /// <param name="props">Props object used to create the actor</param>
        /// <param name="expectedChildrenCount">The number child actors to wait for</param>
        /// <returns>The created actor</returns>
        public TestActorRef<TActor> CreateSut<TActor>(Props props, int expectedChildrenCount) where TActor : ActorBase =>
            _sutCreator.Create<TActor>(
                _childWaiter,
                _testKit,
                props,
                expectedChildrenCount,
                Supervisor);

        /// <summary>
        /// Creates an actor whilst waiting for the expected number of children to be resolved before returning.
        /// </summary>
        /// <typeparam name="TActor">The type of actor to create</typeparam>
        /// <param name="expectedChildrenCount">The number child actors to wait for</param>
        /// <returns>The created actor</returns>
        public TestActorRef<TActor> CreateSut<TActor>(int expectedChildrenCount) where TActor : ActorBase, new() =>
            _sutCreator.Create<TActor>(
                _childWaiter,
                _testKit,
                Props.Create<TActor>(),
                expectedChildrenCount,
                Supervisor);

        /// <summary>
        /// Sends an actor a message whilst waiting for the expected number of children to be resolved before returning.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to send</typeparam>
        /// <param name="recipient">The actor to send a message to</param>
        /// <param name="message">The message to send</param>
        /// <param name="expectedChildrenCount">The number child actors to wait for</param>
        public void TellMessage<TMessage>(IActorRef recipient, TMessage message, int waitForChildrenCount) =>
            _childTeller.TellMessage(
                _childWaiter,
                _testKit,
                recipient,
                message,
                waitForChildrenCount);

        /// <summary>
        /// Sends an actor a message whilst waiting for the expected number of children to be resolved before returning.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient">The actor to send a message to</param>
        /// <param name="message">The message to send</param>
        /// <param name="sender">The actor to send the message from</param>
        /// <param name="expectedChildrenCount">The number child actors to wait for</param>
        public void TellMessage<TMessage>(IActorRef recipient, TMessage message, IActorRef sender, int waitForChildrenCount) =>
            _childTeller.TellMessage(
                _childWaiter,
                _testKit,
                recipient,
                message,
                waitForChildrenCount,
                sender);

        private ActorBase Resolve(Type actorType)
        {
            ITestProbeActor probeActor = _actorCreator.Create(_testKit);
            if (_handlers.ContainsKey(actorType))
            {
                probeActor.SetHandlers(_handlers[actorType]);
            }
            _resolvedProbeStore.ResolveProbe(probeActor.ActorPath, actorType, probeActor.TestProbe);
            _childWaiter.ResolvedChild();
            return probeActor.Actor;
        }
    }
}