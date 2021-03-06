﻿using Akka.Actor;
using Akka.TestKit;

namespace ConnelHooley.AkkaTestingHelpers.DI.Helpers.Abstract
{
    internal interface IChildTeller
    {
        void TellMessage<TMessage>(IChildWaiter childWaiter, TestKitBase testKit, IActorRef recipient, TMessage message, int waitForChildrenCount, IActorRef sender = null);
    }
}