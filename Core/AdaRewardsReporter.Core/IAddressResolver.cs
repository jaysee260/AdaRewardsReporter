namespace AdaRewardsReporter.Core;

public interface IAddressResolver
{
    Task<string> ResolveStakeAddressFromAddress(string address);
    Task<string> ResolveAddressFromHandle(string handle);
}