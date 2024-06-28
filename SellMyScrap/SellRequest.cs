namespace com.github.zehsteam.SellMyScrap;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public enum SellType
{
    None,
    SellAmount,
    SellQuota,
    SellAll,
    SellItem,
    SellList
}

public enum ConfirmationType
{
    None,
    AwaitingConfirmation,
    Confirmed
}

public class SellRequest
{
    public SellType SellType;
    public int Value;
    public int RealValue => ScrapHelper.GetRealValue(Value);
    public int RequestedValue;
    public ConfirmationType ConfirmationType;
    public int ScrapEaterIndex;

    public SellRequest(SellType sellType, int value, int requestedValue, ConfirmationType confirmationType, int scrapEaterIndex = -2)
    {
        SellType = sellType;
        Value = value;
        RequestedValue = requestedValue;
        ConfirmationType = confirmationType;
        ScrapEaterIndex = scrapEaterIndex;
    }
}
