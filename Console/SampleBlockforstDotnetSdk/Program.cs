// See https://aka.ms/new-console-template for more information
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;
using Microsoft.Extensions.DependencyInjection;

var apiKey = "mainnet9XPRZ9XDxEAKrFs21mRXz3AdepGENId1";
var network = "mainnet";
var stakeAddress = "stake1u8a2tx757nh9907vwaylh6kg64q0nx3t09qf0cs4tzf8hkcuegfax";
var concurrentMode = true;
var convertLovelacesToAda = (long lovelaces) => lovelaces * 0.000001M;
// Rewards for a given epoch X are paid out at the end of the next epoch, X+1
var calculatePayoutDate = (long epochEndTime) => DateTimeOffset.FromUnixTimeSeconds(epochEndTime).AddDays(5).DateTime.ToShortDateString();

var provider = new ServiceCollection().AddBlockfrost(network, apiKey).BuildServiceProvider();
var accountsService = provider.GetRequiredService<IAccountsService>();
var poolsService = provider.GetRequiredService<IPoolsService>();
var epochsService = provider.GetRequiredService<IEpochsService>();

// Get rewards history
var history = await accountsService.GetRewardsAsync(stakeAddress);

// Get metadata of each pool that's been delegated to.
var uniquePoolIds = history.Select(x => x.PoolId).Distinct();
//var poolsMetadataTasks = uniquePoolIds.Select(poolId => poolsService.GetMetadataAsync(poolId));
//var poolsMetadata = await Task.WhenAll(poolsMetadataTasks);
List<PoolMetadataResponse> poolsMetadata;
if (concurrentMode)
{
    var poolsMetadataTasks = uniquePoolIds.Select(poolId => poolsService.GetMetadataAsync(poolId));
    poolsMetadata = (await Task.WhenAll(poolsMetadataTasks)).ToList();
}
else
{
    poolsMetadata = new List<PoolMetadataResponse>();
    foreach (var poolId in uniquePoolIds)
        poolsMetadata.Add(await poolsService.GetMetadataAsync(poolId));
}

// Create map of pool per epoch. Basically a table of which pool stake was delegated to during each epoch.
var mapOfPoolIdPerEpoch = new Dictionary<long, string>();
foreach (var reward in history)
{
    mapOfPoolIdPerEpoch.Add(reward.Epoch, reward.PoolId);
}

var mapOfPoolMetadataPerEpoch = new Dictionary<long, PoolMetadataResponse>();
foreach (var set in mapOfPoolIdPerEpoch)
{
    var poolMetadata = poolsMetadata.ToList().Find(x => x.PoolId.Equals(set.Value));
    mapOfPoolMetadataPerEpoch.Add(set.Key, poolMetadata);
}

// Get epochs metadata. We need the end_time of each epoch to calculate payout date.
var epochs = history.Select(x => x.Epoch);
//var epochsMetadataTasks = epochs.Select(epoch => epochsService.GetEpochsAsync((int)epoch));
//var epochsMetadata = await Task.WhenAll(epochsMetadataTasks);

List<EpochContentResponse> epochsMetadata;
if (concurrentMode)
{
    var epochsMetadataTasks = epochs.Select(epoch => epochsService.GetEpochsAsync((int)epoch));
    epochsMetadata = (await Task.WhenAll(epochsMetadataTasks)).ToList();
}
else
{
    epochsMetadata = new List<EpochContentResponse>();
    foreach (var epoch in epochs)
        epochsMetadata.Add(await epochsService.GetEpochsAsync((int)epoch));
}

// Generate rewards report
// Collections are expected to be in asc or desc order, but not a mix of both.
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

var totalRewardsEarned = history.Select(x => x.Amount).Select(long.Parse).Sum();
var totalRewardsEarnedInAda = convertLovelacesToAda(totalRewardsEarned);
Console.WriteLine($"Total Rewards Earned: {totalRewardsEarnedInAda} ADA");