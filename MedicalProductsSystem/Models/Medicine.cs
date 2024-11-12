using System;
using System.Collections.Generic;

namespace MedicinalProductsSystem.Models;

public partial class Medicine
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Indications { get; set; }

    public string? Contraindications { get; set; }

    public string? Manufacturer { get; set; }

    public string? Packaging { get; set; }

    public virtual ICollection<CostMedicine> CostMedicines { get; set; } = new List<CostMedicine>();

    public virtual ICollection<DiseasesAndSymptom> DiseasesAndSymptoms { get; set; } = new List<DiseasesAndSymptom>();
}
