using TowerFight.BusinessLogic.Data;
using TowerFight.BusinessLogic.Data.Models;
using TowerFight.BusinessLogic.Data.RedisCache;
using TowerFight.BusinessLogic.Mappers;
using TowerFight.BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace TowerFight.BusinessLogic.Services
{
    public interface ILeadersService
    {
        Task<IEnumerable<Leader>> GetLeadersAsync(CancellationToken cancellationToken);
    }

    public class LeadersService : ILeadersService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IRedisCache _redisCache;

        public LeadersService(IDbContextFactory<AppDbContext> dbContextFactory, IRedisCache redisCache)
        {
            _dbContextFactory = dbContextFactory;
            _redisCache = redisCache;
        }

        public async Task<IEnumerable<Leader>> GetLeadersAsync(CancellationToken cancellationToken)
        {
            const string cacheSet = nameof(Leader);
            const string key = nameof(Leader);
            var LeadersResult = await _redisCache.GetAsync<IEnumerable<Leader>>(cacheSet, key);
            if (LeadersResult is not null)
            {
                return LeadersResult;
            }

            List<LeaderDao> Leaders = null!;
            
            await Task.WhenAll(
                Task.Run(async () =>
                    {
                        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                        Leaders = await context.Leaders.AsNoTracking().ToListAsync(cancellationToken);
                    }, cancellationToken)
                );

            
            LeadersResult = LeaderMapper.Map(Leaders);

            await _redisCache.AddAsync(cacheSet, key, LeadersResult);

            return LeadersResult;
        }

    }
}
