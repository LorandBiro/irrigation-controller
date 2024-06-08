using IrrigationController.Domain;

namespace IrrigationController.Core;

public class TurnOnStationUseCase(ProgramRunner programRunner, ILogger logger)
{
    private readonly ProgramRunner programRunner = programRunner;
    private readonly ILogger logger = logger;

    public void Execute(int stationId)
    {
        if (this.programRunner.IsRunning)
        {
            if (this.programRunner.CurrentStation == stationId)
            {
                return;
            }
        }

        this.programRunner.Run([(stationId, TimeSpan.FromMinutes(5.0))]);
        this.logger.WriteLine($"User turned on station #{stationId}.");
    }
}
