using System.Threading.Tasks;

namespace tribute
{
    public interface IStartable
    {
        Task StartAsync();
        Task StopAsync();
    }
}