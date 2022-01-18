// See https://aka.ms/new-console-template for more information

using AdaRewardsReporter.Core;
using Blockfrost.Api.Extensions;
using Blockfrost.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder().AddJsonFile("local.settings.json").Build();

var apiKey = configuration["BlockfrostApiKey"];
var network = configuration["CardanoNetwork"];
var stakeAddress = configuration["StakeAddress"];
var adaHandle = configuration["AdaHandle"];

var provider = new ServiceCollection().AddBlockfrost(network, apiKey).BuildServiceProvider();
var accountsService = provider.GetRequiredService<IAccountsService>();
var poolsService = provider.GetRequiredService<IPoolsService>();
var epochsService = provider.GetRequiredService<IEpochsService>();
var addressesService = provider.GetRequiredService<IAddressesService>();
var assetsService = provider.GetRequiredService<IAssetsService>();

var addressResolver = new AddressResolver(addressesService, assetsService);
var addy = await addressResolver.ResolveAddressFromHandle(adaHandle);
var resolvedStakeAddress = await addressResolver.ResolveStakeAddressFromAddress(addy);

var rewardsReporter = new RewardsReporter(accountsService, poolsService, epochsService);
var rewardsReport = await rewardsReporter.GetCompleteRewardsReportAsync(stakeAddress);
var rewardsSum = await rewardsReporter.GetRewardsSumAsync(stakeAddress);
Console.WriteLine("stop");