using System;
using System.Collections.Generic;

namespace Hospital_Management_System.Models;

public partial class PrescriptionDetail
{
    public int RecordId { get; set; }

    public int MedicineId { get; set; }

    public int Quantity { get; set; }

    public string? Dosage { get; set; }

    public decimal? SubTotal { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual MedicalRecord Record { get; set; } = null!;
}