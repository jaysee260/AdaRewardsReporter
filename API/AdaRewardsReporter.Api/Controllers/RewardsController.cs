using AdaRewardsReporter.Core;
using AdaRewardsReporter.Core.Models;
using Blockfrost.Api;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Globalization;

namespace AdaRewardsReporter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RewardsController : ControllerBase
{

    private readonly IRewardsReporter _rewardsReporter;
    private readonly ILogger<RewardsController> _logger;

    public RewardsController(
        IRewardsReporter rewardsReporter,
        ILogger<RewardsController> logger
    )
    {
        _rewardsReporter = rewardsReporter;
        _logger = logger;
    }

    [HttpGet("{stakeAddress}/sum")]
    public async Task<decimal> GetRewardsSum(string stakeAddress)
    {
        _logger.LogDebug($"stake address: {stakeAddress}");
        return await _rewardsReporter.GetRewardsSumAsync(stakeAddress);
    }

    [HttpGet("{stakeAddress}/report")]
    public async Task<IEnumerable<RewardsSummary>> GetRewardsHistory(
        string stakeAddress,
        [FromQuery]int? count = 100,
        [FromQuery]int? page = 1,
        [FromQuery]ESortOrder? order = 0
    )
    {
        _logger.LogDebug($"stake address: {stakeAddress}");
        return await _rewardsReporter.GetPaginatedRewardsReportAsync(stakeAddress, count, page, order);
    }

    [HttpGet("{stakeAddress}/report/download")]
    public async Task<FileContentResult> DownloadRewardsHistory(string stakeAddress)
    {
        _logger.LogDebug($"stake address: {stakeAddress}");
        var rewardsReport = await _rewardsReporter.GetCompleteRewardsReportAsync(stakeAddress);
        await using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync((IEnumerable)rewardsReport);
        await streamWriter.FlushAsync();
        var fileContents = memoryStream.ToArray();
        return File(fileContents, "text/csv", "rewardsReport.csv");
    }
}
