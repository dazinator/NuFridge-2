using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentScheduler;
using Moq;
using NuFridge.Shared.Server.Modules;
using NuFridge.Shared.Server.Scheduler.Jobs;
using NuFridge.Shared.Server.Scheduler.Jobs.Tasks;
using NUnit.Framework;

namespace NuFridge.Tests
{
    [TestFixture]
    public class SchedulerTests
    {
        private IContainer _container;

        [Test]
        [TestCase(typeof(StatisticJobs))]
        [TestCase(typeof(PackageJobs))]
        public void IsJobRegistered(Type type)
        {
            //It should not be registered under the type, but under the IJob interface.
            var isRegistered = _container.IsRegistered(type);
            Assert.IsFalse(isRegistered);

            var jobs = _container.Resolve<IEnumerable<IJob>>();
            Assert.NotNull(jobs);

            var job = jobs.Single(jb => jb.GetType() == type);
            Assert.NotNull(job);

            Mock<Registry> registry = new Mock<Registry>();
            job.AddSchedule(registry.Object);

            Assert.DoesNotThrow(delegate { job.AddSchedule(registry.Object); });
        }

        [Test]
        [TestCase(typeof(UpdateSystemInformationTask))]
        [TestCase(typeof(UpdateFeedDownloadCountStatisticTask))]
        [TestCase(typeof(UpdateFeedPackageCountStatisticTask))]
        [TestCase(typeof(RunPackageRetentionPoliciesTask))]
        public void IsTaskRegistered(Type type)
        {
            var isRegistered = _container.IsRegistered(type);
            Assert.IsTrue(isRegistered);
        }

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new SchedulerModule());

            _container = builder.Build();
        }
    }
}