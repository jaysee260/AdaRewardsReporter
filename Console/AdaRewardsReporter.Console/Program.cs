// See https://aka.ms/new-console-template for more information

using AdaRewardsReporter.Core;
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Services;
using Microsoft.Extensions.DependencyInjection;

var apiKey = "mainnet9XPRZ9XDxEAKrFs21mRXz3AdepGENId1";
var network = "mainnet";
var stakeAddress = "stake1u8a2tx757nh9907vwaylh6kg64q0nx3t09qf0cs4tzf8hkcuegfax";

var provider = new ServiceCollection().AddBlockfrost(network, apiKey).BuildServiceProvider();
var accountsService = provider.GetRequiredService<IAccountsService>();
var poolsService = provider.GetRequiredService<IPoolsService>();
var epochsService = provider.GetRequiredService<IEpochsService>();

var rewardsReporter = new RewardsReporter(accountsService, poolsService, epochsService);
var rewardsReport = await rewardsReporter.GetCompleteRewardsReportAsync(stakeAddress);
Console.WriteLine("stop");