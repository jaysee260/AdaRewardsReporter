public interface IRewardsReporter
{
    Task<IEnumerable<RewardsSummary>> GenerateRewardsReportAsync(string stakeAddress);
}