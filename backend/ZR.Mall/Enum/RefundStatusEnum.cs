namespace ZR.Mall.Enum
{
    public enum RefundStatusEnum
    {
        None = 0,         // No refund
        Refunding = 1,    // Refund processing (user initiated refund/return in progress)
        Refunded = 2,     // Refund completed (full or partial refund)
        Returning = 3,    // Returning goods (user has shipped)
        Returned = 4      // Return completed (merchant confirmed receipt and refunded)
    }
}

