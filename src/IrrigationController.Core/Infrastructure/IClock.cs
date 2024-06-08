namespace IrrigationController.Core.Infrastructure;

public interface IClock
{
    DateTime Now { get; }

    Task WaitAsync(TimeSpan time, CancellationToken cancellationToken = default);
}
