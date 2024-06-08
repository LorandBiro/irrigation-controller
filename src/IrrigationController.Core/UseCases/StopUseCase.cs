using IrrigationController.Domain;

namespace IrrigationController.Core;

public class StopUseCase(ProgramRunner programRunner, ILogger logger)
{
    private readonly ProgramRunner programRunner = programRunner;
    private readonly ILogger logger = logger;

    public void Execute()
    {
        if (!this.programRunner.IsRunning)
        {
            return;
        }

        this.programRunner.Stop();
        this.logger.WriteLine("User stopped irrigation.");
    }
}
