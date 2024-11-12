using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalProductsSystem.Services.CachedServices
{
    public class CachedFamilyMemberService(MedicinalProductsContext dbContext, IMemoryCache memoryCache) : ICachedFamilyMemberService
    {
        private readonly MedicinalProductsContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;
        // Номер варианта в журнале
        private readonly int N = 3;
        // получение списка препоратов из базы
        public IEnumerable<FamilyMember> GetFamilyMembers(int rowsNumber = 20)
        {
            return _dbContext.FamilyMembers.Take(rowsNumber).ToList();
        }

        // добавление списка препоратов в кэш
        public void AddFamilyMembers(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<FamilyMember> familyMembers = _dbContext.FamilyMembers.Take(rowsNumber).ToList();
            if (familyMembers != null)
            {
                _memoryCache.Set(cacheKey, familyMembers, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * N + 240)
                });

            }
        }
        // получение списка препоратов из кэша или из базы, если нет в кэше
        public IEnumerable<FamilyMember> GetFamilyMembers(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<FamilyMember> familyMembers))
            {
                familyMembers = _dbContext.FamilyMembers.Take(rowsNumber).ToList();
                if (familyMembers != null)
                {
                    _memoryCache.Set(cacheKey, familyMembers,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2 * N + 240)));
                }
            }
            return familyMembers;
        }

    }
}

