using Blockfrost.Api;
using Blockfrost.Api.Models;
using Blockfrost.Api.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdaRewardsReporter.Core.Tests;

public class RewardsReporterBaseTests
{
    private readonly Mock<IAccountsService> accountsService;
    private readonly Mock<IPoolsService> poolsService;
    private readonly Mock<IEpochsService> epochsService;
    private readonly RewardsReporterBase rewardsReporterBase;

    public RewardsReporterBaseTests()
    {
        accountsService = new Mock<IAccountsService>();
        poolsService = new Mock<IPoolsService>();
        epochsService = new Mock<IEpochsService>();
        rewardsReporterBase = new RewardsReporterBase(accountsService.Object, poolsService.Object, epochsService.Object);
    }

    [Fact]
    public async Task GetRewardsSum_ShouldReturn_RewardsSumAsDecimal()
    {
        // Arrange
        accountsService.Setup(x => x.GetAccountsAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new AccountContentResponse { RewardsSum = "100000000" });

        // Act
        var response = await rewardsReporterBase.GetRewardsSumAsync("test_stakeAddress123abc");

        // Assert
        response.Should().BeOfType(typeof(decimal));
        response.Should().Be((decimal)100.00);
    }

    [Fact]
    public async Task GetRewardsHistoryAsync_ShouldReturn_AccountRewardContentResponseCollection()
    {
        // Arrange
        accountsService.Setup(x => x.GetRewardsAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<ESortOrder>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new AccountRewardContentResponseCollection());

        // Act
        var response = await rewardsReporterBase.GetRewardsHistoryAsync("test_stakeAddress123abc");

        // Assert
        response.Should().NotBeNull().And.BeOfType<AccountRewardContentResponseCollection>();
    }

    [Fact]
    public async Task GetEpochsMetadataAsync_ShouldReturn_ACollectionOfEpochContentResponse()
    {
        // Arrange
        var testEpochs = new long[] { 1, 2, 3 };
        epochsService.Setup(x => x.GetEpochsAsync(
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new EpochContentResponse());

        // Act
        var response = await rewardsReporterBase.GetEpochsMetadataAsync(testEpochs);

        // Assert
        response.Should().NotBeNull().And.ContainItemsAssignableTo<EpochContentResponse>();
        epochsService.Verify(
            x => x.GetEpochsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Exactly(testEpochs.Length)
        );
    }

    [Fact]
    public async Task GetPoolsMetadataAsync_ShouldReturn_ACollectionOfPoolsMetadataResponse()
    {
        // Arrange
        var testPools = new string[] { "pool_1", "pool_2", "pool_3" };
        poolsService.Setup(x => x.GetMetadataAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new PoolMetadataResponse());

        // Act
        var response = await rewardsReporterBase.GetPoolsMetadataAsync(testPools);

        // Assert
        response.Should().NotBeNull().And.ContainItemsAssignableTo<PoolMetadataResponse>();
        poolsService.Verify(
            x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(testPools.Length)
        );
    }

    [Fact]
    public void BuildMapOfPoolPerEpoch_ShouldReturn_AMapOfStakePoolPerEpoch()
    {
        // Arrange
        var testHistory = new AccountRewardContentResponseCollection();
        testHistory.Add(new AccountRewardContentResponse { Epoch = 1, Amount = "10000", PoolId = "pool_1" });
        testHistory.Add(new AccountRewardContentResponse { Epoch = 2, Amount = "10000", PoolId = "pool_1" });
        testHistory.Add(new AccountRewardContentResponse { Epoch = 3, Amount = "10000", PoolId = "pool_1" });
        testHistory.Add(new AccountRewardContentResponse { Epoch = 4, Amount = "10000", PoolId = "pool_2" });
        testHistory.Add(new AccountRewardContentResponse { Epoch = 5, Amount = "10000", PoolId = "pool_3" });
        testHistory.Add(new AccountRewardContentResponse { Epoch = 6, Amount = "10000", PoolId = "pool_3" });

        var testPools = new List<PoolMetadataResponse>();
        testPools.Add(new PoolMetadataResponse { PoolId = "pool_1", Name = "Test Pool 1", Ticker = "TPOOL1" });
        testPools.Add(new PoolMetadataResponse { PoolId = "pool_2", Name = "Test Pool 2", Ticker = "TPOOL2" });
        testPools.Add(new PoolMetadataResponse { PoolId = "pool_3", Name = "Test Pool 3", Ticker = "TPOOL3" });

        // Act
        var response = rewardsReporterBase.BuildMapOfPoolPerEpoch(testHistory, testPools);

        // Assert
        response.Should().NotBeNull();
        response.Should().HaveCount(testHistory.Count, because: "there should be one entry per epoch of delegation");
    }

    [Fact]
    public void GenerateRewardsReport()
    {
        throw new NotImplementedException();
    }
}