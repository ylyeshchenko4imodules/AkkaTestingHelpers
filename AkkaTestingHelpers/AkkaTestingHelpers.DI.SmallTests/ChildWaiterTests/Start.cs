﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Concrete;
using FluentAssertions;
using NUnit.Framework;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.ChildWaiterTests
{
    internal class Start : TestBase
    {
        [Test]
        public void ChildWaiter_StartWithNullTestKitBase_ThrowsArgumentNullException()
        {
            //arrange
            ChildWaiter sut = CreateChildWaiter();

            //act
            Action act = () => sut.Start(null, TestUtils.Create<int>());

            //assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ChildWaiter_Start_DoesNotThrowAnyExceptions()
        {
            //arrange
            ChildWaiter sut = CreateChildWaiter();

            //act
            Action act = () => sut.Start(this, TestUtils.Create<int>());

            //assert
            act.ShouldNotThrow();
        }
        
        [Test]
        [Timeout(2000)]
        public void ChildWaiter_Start_Started_ShouldBlockThread()
        {
            //arrange
            ChildWaiter sut = CreateChildWaiter();
            int expectedChildrenCount = TestUtils.RandomBetween(0, 5);
            bool isSecondStartRan = false;
            sut.Start(this, expectedChildrenCount);
            
            Task.Run(() =>
            {
                //act
                sut.Start(this, TestUtils.RandomBetween(0, 5));
                isSecondStartRan = true;
            });
            
            //assert
            Thread.Sleep(100);
            isSecondStartRan.Should().BeFalse();
        }

        [Test]
        [Timeout(2000)]
        public void ChildWaiter_Start_Started_ShouldUnblockThreadWhenFirstStartsChildrenAreResolved()
        {
            //arrange
            ChildWaiter sut = CreateChildWaiter();
            int expectedChildrenCount = TestUtils.RandomBetween(0, 5);
            bool isSecondStartRan = false;
            sut.Start(this, expectedChildrenCount);

            Task.Run(() =>
            {
                //act
                sut.Start(this, TestUtils.RandomBetween(0, 5));
                isSecondStartRan = true;
            });
            
            //assert
            Task.Run(() =>
            {
                Thread.Sleep(100);
                Parallel.For(0, expectedChildrenCount, i =>
                {
                    sut.ResolvedChild();
                });
            }); 
            sut.Wait();
            Thread.Sleep(50);
            isSecondStartRan.Should().BeTrue();
        }
    }
}