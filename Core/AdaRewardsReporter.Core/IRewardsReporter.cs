using AdaRewardsReporter.Core.Models;
using Blockfrost.Api;

namespace AdaRewardsReporter.Core;

public interface IRewardsReporter
{
    Task<decimal> GetRewardsSumAsync(string stakeAddress);
    Task<IEnumerable<RewardsSummary>> GetPaginatedRewardsReportAsync(string stakeAddress, int? count = 100, int? page = 1, ESortOrder? order = 0);
    Task<IEnumerable<RewardsSummary>> GetCompleteRewardsReportAsync(string stakeAddress, ESortOrder? order = 0);
}