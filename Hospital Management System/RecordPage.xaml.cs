using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hospital_Management_System
{
    public partial class RecordPage : Page
    {
        private readonly HospitalManagementDbContext _context;
        private readonly Appointment _appointment;
        private MedicalRecord _record;

        private ObservableCollection<PrescriptionDetail> _tempPrescriptions;

        public RecordPage(Appointment appointment)
        {
            InitializeComponent();
            _context = new HospitalManagementDbContext();
            _appointment = appointment;
            _tempPrescriptions = new ObservableCollection<PrescriptionDetail>();
            DgPrescriptions.ItemsSource = _tempPrescriptions;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPatientInfo();
            LoadMedicines();
            LoadExistingRecord();
        }

        private void LoadPatientInfo()
        {
            if (_appointment.Patient != null && _appointment.Doctor != null)
            {
                TxtPatientInfo.Text = $"Patient: {_appointment.Patient.FullName}";
                TxtDoctorInfo.Text = $"Doctor: {_appointment.Doctor.FullName}";
                TxtAppointmentTime.Text = $"Appointment: {_appointment.AppointmentDate:dd/MM/yyyy HH:mm}";
            }
        }

        private void LoadMedicines()
        {
            CboMedicines.ItemsSource = _context.Medicines.Where(m => m.Stock > 0).ToList();
        }

        private void LoadExistingRecord()
        {
            _record = _context.MedicalRecords
                              .Include(r => r.PrescriptionDetails)
                              .ThenInclude(pd => pd.Medicine)
                              .FirstOrDefault(r => r.AppointmentId == _appointment.AppointmentId);

            if (_record != null)
            {
                TxtDiagnosis.Text = _record.Diagnosis;
                TxtDoctorNote.Text = _record.DoctorNote;

                _tempPrescriptions.Clear();
                foreach (var prescription in _record.PrescriptionDetails)
                {
                    _tempPrescriptions.Add(prescription);
                }

                if (_appointment.Status == "Completed" || _appointment.Status == "Paid")
                {
                    BtnSaveRecord.Content = "Saved (Completed)";
                    BtnSaveRecord.IsEnabled = false;
                    BtnAddMedicine.IsEnabled = false;
                }
            }
            else
            {
                _record = new MedicalRecord
                {
                    AppointmentId = _appointment.AppointmentId,
                    CreatedDate = System.DateTime.Now
                };
            }
        }

        private void BtnAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (CboMedicines.SelectedItem is Medicine selectedMedicine &&
                int.TryParse(TxtQuantity.Text, out int quantity))
            {
                if (quantity <= 0)
                {
                    MessageBox.Show("Quantity must be greater than 0.");
                    return;
                }

                if (quantity > selectedMedicine.Stock)
                {
                    MessageBox.Show($"Not enough stock. In stock: {selectedMedicine.Stock}");
                    return;
                }

                var prescriptionDetail = new PrescriptionDetail
                {
                    MedicineId = selectedMedicine.MedicineId,
                    Medicine = selectedMedicine,
                    Quantity = quantity,
                    Dosage = TxtDosage.Text,
                    SubTotal = quantity * (selectedMedicine.UnitPrice ?? 0)
                };

                _tempPrescriptions.Add(prescriptionDetail);

                CboMedicines.SelectedIndex = -1;
                TxtQuantity.Clear();
                TxtDosage.Clear();
            }
            else
            {
                MessageBox.Show("Please select a medicine and enter a valid quantity.");
            }
        }

        private void BtnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _record.Diagnosis = TxtDiagnosis.Text;
                _record.DoctorNote = TxtDoctorNote.Text;

                if (_record.RecordId == 0)
                {
                    _context.MedicalRecords.Add(_record);
                }
                else
                {
                    _context.MedicalRecords.Update(_record);

                    var oldPrescriptions = _context.PrescriptionDetails.Where(p => p.RecordId == _record.RecordId);
                    _context.PrescriptionDetails.RemoveRange(oldPrescriptions);
                }

                _context.SaveChanges();

                foreach (var prescription in _tempPrescriptions)
                {
                    prescription.RecordId = _record.RecordId;

                    var med = _context.Medicines.Find(prescription.MedicineId);
                    if (med != null) med.Stock -= prescription.Quantity;

                    _context.PrescriptionDetails.Add(prescription);
                }

                var appointmentToUpdate = _context.Appointments.Find(_appointment.AppointmentId);
                if (appointmentToUpdate != null)
                {
                    appointmentToUpdate.Status = "Completed";
                    _context.Appointments.Update(appointmentToUpdate);
                }

                _context.SaveChanges();

                MessageBox.Show("Record saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnSaveRecord.Content = "Saved (Completed)";
                BtnSaveRecord.IsEnabled = false;
                BtnAddMedicine.IsEnabled = false;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error saving record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}