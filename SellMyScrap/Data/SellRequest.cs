namespace com.github.zehsteam.SellMyScrap.Data;

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

public enum ConfirmationStatus
{
    None,
    AwaitingConfirmation,
    Confirmed
}

public class SellRequest
{
    public SellType SellType;
    public int ScrapValue;
    public int RealScrapValue => ScrapHelper.GetRealValue(ScrapValue);
    public int RequestedScrapValue;
    public ConfirmationStatus ConfirmationStatus;
    public int ScrapEaterIndex;

    public SellRequest(SellType sellType, int scrapValue, int requestedScrapValue, ConfirmationStatus confirmationStatus, int scrapEaterIndex = -2)
    {
        SellType = sellType;
        ScrapValue = scrapValue;
        RequestedScrapValue = requestedScrapValue;
        ConfirmationStatus = confirmationStatus;
        ScrapEaterIndex = scrapEaterIndex;
    }
}
