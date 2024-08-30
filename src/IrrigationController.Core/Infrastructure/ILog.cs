namespace IrrigationController.Core.Infrastructure;

public interface ILog<out T>
{
    void Info(string message);

    void Debug(string message);

    void Trace(string message);
}
