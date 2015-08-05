using System;

namespace NuFridge.Shared.Server.FileSystem
{
    public class DeletionOptions : IEquatable<DeletionOptions>
    {
        public static DeletionOptions TryThreeTimes
        {
            get
            {
                return new DeletionOptions
                {
                    RetryAttempts = 3,
                    ThrowOnFailure = true
                };
            }
        }

        public static DeletionOptions TryThreeTimesIgnoreFailure
        {
            get
            {
                return new DeletionOptions
                {
                    RetryAttempts = 3,
                    ThrowOnFailure = false
                };
            }
        }

        public int RetryAttempts { get; set; }

        public int SleepBetweenAttemptsMilliseconds { get; }

        public bool ThrowOnFailure { get; set; }

        private DeletionOptions()
        {
            SleepBetweenAttemptsMilliseconds = 100;
        }

        public static bool operator ==(DeletionOptions left, DeletionOptions right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DeletionOptions left, DeletionOptions right)
        {
            return !Equals(left, right);
        }

        public bool Equals(DeletionOptions other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (RetryAttempts == other.RetryAttempts && SleepBetweenAttemptsMilliseconds == other.SleepBetweenAttemptsMilliseconds)
                return ThrowOnFailure.Equals(other.ThrowOnFailure);
            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((DeletionOptions)obj);
        }

        public override int GetHashCode()
        {
            return (RetryAttempts * 397 ^ SleepBetweenAttemptsMilliseconds) * 397 ^ ThrowOnFailure.GetHashCode();
        }
    }
}
