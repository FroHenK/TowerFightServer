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

    public class LeadersService(IDbContextFactory<AppDbContext> _dbContextFactory, IRedisCache _redisCache) : ILeadersService
    {
        const int MaxLeadersCount = 10;

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
                        Leaders = await context.Leaders
                            .AsNoTracking()
                            // need to use subquery as groupBy+selectMany can't be traversed to sql, but seems subquery is optimal
                            .Select(l => l.Difficulty)
                            .Distinct()
                            .SelectMany(difficulty => context.Leaders
                                .Where(l => l.Difficulty == difficulty)
                                .OrderByDescending(l => l.Score)
                                .Take(MaxLeadersCount))
                            //.GroupBy(l => l.Difficulty)
                            //.SelectMany(g => g.OrderByDescending(l => l.Score).Take(MaxLeadersCount))                            
                            //.OrderBy(l => l.Difficulty).ThenByDescending(l => l.Score)
                            .ToListAsync(cancellationToken);
                            
                            // alternative with rawsql
                            //var leaders = await context.Leaders
                            //.FromSqlRaw(@"
                            //    SELECT * FROM (
                            //        SELECT *, ROW_NUMBER() OVER (PARTITION BY Difficulty ORDER BY Score DESC) as Rank
                            //        FROM Leaders
                            //    ) AS t
                            //    WHERE Rank <= {0}", MaxLeadersCount)
                            //.ToListAsync(cancellationToken);
                    }, cancellationToken)
                );
            
            LeadersResult = LeaderMapper.Map(Leaders);

            await _redisCache.AddAsync(cacheSet, key, LeadersResult);

            return LeadersResult;
        }
    }
}
