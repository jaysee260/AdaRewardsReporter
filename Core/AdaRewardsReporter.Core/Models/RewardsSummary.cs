namespace AdaRewardsReporter.Core.Models;

public class RewardsSummary
{
    public long Epoch { get; set; }
    public string PayoutDate { get; set; }
    public decimal Amount { get; set; }
    public string StakePool { get; set; }
}