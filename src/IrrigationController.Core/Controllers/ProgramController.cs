namespace IrrigationController.Core.Controllers
{
    public class ProgramController
    {
        private readonly ValveController valveController;
        private readonly Timer timer;
        private readonly Queue<ProgramStep> steps;

        public ProgramController(ValveController valveController)
        {
            this.valveController = valveController;
            this.timer = new(this.TimerCallback);
            this.steps = new Queue<ProgramStep>();
        }

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
                    this.steps.Enqueue(step);
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
                this.valveController.Close();
            }
        }

        private void TimerCallback(object? state)
        {
            lock (this.steps)
            {
                if (this.steps.Count == 0)
                {
                    this.valveController.Close();
                }
                else
                {
                    this.Step();
                }
            }
        }

        private void Step()
        {
            ProgramStep step = this.steps.Dequeue();
            this.valveController.Open(step.ValveId);
            this.timer.Change(step.Duration, TimeSpan.Zero);
        }
    }
}
