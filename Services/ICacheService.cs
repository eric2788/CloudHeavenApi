using CloudHeavenApi.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace CloudHeavenApi.Services
{
    public interface ICacheService<T>
    {
        void SetItem(string id, T item);
        bool TryGetItem(string id, out T item);
        bool RemoveItem(string id);
    }


    public static class RegisterExtension
    {
        public static IServiceCollection RegisterCache<T>(this IServiceCollection service)
        {
            return service.AddSingleton<ICacheService<T>, CacheManager<T>>();
        }
    }
}