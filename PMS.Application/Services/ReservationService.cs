using Microsoft.Extensions.Caching.Memory;
using PMS.Application.Interfaces;

namespace PMS.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(15);

    public ReservationService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<(bool Success, string? ReservationId, DateTime? ExpiresAt, string? Error, string? ErrorCode)> ReservePostAsync(string staffId, long postId, long? departmentId, long? divisionId, long? branchId, string idempotencyKey)
    {
        var key = $"rsv:{staffId}:{postId}";
        if (_cache.TryGetValue(key, out string? existing))
        {
            var parts = existing.Split('|');
            return Task.FromResult((true, (string?)parts[0], (DateTime?)DateTime.Parse(parts[1]), (string?)null, (string?)null));
        }

        var reservationId = $"RSV_{Guid.NewGuid():N}";
        var expires = DateTime.UtcNow.Add(_ttl);
        _cache.Set(key, $"{reservationId}|{expires:o}", expires);
        _cache.Set($"rsvid:{reservationId}", $"{staffId}|{postId}|{expires:o}", expires);
        return Task.FromResult((true, (string?)reservationId, (DateTime?)expires, (string?)null, (string?)null));
    }

    public Task<(bool Success, string? Error, string? ErrorCode)> ValidateReservationAsync(string reservationId, string staffId, long postId)
    {
        if (!_cache.TryGetValue($"rsvid:{reservationId}", out string? stored))
            return Task.FromResult((false, "Reservation not found or expired", "RESERVATION_NOT_FOUND"));

        var parts = stored.Split('|');
        if (parts.Length < 3) return Task.FromResult((false, "Invalid reservation", "RESERVATION_INVALID"));
        var rsStaff = parts[0];
        var rsPost = long.Parse(parts[1]);
        var expires = DateTime.Parse(parts[2]);
        if (DateTime.UtcNow > expires) return Task.FromResult((false, "Reservation expired", "RESERVATION_EXPIRED"));
        if (!string.Equals(rsStaff, staffId, StringComparison.OrdinalIgnoreCase) || rsPost != postId)
            return Task.FromResult((false, "Reservation details mismatch", "RESERVATION_MISMATCH"));
        return Task.FromResult((true, (string?)null, (string?)null));
    }

    public Task InvalidateReservationAsync(string reservationId)
    {
        if (_cache.TryGetValue($"rsvid:{reservationId}", out string stored))
        {
            var parts = stored.Split('|');
            if (parts.Length >= 2)
            {
                var staffId = parts[0];
                var postId = long.Parse(parts[1]);
                _cache.Remove($"rsv:{staffId}:{postId}");
            }
            _cache.Remove($"rsvid:{reservationId}");
        }
        return Task.CompletedTask;
    }
}


