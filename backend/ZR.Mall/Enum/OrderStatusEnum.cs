namespace ZR.Mall.Enum
{
    /// <summary>
    /// Order status: 0=Pending payment, 1=To be shipped, 2=Shipped, 3=Completed, 4=Cancelled, 5=Closed
    /// </summary>
    public enum OrderStatusEnum
    {
        /// <summary>
        /// Pending payment (order created but not paid)
        /// </summary>
        None = 0,
        /// <summary>
        /// To be shipped (already paid)
        /// </summary>
        TobeShipped = 1,
        /// <summary>
        /// Shipped (in transit)
        /// </summary>
        Shipped = 2,
        /// <summary>
        /// Completed/Received (user confirmed receipt or system auto-receipt)
        /// </summary>
        Completed = 3,
        /// <summary>
        /// Cancelled (user cancelled or unpaid timeout cancellation)
        /// </summary>
        Cancel = 4,
        /// <summary>
        /// Closed (order refunded/after-sales completed and permanently closed)
        /// </summary>
        Closed = 5
    }
}

