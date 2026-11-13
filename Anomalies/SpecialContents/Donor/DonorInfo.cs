namespace Anomalies.SpecialContents.Donor;

public record struct DonationInfo(DateOnly Date, decimal Price, string Message);

public record struct DonorInfo(string Name, List<DonationInfo> Donations)
{
    /// <summary>
    /// 存储规则：
    /// <para/><list type="bullet">
    /// <item/><description/>次序：捐赠时间顺序排列，最早的在前。
    /// <item/><description/>存储内容：捐赠时间、捐赠金额、留言。
    /// </list>
    /// </summary>
    public static Dictionary<int, DonorInfo> DonorsByID = new()
    {
        [1] = new DonorInfo("UAE丶黑桃", [new DonationInfo(new DateOnly(2026, 9, 11), 400.00m, "")]),
        [2] = new DonorInfo("阿辰Xc", [ new DonationInfo(new DateOnly(2026, 9, 11), 200.00m, "But when one story ends, another one begins."),
            new DonationInfo(new DateOnly(2026, 10, 15), 100.00m, "") ]),
        [3] = new DonorInfo("白骨清明", [new DonationInfo(new DateOnly(2026, 9, 11), 300.00m, "还窝mega再生力！！")]),
        [4] = new DonorInfo("噬魂幽fa", [new DonationInfo(new DateOnly(2026, 9, 11), 99.61m, "")]),
        [5] = new DonorInfo("东山之上", [new DonationInfo(new DateOnly(2026, 9, 11), 200.00m, "")]),
    };

    /// <summary>
    /// 单个捐赠者的总捐赠金额。
    /// </summary>
    public readonly decimal TotalDonation => Donations.Sum(d => d.Price);

    /// <summary>
    /// 所有捐赠者的总捐赠金额。
    /// </summary>
    public static decimal TotalDonations => DonorsByID.Values.Sum(d => d.TotalDonation);
}
