using AdaRewardsReporter.Api.Controllers;
using AdaRewardsReporter.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdaRewardsReporter.Api.Tests.Controllers;

public class RewardsControllerTests
{
    private readonly Mock<IRewardsReporter> _rewardsReporter;
    private readonly ILogger<RewardsController> _logger;
    private readonly RewardsController _rewardsController;

    public RewardsControllerTests()
    {
        _rewardsReporter = new Mock<IRewardsReporter>();
        _logger = new LoggerFactory().CreateLogger<RewardsController>();
        _rewardsController = new RewardsController(_rewardsReporter.Object, _logger);
    }

    [Fact]
    public void GetTotalRewardsAmount_GivenAStakeAddress_ShouldThrowNotImplementedException()
    {
        var invocation = () => _rewardsController.GetRewardsSum("test_stakeAddress123");
        invocation.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public void GetRewardsHistory_GivenAStakeAddress_ShouldThrowNotImplementedException()
    {
        var invocation = () => _rewardsController.GetRewardsHistory("test_stakeAddress123");
        invocation.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public void DownloadRewardsHistory_GivenAStakeAddress_ShouldThrowNotImplementedException()
    {
        var invocation = () => _rewardsController.DownloadRewardsHistory("test_stakeAddress123");
        invocation.Should().ThrowAsync<NotImplementedException>();
    }
}