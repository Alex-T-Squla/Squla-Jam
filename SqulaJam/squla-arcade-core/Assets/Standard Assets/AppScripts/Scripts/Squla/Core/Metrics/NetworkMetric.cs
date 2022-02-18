namespace Squla.Core.Metrics
{
    public interface NetworkMetric
    {
        int requestId { get; }
        void Response(int httpStatus, int bytesCount);
        void Response(int httpStatus, int bytesCount, int serverMilliseconds);
        void Response(int httpStatus, int bytesCount, int nativeMilliseconds, int serverMilliseconds);
    }
}
