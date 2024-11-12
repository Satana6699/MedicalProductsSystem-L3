using System;
using System.Collections.Generic;

namespace MedicinalProductsSystem.Models;

public partial class Disease
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Duration { get; set; }

    public string? Symptoms { get; set; }

    public string? Consequences { get; set; }

    public virtual ICollection<DiseasesAndSymptom> DiseasesAndSymptoms { get; set; } = new List<DiseasesAndSymptom>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
