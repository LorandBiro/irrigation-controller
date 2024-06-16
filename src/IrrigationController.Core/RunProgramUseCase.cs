using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;
using System.Text;

namespace IrrigationController.Core
{
    public class RunProgramUseCase(ProgramController programController, IValveRepository valveRepository, IIrrigationLog log)
    {
        private readonly ProgramController programController = programController;
        private readonly IValveRepository valveRepository = valveRepository;
        private readonly IIrrigationLog log = log;

        public void Execute(Program program)
        {
            foreach (ProgramStep step in program.Steps)
            {
                Valve? valve = valveRepository.Get(step.ValveId);
                if (valve?.IsDefective == true)
                {
                    throw new InvalidOperationException($"Can't run program with defective valve #{step.ValveId}");
                }
            }

            this.programController.Run(program);

            StringBuilder sb = new();
            sb.Append("Program started manually: ");
            for (int i = 0; i < program.Steps.Count; i++)
            {
                ProgramStep step = program.Steps[i];
                sb.Append($"#{step.ValveId} for {step.Duration:mm\\:ss}");
                if (i < program.Steps.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            this.log.Write(sb.ToString());
        }
    }
}
