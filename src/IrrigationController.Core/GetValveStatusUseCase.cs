﻿namespace IrrigationController.Core
{
    public class GetValveStatusUseCase(ValveController valveController, ValveConfig valveConfig)
    {
        private readonly ValveController valveController = valveController;
        private readonly IReadOnlyList<string> valveNames = valveConfig.Valves.Select(x => x.Name).ToList();

        public (IReadOnlyList<string> ValveNames, int? OpenValveId) Execute()
        {
            return (this.valveNames, this.valveController.OpenValveId);
        }
    }
}
