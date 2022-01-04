using Blockfrost.Api.Models;

public interface IRewardsReporterBase
{
    Task<AccountRewardContentResponseCollection> GetRewardsHistoryAsync(string stakeAddress);
    Task<IEnumerable<EpochContentResponse>> GetEpochsMetadataAsync(IEnumerable<long> epochs);
    Task<IEnumerable<PoolMetadataResponse>> GetPoolsMetadataAsync(IEnumerable<string> poolIds);
    Dictionary<long, PoolMetadataResponse> BuildMapOfPoolPerEpoch(AccountRewardContentResponseCollection rewardHistory, IEnumerable<PoolMetadataResponse> poolsMetadata);
}