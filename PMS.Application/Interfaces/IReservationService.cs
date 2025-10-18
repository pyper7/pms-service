namespace PMS.Application.Interfaces;

public interface IReservationService
{
    Task<(bool Success, string? ReservationId, DateTime? ExpiresAt, string? Error, string? ErrorCode)> ReservePostAsync(string staffId, long postId, long? departmentId, long? divisionId, long? branchId, string idempotencyKey);
    Task<(bool Success, string? Error, string? ErrorCode)> ValidateReservationAsync(string reservationId, string staffId, long postId);
    Task InvalidateReservationAsync(string reservationId);
}


