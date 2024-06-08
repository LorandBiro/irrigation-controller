namespace IrrigationController.Watering;

public class WateringProgramExecutor
{
    private readonly Queue<WateringProgramStep> program = new();

    public bool IsRunning { get; private set; }

    public int CurrentStation { get; private set; }

    public void Run(CancellationToken cancellationToken)
    {
        // Wait for program
        
    }

    public void Run(IEnumerable<WateringProgramStep> program)
    {
    }

    public void Stop()
    {
    }
}
