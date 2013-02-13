namespace RobMensching.TinyBuild.Tests
{
    using System;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;
    using Xunit;

    public class BuildManagerFixture
    {
        [Fact]
        public void CanCalculateNextBuild()
        {
            var bm = new BuildManager()
            {
                Config = new BuildConfiguration()
                {
                    Day = DayOfWeek.Friday,
                    Time = new TimeSpan(14, 30, 0),
                },
                Data = new BuildData()
                {
                    LastBuild = new DateTime(2013, 2, 4, 16, 12, 59),
                },
            };

            Assert.Equal(new DateTime(2013, 2, 8, 14, 30, 0), bm.CalculateNextBuildTime());
        }

        [Fact]
        public void CanCalculateNextBuildWithNoDayNoTime()
        {
            var bm = new BuildManager()
            {
                Config = new BuildConfiguration()
                {
                },
                Data = new BuildData()
                {
                    LastBuild = new DateTime(2013, 2, 4, 16, 12, 59),
                },
            };

            Assert.Equal(new DateTime(2013, 2, 4, 16, 12, 59), bm.CalculateNextBuildTime());
        }

        [Fact]
        public void CanCalculateNextBuildWithOnlyDay()
        {
            var bm = new BuildManager()
            {
                Config = new BuildConfiguration()
                {
                    Day = DayOfWeek.Friday,
                },
                Data = new BuildData()
                {
                    LastBuild = new DateTime(2013, 2, 4, 16, 12, 59),
                },
            };

            Assert.Equal(new DateTime(2013, 2, 8, 16, 12, 59), bm.CalculateNextBuildTime());
        }

        [Fact]
        public void CanCalculateNextBuildForTodayWithNoDay()
        {
            var bm = new BuildManager()
            {
                Config = new BuildConfiguration()
                {
                    Time = new TimeSpan(14, 30, 0),
                },
                Data = new BuildData()
                {
                    LastBuild = new DateTime(2013, 2, 4, 10, 12, 59),
                },
            };

            Assert.Equal(new DateTime(2013, 2, 4, 14, 30, 0), bm.CalculateNextBuildTime());
        }

        [Fact]
        public void CanCalculateNextBuildForTommorrowNoDay()
        {
            var bm = new BuildManager()
            {
                Config = new BuildConfiguration()
                {
                    Time = new TimeSpan(14, 30, 0),
                },
                Data = new BuildData()
                {
                    LastBuild = new DateTime(2013, 2, 4, 16, 12, 59),
                },
            };

            Assert.Equal(new DateTime(2013, 2, 5, 14, 30, 0), bm.CalculateNextBuildTime());
        }

        [Fact]
        public void CanDetectBuildOutOfDate()
        {
            var bm = new BuildManager()
            {
                Config = new BuildConfiguration()
                {
                    Time = new TimeSpan(14, 30, 0),
                },
                Data = new BuildData()
                {
                    LastBuild = new DateTime(2013, 2, 4, 16, 12, 59),
                },
            };

            Assert.False(bm.BuildOutOfDate(new DateTime(2013, 2, 4, 14, 30, 00)));
            Assert.False(bm.BuildOutOfDate(new DateTime(2013, 2, 5, 14, 29, 59)));
            Assert.True(bm.BuildOutOfDate(new DateTime(2013, 2, 5, 14, 30, 00)));
            Assert.True(bm.BuildOutOfDate(new DateTime(2013, 2, 6, 14, 30, 00)));
        }
    }
}
