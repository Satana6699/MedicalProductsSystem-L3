using MedicinalProductsSystem.Models;
using System.Collections.Generic;

namespace MedicalProductsSystem.Services.ICachedServices
{
    public interface ICachedPrescriptionService
    {
        public IEnumerable<Prescription> GetPrescriptions(int rowsNumber = 20);
        public void AddPrescriptions(string cacheKey, int rowsNumber = 20);
        public IEnumerable<Prescription> GetPrescriptions(string cacheKey, int rowsNumber = 20);
    }
}
