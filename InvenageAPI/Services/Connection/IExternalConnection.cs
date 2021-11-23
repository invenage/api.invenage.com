using InvenageAPI.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Connection
{
    public interface IExternalConnection
    {
        public Task<APIResponseModel<TResponse>> SendRequestAsync<TRequest, TResponse>(string target, string endPoint, HttpMethod method, TRequest body);
    }
}
