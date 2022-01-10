using AdaRewardsReporter.Core.Models;
using Blockfrost.Api;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;

namespace AdaRewardsReporter.Core;

public class RewardsReporter : RewardsReporterBase, IRewardsReporter
{
    public RewardsReporter(
        IAccountsService accountsService,
        IPoolsService poolsService,
        IEpochsService epochsService
    ) : base(accountsService, poolsService, epochsService)
    {
    }

    public new async Task<decimal> GetRewardsSumAsync(string stakeAddress)
    {
        return await base.GetRewardsSumAsync(stakeAddress);
    }

    public async Task<IEnumerable<RewardsSummary>> GetPaginatedRewardsReportAsync(string stakeAddress, int? count = 100, int? page = 1, ESortOrder? order = 0)
    {
        var history = await GetRewardsHistoryAsync(stakeAddress, count, page, order);
        // TODO: Add test case
        if (!history.Any()) return new List<RewardsSummary>();

        var epochs = await GetEpochsMetadataAsync(history.Select(x => x.Epoch));
        var pools = await GetPoolsMetadataAsync(history.Select(x => x.PoolId));
        var mapOfPoolPerEpoch = BuildMapOfPoolPerEpoch(history, pools);
        var rewardsReport = GenerateRewardsReport(history, epochs.ToList(), mapOfPoolPerEpoch);
        return rewardsReport;
    }

    public async Task<IEnumerable<RewardsSummary>> GetCompleteRewardsReportAsync(string stakeAddress, ESortOrder? order = 0)
    {
        var page = 1;
        var keepGoing = true;
        var history = new AccountRewardContentResponseCollection();
        while (keepGoing)
        {
            var results = await GetRewardsHistoryAsync(stakeAddress, count: 100, page, order);
            if (results is not null && results.Any())
            {
                // TODO: Figure out how to concat these two lists.
                // history.Concat(results) is throwing an error.
                foreach (var item in results)
                {
                    history.Add(item);
                }
                page++;
            }
            else
            {
                keepGoing = false;
            }
        }

        // TODO: Add test case
        if (!history.Any()) return new List<RewardsSummary>();
        
        var epochs = await GetEpochsMetadataAsync(history.Select(x => x.Epoch));
        var pools = await GetPoolsMetadataAsync(history.Select(x => x.PoolId));
        var mapOfPoolPerEpoch = BuildMapOfPoolPerEpoch(history, pools);
        var rewardsReport = GenerateRewardsReport(history, epochs.ToList(), mapOfPoolPerEpoch);
        return rewardsReport;
    }
}
