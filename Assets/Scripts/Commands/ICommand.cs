
namespace GameDevTV.RTS.Commands
{
    public interface ICommand
    {
        bool CanHandle(CommandContext context);
        void Handle(CommandContext context);

    }
}
