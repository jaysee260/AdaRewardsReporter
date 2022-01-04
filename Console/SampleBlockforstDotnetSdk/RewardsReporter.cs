using Blockfrost.Api.Services;

public class RewardsReporter : RewardsReporterBase, IRewardsReporter
{
    public RewardsReporter(
        IAccountsService accountsService,
        IPoolsService poolsService,
        IEpochsService epochsService
    ) : base(accountsService, poolsService, epochsService)
    {

    }

    public async Task<IEnumerable<RewardsSummary>> GenerateRewardsReportAsync(string stakeAddress)
    {
        var history = await GetRewardsHistoryAsync(stakeAddress);
        var epochs = await GetEpochsMetadataAsync(history.Select(x => x.Epoch));
        var pools = await GetPoolsMetadataAsync(history.Select(x => x.PoolId));
        var mapOfPoolPerEpoch = BuildMapOfPoolPerEpoch(history, pools);
        var rewardsReport = GenerateReportRewardsReport(history, epochs.ToList(), mapOfPoolPerEpoch);
        return rewardsReport;
    }
}