using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartApiary.Application.Interfaces;

namespace CloudProjekat;

public class SprayingCompletionTimer
{
    private readonly ISprayingCompletionService _completion;
    private readonly ILogger<SprayingCompletionTimer> _logger;

    public SprayingCompletionTimer(ISprayingCompletionService completion, ILogger<SprayingCompletionTimer> logger)
    {
        _completion = completion;
        _logger = logger;
    }

    [Function("SprayingCompletionTimer")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
    {
        var count = await _completion.CompleteExpiredAnnouncementsAsync();
        if (count > 0)
            _logger.LogInformation("Azure Function: evidentirano {Count} završenih tretmana.", count);
    }
}
