namespace SmartApiary.Application.Interfaces;

public interface ISprayingCompletionService
{
    Task<int> CompleteExpiredAnnouncementsAsync(CancellationToken cancellationToken = default);
}
