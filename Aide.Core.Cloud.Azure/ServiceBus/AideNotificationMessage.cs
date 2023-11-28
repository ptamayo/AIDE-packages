namespace Aide.Core.Cloud.Azure.ServiceBus
{
    public class AideNotificationMessage
    {
        public string TransactionId { get; set; }
        public int ClaimId { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
    }
}
