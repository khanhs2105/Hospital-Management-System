using System;
using System.Collections.Generic;

namespace Hospital_Management_System.Models;

public partial class MedicalRecord
{
    public MedicalRecord()
    {
        PrescriptionDetails = new List<PrescriptionDetail>();
    }

    public int RecordId { get; set; }

    public int AppointmentId { get; set; }

    public string? Diagnosis { get; set; }

    public string? DoctorNote { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; }
}