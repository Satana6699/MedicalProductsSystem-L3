using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalProductsSystem.Services.CachedServices
{
    public class CachedDiseasesAndSymptomService(MedicinalProductsContext dbContext, IMemoryCache memoryCache) : ICachedDiseasesAndSymptomService
    {
        private readonly MedicinalProductsContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;
        // Номер варианта в журнале
        private readonly int N = 3;
        // получение списка препоратов из базы
        public IEnumerable<DiseasesAndSymptom> GetDiseasesAndSymptoms(int rowsNumber = 20)
        {
            var diseasesAndSymptoms = _dbContext.DiseasesAndSymptoms.Take(rowsNumber).ToList();
            diseasesAndSymptoms = _dbContext.DiseasesAndSymptoms
                        .Include(d => d.Diseases)
                        .Include(m => m.Medicines)
                        .Take(rowsNumber)
                        .ToList();
            return diseasesAndSymptoms;
        }

        // добавление списка препоратов в кэш
        public void AddDiseasesAndSymptoms(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<DiseasesAndSymptom> diseasesAndSymptoms = _dbContext.DiseasesAndSymptoms.Take(rowsNumber).ToList();
            diseasesAndSymptoms = _dbContext.DiseasesAndSymptoms
                        .Include(d => d.Diseases)
                        .Include(m => m.Medicines)
                        .Take(rowsNumber)
                        .ToList();
            if (diseasesAndSymptoms != null)
            {
                _memoryCache.Set(cacheKey, diseasesAndSymptoms, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * N + 240)
                });

            }
        }
        // получение списка препоратов из кэша или из базы, если нет в кэше
        public IEnumerable<DiseasesAndSymptom> GetDiseasesAndSymptoms(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<DiseasesAndSymptom> diseasesAndSymptoms))
            {
                diseasesAndSymptoms = _dbContext.DiseasesAndSymptoms.Take(rowsNumber).ToList();
                diseasesAndSymptoms = _dbContext.DiseasesAndSymptoms
                        .Include(d => d.Diseases)
                        .Include(m => m.Medicines)
                        .Take(rowsNumber)
                        .ToList();
                if (diseasesAndSymptoms != null)
                {
                    _memoryCache.Set(cacheKey, diseasesAndSymptoms,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2 * N + 240)));
                }
            }
            return diseasesAndSymptoms;
        }

    }
}

