using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module07DataAccess.Model;
using Module07DataAccess.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Module07DataAccess.ViewModel
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private readonly EmployeeService _employeeService;
        public ObservableCollection<Employee> EmployeeList { get; set; }
        public ObservableCollection<Employee> FilteredEmployeeList { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterEmployeeList();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                if (_selectedEmployee != null)
                {
                    NewEmployeeName = _selectedEmployee.Name;
                    NewEmployeeAddress = _selectedEmployee.Address;
                    NewEmployeeEmail = _selectedEmployee.Email;
                    NewEmployeeContactNo = _selectedEmployee.ContactNo;

                    IsEmployeeSelected = true;
                    IsEmployeeSelectedAdd = false;
                }
                else
                {
                    IsEmployeeSelected = false;
                    IsEmployeeSelectedAdd = true;

                }
                OnPropertyChanged();
            }
        }

        private bool _isEmployeeSelected;
        public bool IsEmployeeSelected
        {
            get => _isEmployeeSelected;
            set
            {
                _isEmployeeSelected = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmployeeSelectedAdd;
        public bool IsEmployeeSelectedAdd
        {
            get => _isEmployeeSelectedAdd;
            set
            {
                _isEmployeeSelectedAdd = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // New Employee Entry for Name, Address, Email, and Contact No.
        private string _newEmployeeName;
        public string NewEmployeeName
        {
            get => _newEmployeeName;
            set
            {
                _newEmployeeName = value;
                OnPropertyChanged();
            }
        }

        private string _newEmployeeAddress;
        public string NewEmployeeAddress
        {
            get => _newEmployeeAddress;
            set
            {
                _newEmployeeAddress = value;
                OnPropertyChanged();
            }
        }

        private string _newEmployeeEmail;
        public string NewEmployeeEmail
        {
            get => _newEmployeeEmail;
            set
            {
                _newEmployeeEmail = value;
                OnPropertyChanged();
            }
        }

        private string _newEmployeeContactNo;
        public string NewEmployeeContactNo
        {
            get => _newEmployeeContactNo;
            set
            {
                _newEmployeeContactNo = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand SelectedEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand UpdateEmployeeCommand { get; }

        //PersonalViewModel Constructor
        public EmployeeViewModel()
        {
            _employeeService = new EmployeeService();
            EmployeeList = new ObservableCollection<Employee>();

            //For Filtering
            FilteredEmployeeList = new ObservableCollection<Employee>();


            LoadDataCommand = new Command(async () => await LoadData());
            AddEmployeeCommand = new Command(async () => await AddEmployee());
            SelectedEmployeeCommand = new Command<Employee>(employee => SelectedEmployee = employee);
            DeleteEmployeeCommand = new Command(async () => await DeleteEmployee(), () => SelectedEmployee != null);
            UpdateEmployeeCommand = new Command(async () => await UpdateEmployee());
            LoadData();
        }

        public async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Loading employee data...";

            try
            {
                var employees = await _employeeService.GetAllEmployeeAsync();
                EmployeeList.Clear();
                foreach (var employee in employees)
                {
                    EmployeeList.Add(employee);
                }
                FilteredEmployeeList.Clear();
                FilterEmployeeList();

                StatusMessage = "Data loaded successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterEmployeeList()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredEmployeeList.Clear();
                foreach (var employee in EmployeeList)
                {
                    FilteredEmployeeList.Add(employee);
                }
            }
            else
            {
                var filtered = EmployeeList.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                                        || p.Address.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                                        || p.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                                        || p.ContactNo.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

                FilteredEmployeeList.Clear();
                foreach (var employee in filtered)
                {
                    FilteredEmployeeList.Add(employee);
                }
            }
        }

        private async Task UpdateEmployee()
        {
            if (IsBusy || SelectedEmployee == null)
            {
                StatusMessage = "Select an Employee to update.";
                return;
            }

            IsBusy = true;
            try
            {
                SelectedEmployee.Name = NewEmployeeName;
                SelectedEmployee.Email = NewEmployeeEmail;
                SelectedEmployee.Address = NewEmployeeAddress;
                SelectedEmployee.ContactNo = NewEmployeeContactNo;

                var success = await _employeeService.UpdateEmployeeAsync(SelectedEmployee);
                StatusMessage = success ? "Employeee updated successfully!" : "Failed to update person.";

                if (success)
                {
                    var indexCount = FilteredEmployeeList.IndexOf(SelectedEmployee);
                    if (indexCount >= 0)
                    {
                        FilteredEmployeeList[indexCount] = SelectedEmployee;
                    }
                    ClearField();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddEmployee()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(NewEmployeeName) || string.IsNullOrWhiteSpace(NewEmployeeAddress) || string.IsNullOrWhiteSpace(NewEmployeeEmail) || string.IsNullOrWhiteSpace(NewEmployeeContactNo))
            {
                StatusMessage = "Please fill in all the fields before adding";
                return;
            }

            IsBusy = true;
            StatusMessage = "Adding new Employee...";

            try
            {
                var newEmployee = new Employee
                {
                    Name = NewEmployeeName,
                    Address = NewEmployeeAddress,
                    Email = NewEmployeeEmail,
                    ContactNo = NewEmployeeContactNo,
                };

                var isSuccess = await _employeeService.InsertEmployeeAsync(newEmployee);
                if (isSuccess)
                {
                    NewEmployeeName = string.Empty;
                    NewEmployeeAddress = string.Empty;
                    NewEmployeeEmail = string.Empty;
                    NewEmployeeContactNo = string.Empty;

                    StatusMessage = "New Employee added Successfully.";
                }

                else
                {
                    StatusMessage = " Failed to add new Employee";
                }
            }
            catch (Exception e)
            {
                StatusMessage = $"Failed adding new Employee: {e.Message}";
            }

            finally
            {
                IsBusy = false;
                var employees = await _employeeService.GetAllEmployeeAsync();
                FilteredEmployeeList.Clear();
                foreach (var employee in employees)
                {
                    FilteredEmployeeList.Add(employee);
                }
            }
        }

        private async Task DeleteEmployee()
        {
            if (SelectedEmployee == null) return;
            var answer = await Application.Current.MainPage.DisplayAlert("Confirm Delete", $"Are you sure you want to delete {SelectedEmployee.Name}?", "Yes", "No");

            if (!answer) return;

            IsBusy = true;

            try
            {
                var success = await _employeeService.DeleteEmployeeAsync(SelectedEmployee.EmployeeID);
                StatusMessage = success ? "Employee Record Deleted Successfully!" : "Failed to Delete Employee Record";

                if (success)
                {
                    FilteredEmployeeList.Remove(SelectedEmployee);
                    SelectedEmployee = null;
                    ClearField();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error Deleting Employee Record: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                var employees = await _employeeService.GetAllEmployeeAsync();
                FilteredEmployeeList.Clear();
                foreach (var employee in employees)
                {
                    FilteredEmployeeList.Add(employee);
                }
            }
        }

        private void ClearField()
        {
            NewEmployeeName = string.Empty;
            NewEmployeeAddress = string.Empty;
            NewEmployeeEmail = string.Empty;
            NewEmployeeContactNo = string.Empty;
            IsEmployeeSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
