using Microsoft.Extensions.DependencyInjection;
using TowerFight.BusinessLogic.Data.Redis;
using TowerFight.BusinessLogic.Data.RedisCache;
using Microsoft.Extensions.Caching.Memory;

namespace TowerFight.BusinessLogic.Data.RedisDI
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterRedisServices(this IServiceCollection services, bool useInMemoryCache = false)
        {
            if (useInMemoryCache)
            {
                services.AddSingleton<IMemoryCache, MemoryCache>();
                services.AddSingleton<IRedisCache, RedisCacheMock>();
            }
            else
            {
                services.AddSingleton<IRedisCache, RedisCache.RedisCache>();
                services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            }
        }
    }
}