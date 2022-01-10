using AdaRewardsReporter.Core;
using Microsoft.AspNetCore.Mvc;

namespace AdaRewardsReporter.Api.Controllers;

[ApiController]
[Route("resolve")]
[Produces("application/json")]
public class AddressResolutionController : ControllerBase
{
    private readonly IAddressResolver _addressResolver;
    private readonly ILogger<AddressResolutionController> _logger;

    public AddressResolutionController(
        IAddressResolver addressResolver,
        ILogger<AddressResolutionController> logger
    )
    {
        _addressResolver = addressResolver;
        _logger = logger;
    }

    [HttpGet("stakeAddress/fromAddress/{address}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> ResolveStakeAddressFromAddress(string address)
    {
        var stakeAddress = await _addressResolver.ResolveStakeAddressFromAddress(address);
        var response = new { stake_address = stakeAddress };
        return stakeAddress is not null ? Ok(response) : NotFound(response);
    }

    [HttpGet("address/fromHandle/{adaHandle}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> ResolveStakeAddressFromAdaHandle(string adaHandle)
    {
        if (!adaHandle.StartsWith("$"))
        {
            return BadRequest(new { message = "Invalid ADA Handle. Handles must be prefixed by $" });
        }
        var address = await _addressResolver.ResolveAddressFromHandle(adaHandle);
        var response = new { address };
        return address is not null ? Ok(response) : NotFound(response);
    }
}
