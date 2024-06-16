using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers
{
    public class ProgramController
    {
        private readonly ValveController valveController;
        private readonly ProgramStepCompletedEventHandler programStepCompletedEventHandler;
        private readonly Timer timer;
        private readonly List<ProgramStep> nextSteps;

        public ProgramController(ValveController valveController, ProgramStepCompletedEventHandler programStepCompletedEventHandler)
        {
            this.valveController = valveController;
            this.programStepCompletedEventHandler = programStepCompletedEventHandler;
            this.timer = new(this.TimerCallback);
            this.nextSteps = [];
        }

        public ProgramStep? CurrentStep { get; private set; }

        public DateTime? CurrentStepEndsAt { get; private set; }

        public IReadOnlyList<ProgramStep> NextSteps => this.nextSteps;

        public event EventHandler? CurrentStepChanged;

        public void Run(Program program)
        {
            if (program.Steps.Count == 0)
            {
                throw new ArgumentException("Program must have at least one step");
            }

            lock (this.nextSteps)
            {
                this.nextSteps.Clear();
                foreach (ProgramStep step in program.Steps)
                {
                    this.nextSteps.Add(step);
                }

                this.Step();
            }
        }

        public void Skip()
        {
            lock (this.nextSteps)
            {
                if (this.CurrentStep == null)
                {
                    return;
                }

                if (this.nextSteps.Count == 0)
                {
                    this.StopInternal();
                    return;
                }

                this.Step();
            }
        }

        public void Stop()
        {
            lock (this.nextSteps)
            {
                if (this.CurrentStep == null)
                {
                    return;
                }

                this.nextSteps.Clear();
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.StopInternal();
            }
        }

        private void TimerCallback(object? state)
        {
            lock (this.nextSteps)
            {
                if (this.CurrentStep is not null)
                {
                    this.programStepCompletedEventHandler.Handle(this.CurrentStep);
                }

                if (this.nextSteps.Count == 0)
                {
                    this.StopInternal();
                }
                else
                {
                    this.Step();
                }
            }
        }

        private void Step()
        {
            ProgramStep step = this.nextSteps[0];
            this.nextSteps.RemoveAt(0);

            this.valveController.Open(step.ValveId);
            this.timer.Change(step.Duration, TimeSpan.Zero);

            this.CurrentStep = step;
            this.CurrentStepEndsAt = DateTime.UtcNow + step.Duration;
            this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StopInternal()
        {
            this.valveController.Close();

            this.CurrentStep = null;
            this.CurrentStepEndsAt = null;
            this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
