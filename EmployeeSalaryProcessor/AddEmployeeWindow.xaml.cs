using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EmployeeSalaryProcessor
{
    /// <summary>
    /// Логика взаимодействия для AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        public Dictionary<string, string> EmployeeData { get; private set; }

        public AddEmployeeWindow()
        {
            InitializeComponent();
            EmployeeData = new Dictionary<string, string>();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                MessageBox.Show("Заполните имя и фамилию сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EmployeeData["name"] = txtName.Text;
            EmployeeData["surname"] = txtSurname.Text;

            if (!string.IsNullOrWhiteSpace(txtJanuary.Text))
                EmployeeData["january"] = txtJanuary.Text;

            if (!string.IsNullOrWhiteSpace(txtFebruary.Text))
                EmployeeData["february"] = txtFebruary.Text;

            if (!string.IsNullOrWhiteSpace(txtMarch.Text))
                EmployeeData["march"] = txtMarch.Text;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
