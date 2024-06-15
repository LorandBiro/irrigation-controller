using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers
{
    public class ProgramController
    {
        private readonly ValveController valveController;
        private readonly Timer timer;
        private readonly List<ProgramStep> steps;

        public ProgramController(ValveController valveController)
        {
            this.valveController = valveController;
            this.timer = new(this.TimerCallback);
            this.steps = new List<ProgramStep>();
        }

        public ProgramStep? CurrentStep { get; private set; }

        public DateTime? CurrentStepEndsAt { get; private set; }

        public IReadOnlyList<ProgramStep> NextSteps => this.steps;

        public event EventHandler? CurrentStepChanged;

        public void Run(Program program)
        {
            if (program.Steps.Count == 0)
            {
                throw new ArgumentException("Program must have at least one step");
            }

            lock (this.steps)
            {
                this.steps.Clear();
                foreach (ProgramStep step in program.Steps)
                {
                    this.steps.Add(step);
                }

                this.Step();
            }
        }

        public void Stop()
        {
            lock (this.steps)
            {
                this.steps.Clear();
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.End();
            }
        }

        private void TimerCallback(object? state)
        {
            lock (this.steps)
            {
                if (this.steps.Count == 0)
                {
                    this.End();
                }
                else
                {
                    this.Step();
                }
            }
        }

        private void Step()
        {
            ProgramStep step = this.steps[0];
            this.steps.RemoveAt(0);

            this.valveController.Open(step.ValveId);
            this.timer.Change(step.Duration, TimeSpan.Zero);

            this.CurrentStep = step;
            this.CurrentStepEndsAt = DateTime.UtcNow + step.Duration;
            this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);
        }

        private void End()
        {
            this.valveController.Close();

            this.CurrentStep = null;
            this.CurrentStepEndsAt = null;
            this.CurrentStepChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
