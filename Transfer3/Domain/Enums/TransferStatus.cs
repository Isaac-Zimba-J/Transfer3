namespace Transfer3.Domain.Enums;

public enum TransferStatus
{
    Pending,      // Transfer is created but not started
    InProgress,   // Transfer is currently happening
    Completed,    // Transfer finished successfully
    Failed,       // Transfer failed
    Cancelled     // Transfer was cancelled by the user
}
