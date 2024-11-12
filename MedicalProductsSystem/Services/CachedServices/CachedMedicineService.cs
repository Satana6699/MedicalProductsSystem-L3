using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalProductsSystem.Services.CachedServices
{
    public class CachedMedicineService(MedicinalProductsContext dbContext, IMemoryCache memoryCache) : ICachedMedicineService
    {
        private readonly MedicinalProductsContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;
        // Номер варианта в журнале
        private readonly int N = 3;
        // получение списка препоратов из базы
        public IEnumerable<Medicine> GetMedicines(int rowsNumber = 20)
        {
            return _dbContext.Medicines.Take(rowsNumber).ToList();
        }

        // добавление списка препоратов в кэш
        public void AddMedicines(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Medicine> medicines = _dbContext.Medicines.Take(rowsNumber).ToList();
            if (medicines != null)
            {
                _memoryCache.Set(cacheKey, medicines, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * N + 240)
                });

            }
        }
        // получение списка препоратов из кэша или из базы, если нет в кэше
        public IEnumerable<Medicine> GetMedicines(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Medicine> medicines))
            {
                medicines = _dbContext.Medicines.Take(rowsNumber).ToList();
                if (medicines != null)
                {
                    _memoryCache.Set(cacheKey, medicines,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2 * N + 240)));
                }
            }
            return medicines;
        }

        public Medicine SearchObject(string name)
        {
            return _dbContext.Medicines.FirstOrDefault(e => e.Name == name);

        }

    }
}

