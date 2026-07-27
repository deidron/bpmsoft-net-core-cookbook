namespace BPMSoft.Configuration.EP
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEpHttpClientExample
    {
        string Execute(string endpoint);

        Task<string> ExecuteAsync(string endpoint);

        Task<string> ExecuteAsync(string endpoint, CancellationToken cancellationToken);

    }
}
