using MedicinalProductsSystem.Models;
using System.Collections.Generic;

namespace MedicalProductsSystem.Services.ICachedServices
{
    public interface ICachedDiseaseService
    {
        public IEnumerable<Disease> GetDiseases(int rowsNumber = 20);
        public void AddDiseases(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Disease> GetDiseases(string cacheKey, int rowsNumber = 20);
    }
}
