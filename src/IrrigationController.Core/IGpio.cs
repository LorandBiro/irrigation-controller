namespace IrrigationController.Core
{
    public interface IGpio
    {
        void OpenOutput(int pin);

        void Write(int pin, bool value);
    }
}
