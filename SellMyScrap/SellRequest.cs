namespace com.github.zehsteam.SellMyScrap;

public enum SellType
{
    None,
    SellAmount,
    SellQuota,
    SellAll,
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
    public int valueFound;
    public int valueRequested;
    public ConfirmationType confirmationType;

    public SellRequest(SellType type, int valueFound, int valueRequested, ConfirmationType confirmationType)
    {
        this.sellType = type;
        this.valueFound = valueFound;
        this.valueRequested = valueRequested;
        this.confirmationType = confirmationType;
    }
}
