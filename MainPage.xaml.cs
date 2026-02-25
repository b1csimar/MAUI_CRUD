using Maui_Doga.Services;
using System.Globalization;

namespace Maui_Doga
{
    public partial class MainPage : ContentPage
    {
        private readonly StudentService _studentService;
        private readonly ReportService _reportService;
        private Student _selectedStudent;

        public MainPage(StudentService studentService, ReportService reportService)
        {
            InitializeComponent();
            _studentService = studentService;
            _reportService = reportService;
            LoadData();
        }

        private async void LoadData()
        {
            await _studentService.LoadFromCsvAsync();
            StudentsCollectionView.ItemsSource = _studentService.GetAllStudents();
        }

        private void OnStudentSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Student student) return;
            
            _selectedStudent = student;
            NameEntry.Text = student.Name;
            GenderPicker.SelectedItem = student.Gender;
            HeightEntry.Text = student.Height.ToString(CultureInfo.InvariantCulture);
            WeightEntry.Text = student.Weight.ToString(CultureInfo.InvariantCulture);
            ClassEntry.Text = student.ClassNumber;
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            var student = CreateStudentFromFields();
            _studentService.AddStudent(student);
            ClearFields();
            await DisplayAlert("Siker", "Tanuló hozzáadva!", "OK");
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            if (_selectedStudent == null)
            {
                await DisplayAlert("Hiba", "Válassz ki egy tanulót!", "OK");
                return;
            }

            if (!ValidateFields()) return;

            var student = CreateStudentFromFields();
            student.Id = _selectedStudent.Id;
            _studentService.UpdateStudent(student);
            ClearFields();
            _selectedStudent = null;
            await DisplayAlert("Siker", "Tanuló módosítva!", "OK");
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (_selectedStudent == null)
            {
                await DisplayAlert("Hiba", "Válassz ki egy tanulót!", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Megerősítés",
                $"Törlöd: {_selectedStudent.Name}?", "Igen", "Nem");

            if (!confirm) return;

            _studentService.DeleteStudent(_selectedStudent.Id);
            ClearFields();
            _selectedStudent = null;
            await DisplayAlert("Siker", "Tanuló törölve!", "OK");
        }

        private void OnShowClassAveragesClicked(object sender, EventArgs e)
        {
            var averages = _reportService.GetClassAverages();
            var report = "Osztályok Átlagai:\n\n";

            foreach (var (className, (avgWeight, avgHeight)) in averages.OrderBy(x => x.Key))
            {
                report += $"Osztály {className}:\n";
                report += $"  Magasság: {avgHeight:F2} cm\n";
                report += $"  Testsúly: {avgWeight:F2} kg\n\n";
            }

            ReportLabel.Text = report;
        }

        private void OnShowTallestGirlClicked(object sender, EventArgs e)
        {
            var student = _reportService.GetTallestGirl();
            ReportLabel.Text = student != null
                ? FormatStudentReport("Legmagasabb Lány", student)
                : "Nincs lány tanuló.";
        }

        private void OnShowHeaviestBoyClicked(object sender, EventArgs e)
        {
            var student = _reportService.GetHeaviestBoy();
            ReportLabel.Text = student != null
                ? FormatStudentReport("Legsúlyosabb Fiú", student)
                : "Nincs fiú tanuló.";
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                GenderPicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(HeightEntry.Text) ||
                string.IsNullOrWhiteSpace(WeightEntry.Text) ||
                string.IsNullOrWhiteSpace(ClassEntry.Text))
            {
                DisplayAlert("Hiba", "Minden mező kitöltése kötelező!", "OK");
                return false;
            }
            return true;
        }

        private Student CreateStudentFromFields() => new()
        {
            Name = NameEntry.Text,
            Gender = GenderPicker.SelectedItem.ToString(),
            Height = double.Parse(HeightEntry.Text, CultureInfo.InvariantCulture),
            Weight = double.Parse(WeightEntry.Text, CultureInfo.InvariantCulture),
            ClassNumber = ClassEntry.Text
        };

        private string FormatStudentReport(string title, Student student) =>
            $"{title}:\n\n" +
            $"Név: {student.Name}\n" +
            $"Magasság: {student.Height} cm\n" +
            $"Testsúly: {student.Weight} kg\n" +
            $"Osztály: {student.ClassNumber}";

        private void ClearFields()
        {
            NameEntry.Text = string.Empty;
            GenderPicker.SelectedItem = null;
            HeightEntry.Text = string.Empty;
            WeightEntry.Text = string.Empty;
            ClassEntry.Text = string.Empty;
            StudentsCollectionView.SelectedItem = null;
        }
    }
}
