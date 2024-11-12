using MedicinalProductsSystem.Models;
using System.Collections.Generic;

namespace MedicalProductsSystem.Services.ICachedServices
{
    public interface ICachedCostMedicineService
    {
        public IEnumerable<CostMedicine> GetCostMedicines(int rowsNumber = 20);
        public void AddCostMedicines(string cacheKey, int rowsNumber = 20);
        public IEnumerable<CostMedicine> GetCostMedicines(string cacheKey, int rowsNumber = 20);
    }
}
