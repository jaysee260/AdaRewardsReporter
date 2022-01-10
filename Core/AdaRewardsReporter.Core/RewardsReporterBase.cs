using AdaRewardsReporter.Core.Models;
using AdaRewardsReporter.Core.Utils;
using Blockfrost.Api;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;

namespace AdaRewardsReporter.Core;

public class RewardsReporterBase
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

    public async Task<decimal> GetRewardsSumAsync(string stakeAddress)
    {
        // TODO: Add test case
        try
        {
            var account = await _accountsService.GetAccountsAsync(stakeAddress);
            return RewardsReporterUtils.ConvertLovelacesToAda(long.Parse(account.RewardsSum));
        }
        catch (Exception exception)
        {
            if (exception.Message.Contains("404")) return 0;
            // Found this weird edge case where a stake_address may not be registered but still returns a non 404 response. For some reason, if this is the case, dotnet SDK is throwing an exception. No error when testing the call through Postman.
            if (exception.Message.Contains("Could not deserialize the response body stream as Blockfrost.Api.Models.AccountContentResponse")) return 0;
            throw;
        }
    }

    public async Task<AccountRewardContentResponseCollection> GetRewardsHistoryAsync(string stakeAddress, int? count = 100, int? page = 1, ESortOrder? order = 0)
    {
        // TODO: Add test case
        try
        {
            return await _accountsService.GetRewardsAsync(stakeAddress, count, page, order);
        }
        catch (Exception exception)
        {
            if (exception.Message.Contains("404")) return new AccountRewardContentResponseCollection();
            throw;
        }
    }

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

    public Dictionary<long, PoolMetadataResponse> BuildMapOfPoolPerEpoch(
        AccountRewardContentResponseCollection history,
        IEnumerable<PoolMetadataResponse> poolsMetadata
    )
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

    public IEnumerable<RewardsSummary> GenerateRewardsReport(
        AccountRewardContentResponseCollection history,
        List<EpochContentResponse> epochsMetadata,
        Dictionary<long, PoolMetadataResponse> mapOfPoolMetadataPerEpoch
    )
    {
        var numberOfEntries = history.Count() == epochsMetadata.Count()
            ? history.Count()
            : throw new Exception("Number of reward entries must be equal to the number of epochs.");

        // Create reward entries
        var rewardsReport = new List<RewardsSummary>();
        for (int i = 0; i < numberOfEntries; i++)
        {
            rewardsReport.Add(new RewardsSummary
            {
                Epoch = epochsMetadata[i].Epoch,
                Amount = RewardsReporterUtils.ConvertLovelacesToAda(long.Parse(history[i].Amount)),
                PayoutDate = RewardsReporterUtils.CalculatePayoutDate(epochsMetadata[i].EndTime)
            });
        }
        // Set Stake Pool description using pool name and ticker e.g. "ABC Pool - [ABC]"
        foreach (var entry in rewardsReport)
        {
            var stakePool = mapOfPoolMetadataPerEpoch[entry.Epoch];
            var stakePoolDescription = $"{stakePool.Name} [{stakePool.Ticker}]";
            entry.StakePool = stakePoolDescription;
        }

        return rewardsReport;
    }
}
