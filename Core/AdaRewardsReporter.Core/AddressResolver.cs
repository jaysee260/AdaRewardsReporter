using Blockfrost.Api.Services;
using System.Text;

namespace AdaRewardsReporter.Core;

public class AddressResolver : IAddressResolver
{
    private readonly IAddressesService _addressesService;
    private readonly IAssetsService _assetsService;

    public const string AdaHandlePolicyId = "f0ff48bbb7bbe9d59a40f1ce90e9e9d0ff5002ec48f232b49ca0fb9a";

    public AddressResolver(
        IAddressesService addressesService,
        IAssetsService assetsService
    )
    {
        _addressesService = addressesService;
        _assetsService = assetsService;
    }

    public async Task<string> ResolveStakeAddressFromAddress(string address)
    {
        try
        {
            var addressInfo = await _addressesService.GetAddressesAsync(address);
            return addressInfo.StakeAddress;
        }
        catch (Exception exception)
        {
            if (exception.Message.Contains("404")) return null;
            throw;
        }
    }

    public async Task<string> ResolveAddressFromHandle(string handle)
    {
        if (!handle.StartsWith('$'))
        {
            throw new ArgumentException("Invalid handle. A handle must be prefixed by $");
        }

        var assetName = handle.Split('$').Last();
        var hexEncodedAssetName = HexEncode(assetName);
        var assetIdentifier = $"{AdaHandlePolicyId}{hexEncodedAssetName}";

        try
        {
            var assetAddressesResponse = await _assetsService.GetAddressesAsync(assetIdentifier);
            return assetAddressesResponse.First().Address;
        }
        catch (Exception exception)
        {
            if (exception.Message.Contains("404")) return null;
            throw;
        }
    }

    private string HexEncode(string value)
    {
        var bytes = Encoding.Default.GetBytes(value);
        var hexString = BitConverter.ToString(bytes);
        return hexString.Replace("-", "").ToLower();
    }
}