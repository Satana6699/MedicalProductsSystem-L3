using MedicinalProductsSystem.Models;
using System.Collections.Generic;

namespace MedicalProductsSystem.Services.ICachedServices
{
    public interface ICachedMedicineService
    {
        public IEnumerable<Medicine> GetMedicines(int rowsNumber = 20);
        public void AddMedicines(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Medicine> GetMedicines(string cacheKey, int rowsNumber = 20);
        public Medicine SearchObject(string name);
    }
}
