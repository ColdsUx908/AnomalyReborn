namespace Anomalies.SpecialContents.Donor;

public record struct DonorInfo(string Name, decimal Price, string Message = "")
{
    public static Dictionary<int, DonorInfo> DonorsByID = new()
    {
        [1] = new DonorInfo("UAE丶黑桃", 400.00m, ""),
        [2] = new DonorInfo("阿辰Xc", 100.00m, "But when one story ends, another one begins."),
        [3] = new DonorInfo("白骨清明", 300.00m, "还窝mega再生力！！"),
        [4] = new DonorInfo("噬魂幽fa", 99.61m, ""),
        [5] = new DonorInfo("东山之上", 200.00m, ""),
    };
}
