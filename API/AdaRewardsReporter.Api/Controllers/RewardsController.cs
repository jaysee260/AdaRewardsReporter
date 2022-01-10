using AdaRewardsReporter.Core;
using AdaRewardsReporter.Core.Models;
using Blockfrost.Api;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Globalization;

namespace AdaRewardsReporter.Api.Controllers;

[ApiController]
[Route("rewards")]
[Produces("application/json")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetRewardsSum(string stakeAddress)
    {
        _logger.LogInformation($"stake address: {stakeAddress}");
        var rewardsSum = await _rewardsReporter.GetRewardsSumAsync(stakeAddress);
        // TODO: Add proper response models
        var response = new { rewards_sum = rewardsSum };
        return rewardsSum is not 0 ? Ok(response) : NotFound(response);
    }

    [HttpGet("{stakeAddress}/report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RewardsSummary>>> GetRewardsHistory(
        string stakeAddress,
        [FromQuery]int? count = 100,
        [FromQuery]int? page = 1,
        [FromQuery]ESortOrder? order = 0
    )
    {
        _logger.LogInformation($"stake address: {stakeAddress}");
        var response = await _rewardsReporter.GetPaginatedRewardsReportAsync(stakeAddress, count, page, order);
        return response.Any() ? Ok(response) : NotFound(response);
    }

    [HttpGet("{stakeAddress}/report/download")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<FileContentResult> DownloadRewardsHistory(string stakeAddress)
    {
        _logger.LogInformation($"stake address: {stakeAddress}");
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
