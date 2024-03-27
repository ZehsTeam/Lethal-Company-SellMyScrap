namespace com.github.zehsteam.SellMyScrap;

public enum SellType
{
    None,
    SellAmount,
    SellQuota,
    SellAll,
    SellItem,
}

public enum ConfirmationType
{
    None,
    AwaitingConfirmation,
    Confirmed,
}

public class SellRequest
{
    public SellType sellType;
    public int value;
    public int realValue => ScrapHelper.GetRealValue(value);
    public int requestedValue;
    public ConfirmationType confirmationType;
    public int scrapEaterIndex;

    public SellRequest(SellType sellType, int value, int requestedValue, ConfirmationType confirmationType, int scrapEaterIndex = -2)
    {
        this.sellType = sellType;
        this.value = value;
        this.requestedValue = requestedValue;
        this.confirmationType = confirmationType;
        this.scrapEaterIndex = scrapEaterIndex;
    }
}
