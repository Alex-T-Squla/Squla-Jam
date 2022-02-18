using SimpleJson;

namespace Squla.Core.TDD_Impl
{
    public interface IMessageSender
    {
        void Send(JsonObject msg);
    }
}
