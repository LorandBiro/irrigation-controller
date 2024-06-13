namespace IrrigationController.Core.Controllers
{
    public record Program(IReadOnlyList<ProgramStep> Steps);
    public record ProgramStep(int ValveId, TimeSpan Duration);
}
