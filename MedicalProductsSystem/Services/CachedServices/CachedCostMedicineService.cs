using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalProductsSystem.Services.CachedServices
{
    public class CachedCostMedicineService(MedicinalProductsContext dbContext, IMemoryCache memoryCache) : ICachedCostMedicineService
    {
        private readonly MedicinalProductsContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;
        // Номер варианта в журнале
        private readonly int N = 3;
        // получение списка препоратов из базы
        public IEnumerable<CostMedicine> GetCostMedicines(int rowsNumber = 20)
        {
            var costMedicines = _dbContext.CostMedicines.Include(cm => cm.Medicines).Take(rowsNumber).ToList();
            return costMedicines;
        }

        // добавление списка препоратов в кэш
        public void AddCostMedicines(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<CostMedicine> costMedicines = _dbContext.CostMedicines.Take(rowsNumber).Include(cm => cm.Medicines).ToList();
            if (costMedicines != null)
            {
                _memoryCache.Set(cacheKey, costMedicines, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * N + 240)
                });

            }
        }
        // получение списка препоратов из кэша или из базы, если нет в кэше
        public IEnumerable<CostMedicine> GetCostMedicines(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<CostMedicine> costMedicines))
            {
                costMedicines = _dbContext.CostMedicines.Take(rowsNumber).Include(cm => cm.Medicines).ToList();
                if (costMedicines != null)
                {
                    _memoryCache.Set(cacheKey, costMedicines,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2 * N + 240)));
                }
            }
            return costMedicines;
        }

    }
}

