using System;

namespace StaticSwitch
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            ProviderSwitch.PickProvider(5, 5);
        }

        public enum ProviderType
        {
            Primary,
            Secondary
        }

        public static class ProviderSwitch
        {
            private static readonly object MyLock = new object();
            private static bool hasPrimaryFailed;
            private static DateTime? switchedAt;
            private static DateTime? primaryFailedDate;

            public static ProviderType PickProvider(int secondsBeforeSwitch, int secondsBeforeCheckingPrimary)
            {
                lock (MyLock)
                {
                    if (!hasPrimaryFailed)
                        return ProviderType.Primary;

                    if (switchedAt.HasValue)
                    {
                        var howLongSinceSwitched = DateTime.Now.Subtract(switchedAt.Value).TotalSeconds;
                        if (howLongSinceSwitched > secondsBeforeCheckingPrimary)
                        {
                            hasPrimaryFailed = false;
                            switchedAt = null;
                            return ProviderType.Primary;
                        }
                    }

                    if (!switchedAt.HasValue)
                        switchedAt = DateTime.Now;
                    return ProviderType.Secondary;
                }
            }

            public static void UpdateProviderSuccess(ProviderType providerType, bool success, int secondsBeforeRevertToSecondary)
            {
                lock (MyLock)
                {
                    if (providerType != ProviderType.Primary)
                        return;
                    if (success)
                    {
                        switchedAt = null;
                        hasPrimaryFailed = false;
                        return;
                    }
                    if (!primaryFailedDate.HasValue)
                        primaryFailedDate = DateTime.Now;

                    var secondsSinceFailed = DateTime.Now.Subtract(primaryFailedDate.Value).TotalSeconds;
                    if (secondsSinceFailed > secondsBeforeRevertToSecondary)
                    {
                        hasPrimaryFailed = true;
                        primaryFailedDate = null;
                    }
                }
            }
        }
    }
}
