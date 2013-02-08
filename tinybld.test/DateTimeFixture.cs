namespace RobMensching.TinyBuild.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class DateTimeFixture
    {
        [Fact]
        public void CanAddTimeSpan()
        {
            DayOfWeek d = DayOfWeek.Friday;
            TimeSpan t = new TimeSpan(14, 30, 0);
            DateTime dt = new DateTime(2013, 2, 4, 15, 35, 12);

            int delta = Math.Abs((int)d - (int)dt.DayOfWeek);
            Assert.Equal(4, delta);
            Assert.Equal(DayOfWeek.Friday, dt.AddDays(4).DayOfWeek);
            TimeSpan deltaT = dt.TimeOfDay - t;
            DateTime dd = dt.Subtract(deltaT);
            Assert.Equal(14, dd.Hour);
            Assert.Equal(30, dd.Minute);
            Assert.Equal(0, dd.Second);
        }

        [Fact]
        public void CanAddAll()
        {
            DayOfWeek d = DayOfWeek.Thursday;
            TimeSpan t = new TimeSpan(14, 30, 0);

            DateTime start = new DateTime(2013, 2, 4, 15, 35, 12);
            DateTime end =   new DateTime(2013, 2, 7, 14, 30, 0);

            DateTime actual = start.AddDays(Math.Abs(d - start.DayOfWeek)).Subtract(start.TimeOfDay - t);
            Assert.Equal(end, actual);
        }

        [Fact]
        public void CanParseTimeSpan()
        {
            TimeSpan t;
            Assert.True(TimeSpan.TryParse("12:02:01", out t));
            Assert.Equal(new TimeSpan(12, 2, 1), t); 
        }
    }
}
