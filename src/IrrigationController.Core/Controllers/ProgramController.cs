﻿using IrrigationController.Core.Domain;

namespace IrrigationController.Core.Controllers
{
    public class ProgramController
    {
        private readonly ValveController valveController;
        private readonly Timer timer;
        private readonly List<ProgramStep> nextSteps;

        public ProgramController(ValveController valveController)
        {
            this.valveController = valveController;
            this.timer = new(this.TimerCallback);
            this.nextSteps = new List<ProgramStep>();
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

                this.StepInternal();
            }
        }

        public void Step()
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

                this.StepInternal();
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
                if (this.nextSteps.Count == 0)
                {
                    this.StopInternal();
                }
                else
                {
                    this.StepInternal();
                }
            }
        }

        private void StepInternal()
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
