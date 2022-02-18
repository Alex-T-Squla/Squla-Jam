namespace Squla.Core.Logging
{
    public interface ILogMeta
    {
        string GetContext();
        void SetUserId(string user_id);
    }
}
