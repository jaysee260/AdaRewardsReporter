namespace AdaRewardsReporter.Core.Utils;

public static class RewardsReporterUtils
{
    public static decimal ConvertLovelacesToAda(long lovelaces) => lovelaces * 0.000001M;
    // Rewards for a given epoch X are paid out at the end of the next epoch, X+1
    public static string CalculatePayoutDate(long epochEndTime) => DateTimeOffset.FromUnixTimeSeconds(epochEndTime).AddDays(5).DateTime.ToShortDateString();
}