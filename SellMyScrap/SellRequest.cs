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
    public SellType type;
    public int amountFound;
    public int requestedAmount;
    public ConfirmationType confirmationType;

    public SellRequest(SellType type, int amountFound, int requestedAmount, ConfirmationType confirmationType)
    {
        this.type = type;
        this.amountFound = amountFound;
        this.requestedAmount = requestedAmount;
        this.confirmationType = confirmationType;
    }
}
