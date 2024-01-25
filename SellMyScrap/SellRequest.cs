namespace SellMyScrap
{
    public enum SellType
    {
        None = -1,
        SellAmount = 0,
        SellQuota = 1,
        SellAll = 2,
    }

    public class SellRequest
    {
        public SellType type;
        public int amount;
        public int requestedAmount;
        public bool awaitingConfirmation;

        public SellRequest(SellType type, int amount, int requestedAmount, bool awaitingConfirmation)
        {
            this.type = type;
            this.amount = amount;
            this.requestedAmount = requestedAmount;
            this.awaitingConfirmation = awaitingConfirmation;
        }
    }
}
