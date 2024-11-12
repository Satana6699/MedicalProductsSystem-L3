using MedicinalProductsSystem.Models;
using System.Collections.Generic;

namespace MedicalProductsSystem.Services.ICachedServices
{
    public interface ICachedDiseasesAndSymptomService
    {
        public IEnumerable<DiseasesAndSymptom> GetDiseasesAndSymptoms(int rowsNumber = 20);
        public void AddDiseasesAndSymptoms(string cacheKey, int rowsNumber = 20);
        public IEnumerable<DiseasesAndSymptom> GetDiseasesAndSymptoms(string cacheKey, int rowsNumber = 20);
    }
}
