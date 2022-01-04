using Blockfrost.Api.Models;
using Blockfrost.Api.Services;

public class RewardsReporterBase : IRewardsReporterBase
{
    private readonly IAccountsService _accountsService;
    private readonly IPoolsService _poolsService;
    private readonly IEpochsService _epochsService;

    public RewardsReporterBase(
        IAccountsService accountsService,
        IPoolsService poolsService,
        IEpochsService epochsService
    )
    {
        _accountsService = accountsService;
        _poolsService = poolsService;
        _epochsService = epochsService;
    }

    public async Task<AccountRewardContentResponseCollection> GetRewardsHistoryAsync(string stakeAddress) =>
        await _accountsService.GetRewardsAsync(stakeAddress);

    public async Task<IEnumerable<EpochContentResponse>> GetEpochsMetadataAsync(IEnumerable<long> epochs)
    {
        var epochsMetadataTasks = epochs.Select(epoch => _epochsService.GetEpochsAsync((int)epoch));
        return await Task.WhenAll(epochsMetadataTasks);
    }

    public async Task<IEnumerable<PoolMetadataResponse>> GetPoolsMetadataAsync(IEnumerable<string> uniquePoolIds)
    {
        var poolsMetadataTasks = uniquePoolIds.Select(poolId => _poolsService.GetMetadataAsync(poolId));
        return await Task.WhenAll(poolsMetadataTasks);
    }

    public Dictionary<long, PoolMetadataResponse> BuildMapOfPoolPerEpoch(AccountRewardContentResponseCollection history, IEnumerable<PoolMetadataResponse> poolsMetadata)
    {
        var mapOfPoolIdPerEpoch = new Dictionary<long, string>();
        foreach (var reward in history)
        {
            mapOfPoolIdPerEpoch.Add(reward.Epoch, reward.PoolId);
        }

        var mapOfPoolMetadataPerEpoch = new Dictionary<long, PoolMetadataResponse>();
        var list = poolsMetadata.ToList();
        foreach (var set in mapOfPoolIdPerEpoch)
        {
            var poolMetadata = list.Find(x => x.PoolId.Equals(set.Value));
            mapOfPoolMetadataPerEpoch.Add(set.Key, poolMetadata);
        }

        return mapOfPoolMetadataPerEpoch;
    }

    public IEnumerable<RewardsSummary> GenerateReportRewardsReport(
        AccountRewardContentResponseCollection history,
        List<EpochContentResponse> epochsMetadata,
        Dictionary<long, PoolMetadataResponse> mapOfPoolMetadataPerEpoch
    )
    {
        var convertLovelacesToAda = (long lovelaces) => lovelaces * 0.000001M;
        // Rewards for a given epoch X are paid out at the end of the next epoch, X+1
        var calculatePayoutDate = (long epochEndTime) => DateTimeOffset.FromUnixTimeSeconds(epochEndTime).AddDays(5).DateTime.ToShortDateString();

        var numberOfEntries = history.Count() == epochsMetadata.Count()
            ? history.Count()
            : throw new Exception("Number of reward entries must be equal to the number of epochs");

        var rewardsReport = new List<RewardsSummary>();
        for (int i = 0; i < numberOfEntries; i++)
        {
            rewardsReport.Add(new RewardsSummary
            {
                Epoch = epochsMetadata[i].Epoch,
                Amount = convertLovelacesToAda(long.Parse(history[i].Amount)),
                PayoutDate = calculatePayoutDate(epochsMetadata[i].EndTime)
            });
        }
        foreach (var entry in rewardsReport)
        {
            var stakePool = mapOfPoolMetadataPerEpoch[entry.Epoch];
            var stakePoolDescriptionLabel = $"{stakePool.Name} [{stakePool.Ticker}]";
            entry.StakePool = stakePoolDescriptionLabel;
        }

        return rewardsReport;
    }
}