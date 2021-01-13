using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Database.Entities;
using Database.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WDLT.Clients.PS3838;
using WDLT.Clients.PS3838.Enums;
using WDLT.Workers.Base;

namespace PS3838Worker
{
    public class DataWorker : BaseBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PS3838Client _ps3838Client;
        private bool _initLoad;

        public DataWorker(IServiceProvider serviceProvider, PS3838Client ps3838Client)
        {
            Timeout = TimeSpan.FromMinutes(1);
            _serviceProvider = serviceProvider;
            _ps3838Client = ps3838Client;
            _initLoad = true;
        }

        protected override async Task DoWorkAsync(CancellationToken ct)
        {
            await using var db = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDatabase>();

            if (!await db.Sports.AnyAsync(ct))
                await UpdateSports();

            await UpdateFixtures(EPS3838Sport.Soccer);
            await UpdateOdds(EPS3838Sport.Soccer);

            db.Events.RemoveRange(db.Events.Where(w => w.StartAt < DateTimeOffset.Now));
            await db.SaveChangesAsync(ct);

            _initLoad = false;
        }

        private async Task UpdateSports()
        {
            var sports = await _ps3838Client.SportsV3Async();
            await using var db = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDatabase>();

            foreach (var sport in sports.Sports)
            {
                if (!await db.Sports.AnyAsync(a => a.Id == (int)sport.Id))
                {
                    await db.Sports.AddAsync(new DBSport { Id = (int)sport.Id, Name = sport.Name });
                }
            }

            await db.SaveChangesAsync();
        }

        private async Task UpdateFixtures(EPS3838Sport sport)
        {
            var sinceKey = $"Fixtures_{(int)sport}";

            await using var db = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDatabase>();

            var since = await db.Sinces.FirstOrDefaultAsync(f => f.For == sinceKey);
            if (since == null)
            {
                since = new DBSince { For = sinceKey };
                await db.Sinces.AddAsync(since);
            }

            var line = await _ps3838Client.FixturesV3Async(sport, since: since.Value);
            since.Value = line.Last;

            foreach (var league in line.League)
            {
                var existLeague = await db.Leagues.FirstOrDefaultAsync(f => f.Id == league.Id);
                if (existLeague == null)
                {
                    existLeague = new DBLeague { Id = league.Id, Name = league.Name, SportId = (int)sport };
                    await db.Leagues.AddAsync(existLeague);
                }

                foreach (var @event in league.Events.Where(w => w.ParentId == null && w.ResultingUnit == EPS3838Unit.Regular))
                {
                    var existEvent = await db.Events.FirstOrDefaultAsync(f => f.Id == @event.Id);

                    if (existEvent == null)
                    {
                        var newEvent = new DBEvent
                        {
                            Id = @event.Id,
                            Home = @event.Home,
                            Away = @event.Away,
                            IsLive = @event.LiveStatus == EPS3838LiveStatus.Live,
                            IsOpen = @event.Status != EPS3838EventStatus.Unavailable,
                            StartAt = @event.Starts,
                            LeagueId = existLeague.Id
                        };

                        await db.Events.AddAsync(newEvent);
                    }
                    else
                    {
                        existEvent.IsLive = @event.LiveStatus == EPS3838LiveStatus.Live;
                        existEvent.IsOpen = @event.Status != EPS3838EventStatus.Unavailable;
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        private async Task UpdateOdds(EPS3838Sport sport)
        {
            var sinceKey = $"Odds_{(int)sport}";

            await using var db = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDatabase>();

            var since = await db.Sinces.FirstOrDefaultAsync(f => f.For == sinceKey);
            if (since == null)
            {
                since = new DBSince { For = sinceKey };
                await db.Sinces.AddAsync(since);
            }

            var line = await _ps3838Client.OddsV3Async(sport, EPS3838OddsFormat.Decimal, "USD", false, since: since.Value);
            since.Value = line.Last;

            foreach (var @event in line.Leagues.SelectMany(s => s.Events))
            {
                var mainPeriod = @event.Periods.FirstOrDefault(w => w.Number == 0);
                if(mainPeriod?.Moneyline == null) continue;

                var dbEvent = await db.Events.FirstOrDefaultAsync(f => f.Id == @event.Id);
                if(dbEvent == null) continue;

                var item = new DBLine
                {
                    EventId = dbEvent.Id,
                    T1 = mainPeriod.Moneyline.Home,
                    T2 = mainPeriod.Moneyline.Draw,
                    T3 = mainPeriod.Moneyline.Away,
                    WinMaxRiskStake = (int?)mainPeriod.MaxMoneyline ?? 0,
                    Line = EDBLine.Moneyline
                };

                if (!dbEvent.Lines.Any())
                {
                    item.Type = EDBLineType.Open;
                    await db.Notifies.AddAsync(new DBNotify {EventId = dbEvent.Id, IsExecuted = _initLoad});
                }
                else
                {
                    item.Type = EDBLineType.Move;
                }

                await db.Lines.AddAsync(item);
            }

            await db.SaveChangesAsync();
        }
    }
}