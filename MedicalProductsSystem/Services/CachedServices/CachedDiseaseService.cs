using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalProductsSystem.Services.CachedServices
{
    public class CachedDiseaseService(MedicinalProductsContext dbContext, IMemoryCache memoryCache) : ICachedDiseaseService
    {
        private readonly MedicinalProductsContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;
        // Номер варианта в журнале
        private readonly int N = 3;
        // получение списка препоратов из базы
        public IEnumerable<Disease> GetDiseases(int rowsNumber = 20)
        {
            return _dbContext.Diseases.Take(rowsNumber).ToList();
        }

        // добавление списка препоратов в кэш
        public void AddDiseases(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Disease> diseases = _dbContext.Diseases.Take(rowsNumber).ToList();
            if (diseases != null)
            {
                _memoryCache.Set(cacheKey, diseases, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * N + 240)
                });

            }
        }
        // получение списка препоратов из кэша или из базы, если нет в кэше
        public IEnumerable<Disease> GetDiseases(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Disease> diseases))
            {
                diseases = _dbContext.Diseases.Take(rowsNumber).ToList();
                if (diseases != null)
                {
                    _memoryCache.Set(cacheKey, diseases,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2 * N + 240)));
                }
            }
            return diseases;
        }

    }
}

