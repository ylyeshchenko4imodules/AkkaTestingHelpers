﻿using System;
using System.Threading.Tasks;
using ConnelHooley.AkkaTestingHelpers.DI.Helpers.Concrete;
using FluentAssertions;
using Xunit;

namespace ConnelHooley.AkkaTestingHelpers.DI.SmallTests.ChildWaiterTests
{
    public class Start : TestBase
    {
        [Fact]
        public void ChildWaiter_StartWithNullTestKitBase_ThrowsArgumentNullException()
        {
            Within(TimeSpan.FromSeconds(2), () =>
            {
                //arrange
                ChildWaiter sut = CreateChildWaiter();

                //act
                Action act = () => sut.Start(null, TestUtils.Create<int>());

                //assert
                act.ShouldThrow<ArgumentNullException>();
            });
        }

        [Fact]
        public void ChildWaiter_Start_DoesNotThrowAnyExceptions()
        {
            Within(TimeSpan.FromSeconds(2), () =>
            {
                //arrange
                ChildWaiter sut = CreateChildWaiter();

                //act
                Action act = () => sut.Start(this, TestUtils.Create<int>());

                //assert
                act.ShouldNotThrow();
            });
        }
        
        [Fact]
        public void ChildWaiter_Started_Start_ShouldBlockThread()
        {
            Within(TimeSpan.FromSeconds(2), () =>
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
                this.Sleep(TestKitSettings.DefaultTimeout);
                isSecondStartRan.Should().BeFalse();
            });
        }

        [Fact]
        public void ChildWaiter_Started_Start_ShouldUnblockThreadWhenFirstStartsChildrenAreResolved()
        {
            Within(TimeSpan.FromSeconds(2), () =>
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
                this.Sleep(50); //ensures start is called before continuing

                //assert
                Task.Run(() =>
                {
                    Parallel.For(0, expectedChildrenCount, i =>
                    {
                        sut.ResolvedChild();
                    });
                });
                sut.Wait();
                AwaitAssert(() => isSecondStartRan.Should().BeTrue());
            });
        }
    }
}