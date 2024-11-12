using System;
using System.Collections.Generic;

namespace MedicinalProductsSystem.Models;

public partial class FamilyMember
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
