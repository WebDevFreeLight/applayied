using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Customers.DataMine;
using ZR.Repository;
using ZR.Service.IService.Customers.DataMine;

namespace ZR.Service.Customers.DataMine
{
    [AppService(ServiceType = typeof(IDmPhoneRevealService), ServiceLifetime = LifeTime.Transient)]
    public class DmPhoneRevealService : BaseService<DmPhoneReveal>, IDmPhoneRevealService
    {
        private const int DAILY_REVEAL_LIMIT = 20;

        public async Task<int> GetTodayRevealCount(long userId)
        {
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            return await Context.Queryable<DmPhoneReveal>()
                .Where(r => r.UserId == userId && r.RevealedAt >= todayStart && r.RevealedAt < todayEnd)
                .CountAsync();
        }

        public async Task<List<long>> GetTodayRevealedContactIds(long userId)
        {
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            return await Context.Queryable<DmPhoneReveal>()
                .Where(r => r.UserId == userId && r.RevealedAt >= todayStart && r.RevealedAt < todayEnd)
                .Select(r => r.ContactId)
                .ToListAsync();
        }

        public async Task<string> RevealPhone(long userId, long contactId)
        {
            var todayCount = await GetTodayRevealCount(userId);
            if (todayCount >= DAILY_REVEAL_LIMIT)
            {
                return null; // limit reached
            }

            // Check if already revealed today (don't double-count)
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);
            var alreadyRevealed = await Context.Queryable<DmPhoneReveal>()
                .Where(r => r.UserId == userId && r.ContactId == contactId && r.RevealedAt >= todayStart && r.RevealedAt < todayEnd)
                .AnyAsync();

            if (!alreadyRevealed)
            {
                var reveal = new DmPhoneReveal
                {
                    RevealId = SnowFlakeSingle.Instance.NextId(),
                    UserId = userId,
                    ContactId = contactId,
                    RevealedAt = DateTime.Now
                };

                await Context.Insertable(reveal).ExecuteCommandAsync();
            }

            // Return the full phone from dm_contact
            var contact = await Context.Queryable<DmContact>()
                .Where(c => c.ContactId == contactId)
                .Select(c => new { c.OwnerPhone, c.SecondaryPhone })
                .FirstAsync();

            return contact?.OwnerPhone;
        }

        public async Task<int> GetRemainingReveals(long userId)
        {
            var todayCount = await GetTodayRevealCount(userId);
            return Math.Max(0, DAILY_REVEAL_LIMIT - todayCount);
        }
    }
}
