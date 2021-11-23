using InvenageAPI.Models;
using InvenageAPI.Services.Enum;

namespace InvenageAPI.Services.Synchronizer
{
    public interface ISynchronizer
    {
        public SynchronizerType GetSynchronizerType();
        public bool Process();
        public void Initiate();
        public void Stop();
    }
}
