using MedicalProductsSystem.Services.ICachedServices;
using MedicinalProductsSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalProductsSystem.Services.CachedServices
{
    public class CachedPrescriptionService(MedicinalProductsContext dbContext, IMemoryCache memoryCache) : ICachedPrescriptionService
    {
        private readonly MedicinalProductsContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;
        // Номер варианта в журнале
        private readonly int N = 3;
        // получение списка препоратов из базы
        public IEnumerable<Prescription> GetPrescriptions(int rowsNumber = 20)
        {
            var prescriptions = _dbContext.Prescriptions.Take(rowsNumber).ToList();
            prescriptions = _dbContext.Prescriptions
                        .Include(d => d.Diseases)
                        .Include(fm => fm.FamilyMember)
                        .Take(rowsNumber)
                        .ToList();
            return prescriptions;
        }

        // добавление списка препоратов в кэш
        public void AddPrescriptions(string cacheKey, int rowsNumber = 20)
        {
            IEnumerable<Prescription> prescriptions = _dbContext.Prescriptions.Take(rowsNumber).ToList();
            prescriptions = _dbContext.Prescriptions
                        .Include(d => d.Diseases)
                        .Include(fm => fm.FamilyMember)
                        .Take(rowsNumber)
                        .ToList();
            if (prescriptions != null)
            {
                _memoryCache.Set(cacheKey, prescriptions, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * N + 240)
                });

            }
        }
        // получение списка препоратов из кэша или из базы, если нет в кэше
        public IEnumerable<Prescription> GetPrescriptions(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Prescription> prescriptions))
            {
                prescriptions = _dbContext.Prescriptions.Take(rowsNumber).ToList();
                prescriptions = _dbContext.Prescriptions
                        .Include(d => d.Diseases)
                        .Include(fm => fm.FamilyMember)
                        .Take(rowsNumber)
                        .ToList();
                if (prescriptions != null)
                {
                    _memoryCache.Set(cacheKey, prescriptions,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2 * N + 240)));
                }
            }
            return prescriptions;
        }
    }
}

