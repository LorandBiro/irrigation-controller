using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers
{
    public class ProgramController
    {
        private readonly ZoneController zoneController;
        private readonly IIrrigationLog log;

        private readonly Timer timer;
        private readonly List<ProgramStep> previousSteps;
        private readonly List<ProgramStep> nextSteps;

        public ProgramController(ZoneController zoneController, IIrrigationLog log)
        {
            this.zoneController = zoneController;
            this.log = log;

            this.timer = new(this.TimerCallback);
            this.previousSteps = [];
            this.nextSteps = [];
        }

        public ProgramStep? CurrentStep { get; private set; }

        public DateTime? CurrentStepEndsAt { get; private set; }

        public IReadOnlyList<ProgramStep> NextSteps => this.nextSteps;

        public event EventHandler? CurrentStepChanged;

        public void Run(IReadOnlyList<ProgramStep> steps, IrrigationStartReason reason)
        {
            if (steps.Count == 0)
            {
                throw new ArgumentException("Program must have at least one step");
            }

            lock (this.nextSteps)
            {
                if (this.CurrentStep is not null && this.CurrentStepEndsAt is not null)
                {
                    this.log.Write(new IrrigationStopped(DateTime.UtcNow, [.. this.previousSteps, GetAbortedStep()], ToStopReason(reason)));

                    this.previousSteps.Clear();
                    this.nextSteps.Clear();
                }

                this.log.Write(new IrrigationStarted(DateTime.UtcNow, steps, reason));

                this.nextSteps.AddRange(steps.Skip(1));
                this.CurrentStep = steps[0];
                this.CurrentStepEndsAt = DateTime.UtcNow + steps[0].Duration;
                this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);

                this.zoneController.Open(steps[0].ZoneId);
                this.timer.Change(steps[0].Duration, TimeSpan.Zero);
            }
        }

        public void Skip(IrrigationSkipReason reason)
        {
            lock (this.nextSteps)
            {
                if (this.CurrentStep == null)
                {
                    return;
                }

                if (this.nextSteps.Count == 0)
                {
                    this.Stop(ToStopReason(reason));
                    return;
                }

                ProgramStep abortedStep = GetAbortedStep();
                this.log.Write(new IrrigationSkipped(DateTime.UtcNow, abortedStep, reason));

                ProgramStep nextStep = this.nextSteps[0];
                this.previousSteps.Add(abortedStep);
                this.nextSteps.RemoveAt(0);
                this.CurrentStep = nextStep;
                this.CurrentStepEndsAt = DateTime.UtcNow + nextStep.Duration;
                this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);

                this.zoneController.Open(nextStep.ZoneId);
                this.timer.Change(nextStep.Duration, TimeSpan.Zero);

            }
        }

        public void Stop(IrrigationStopReason reason)
        {
            lock (this.nextSteps)
            {
                if (this.CurrentStep is null || this.CurrentStepEndsAt is null)
                {
                    return;
                }

                this.log.Write(new IrrigationStopped(DateTime.UtcNow, [.. this.previousSteps, GetAbortedStep()], reason));

                this.previousSteps.Clear();
                this.nextSteps.Clear();
                this.CurrentStep = null;
                this.CurrentStepEndsAt = null;
                this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);

                this.zoneController.Close();
                this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }

        private void TimerCallback(object? state)
        {
            lock (this.nextSteps)
            {
                ProgramStep currentStep = this.CurrentStep!;
                if (this.nextSteps.Count == 0)
                {
                    this.log.Write(new IrrigationStopped(DateTime.UtcNow, [.. this.previousSteps, currentStep], IrrigationStopReason.Completed));

                    this.previousSteps.Clear();
                    this.nextSteps.Clear();
                    this.CurrentStep = null;
                    this.CurrentStepEndsAt = null;

                    this.zoneController.Close();
                    this.timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                }
                else
                {
                    ProgramStep nextStep = this.nextSteps[0];
                    this.previousSteps.Add(currentStep);
                    this.nextSteps.RemoveAt(0);
                    this.CurrentStep = nextStep;
                    this.CurrentStepEndsAt = DateTime.UtcNow + nextStep.Duration;

                    this.zoneController.Open(nextStep.ZoneId);
                    this.timer.Change(nextStep.Duration, Timeout.InfiniteTimeSpan);
                }

                this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ProgramStep GetAbortedStep()
        {
            return new ProgramStep(this.CurrentStep!.ZoneId, this.CurrentStep.Duration - (this.CurrentStepEndsAt!.Value - DateTime.UtcNow));
        }

        private static IrrigationStopReason ToStopReason(IrrigationStartReason reason) => reason switch
        {
            IrrigationStartReason.Manual => IrrigationStopReason.Manual,
            IrrigationStartReason.Scheduled or IrrigationStartReason.LowSoilMoisture => IrrigationStopReason.Schedule,
            _ => throw new ArgumentException("Invalid start reason")
        };

        private static IrrigationStopReason ToStopReason(IrrigationSkipReason reason) => reason switch
        {
            IrrigationSkipReason.Manual => IrrigationStopReason.Manual,
            IrrigationSkipReason.ShortCircuit => IrrigationStopReason.ShortCircuit,
            _ => throw new ArgumentException("Invalid skip reason")
        };
    }
}
