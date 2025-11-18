using System;
using System.Collections.Generic;

namespace Hospital_Management_System.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int StaffId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Status { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}