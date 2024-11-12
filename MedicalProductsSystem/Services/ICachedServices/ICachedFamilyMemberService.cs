using MedicinalProductsSystem.Models;
using System.Collections.Generic;

namespace MedicalProductsSystem.Services.ICachedServices
{
    public interface ICachedFamilyMemberService
    {
        public IEnumerable<FamilyMember> GetFamilyMembers(int rowsNumber = 20);
        public void AddFamilyMembers(string cacheKey, int rowsNumber = 20);
        public IEnumerable<FamilyMember> GetFamilyMembers(string cacheKey, int rowsNumber = 20);
    }
}
