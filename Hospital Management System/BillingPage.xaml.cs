using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hospital_Management_System
{
    public partial class BillingPage : Page
    {
        private readonly HospitalManagementDbContext _context;
        private Appointment _selectedAppointment;
        private decimal _totalAmount;

        private const int STAFF_ID = 2;

        public BillingPage()
        {
            InitializeComponent();
            _context = new HospitalManagementDbContext();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPendingAppointments();
            CboPaymentMethod.SelectedIndex = 0;
        }

        private void LoadPendingAppointments()
        {
            var pendingAppointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == "Completed")
                .ToList();

            DgPendingAppointments.ItemsSource = pendingAppointments;

            ClearBillDetails();
        }

        private void DgPendingAppointments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPendingAppointments.SelectedItem is Appointment selected)
            {
                _selectedAppointment = selected;
                LoadBillDetails();
                BtnProcessPayment.IsEnabled = true;
            }
            else
            {
                ClearBillDetails();
            }
        }

        private void LoadBillDetails()
        {
            if (_selectedAppointment == null) return;

            TxtBillPatientName.Text = $"Patient: {_selectedAppointment.Patient.FullName}";
            TxtBillAppointmentID.Text = $"Appointment ID: {_selectedAppointment.AppointmentId}";

            var services = _context.AppointmentServices
                .Include(s => s.Service)
                .Where(s => s.AppointmentId == _selectedAppointment.AppointmentId)
                .ToList();
            DgServices.ItemsSource = services;

            var record = _context.MedicalRecords
                .Include(r => r.PrescriptionDetails)
                .ThenInclude(pd => pd.Medicine)
                .FirstOrDefault(r => r.AppointmentId == _selectedAppointment.AppointmentId);

            if (record != null)
            {
                DgMedicines.ItemsSource = record.PrescriptionDetails.ToList();
            }
            else
            {
                DgMedicines.ItemsSource = null;
            }

            decimal servicesTotal = services.Sum(s => s.SubTotal ?? 0);
            decimal medicinesTotal = record?.PrescriptionDetails.Sum(p => p.SubTotal ?? 0) ?? 0;

            _totalAmount = servicesTotal + medicinesTotal;
            TxtTotalAmount.Text = $"TOTAL: {_totalAmount:N0}";
        }

        private void ClearBillDetails()
        {
            TxtBillPatientName.Text = "Patient: [Select from list]";
            TxtBillAppointmentID.Text = "Appointment ID: N/A";
            DgServices.ItemsSource = null;
            DgMedicines.ItemsSource = null;
            TxtTotalAmount.Text = "TOTAL: 0";
            _selectedAppointment = null;
            _totalAmount = 0;
            BtnProcessPayment.IsEnabled = false;
        }

        private void BtnProcessPayment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAppointment == null || CboPaymentMethod.SelectedItem == null)
            {
                MessageBox.Show("Please select an appointment and payment method.");
                return;
            }

            try
            {
                var newBill = new Bill
                {
                    AppointmentId = _selectedAppointment.AppointmentId,
                    PatientId = _selectedAppointment.PatientId,
                    StaffId = STAFF_ID,
                    TotalAmount = _totalAmount,
                    PaymentMethod = (CboPaymentMethod.SelectedItem as ComboBoxItem).Content.ToString(),
                    PaymentDate = System.DateTime.Now,
                    Status = "Paid"
                };

                _context.Bills.Add(newBill);

                var appointmentToUpdate = _context.Appointments.Find(_selectedAppointment.AppointmentId);
                if (appointmentToUpdate != null)
                {
                    appointmentToUpdate.Status = "Paid";
                    _context.Appointments.Update(appointmentToUpdate);
                }

                _context.SaveChanges();

                MessageBox.Show("Payment successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadPendingAppointments();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}