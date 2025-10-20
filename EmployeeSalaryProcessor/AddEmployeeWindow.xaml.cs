using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        // Метод для проверки всех вводимых данных
        private void ValidateInputs(object sender, TextChangedEventArgs e)
        {
            bool isValid = true;
            decimal totalSalary = 0;

            // Проверка имени
            string name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                SetTextBoxError(txtName, "Имя обязательно для заполнения");
                isValid = false;
            }
            else if (!IsValidName(name))
            {
                SetTextBoxError(txtName, "Имя может содержать только буквы");
                isValid = false;
            }
            else
            {
                ClearTextBoxError(txtName);
            }

            // Проверка фамилии
            string surname = txtSurname.Text.Trim();
            if (string.IsNullOrWhiteSpace(surname))
            {
                SetTextBoxError(txtSurname, "Фамилия обязательна для заполнения");
                isValid = false;
            }
            else if (!IsValidName(surname))
            {
                SetTextBoxError(txtSurname, "Фамилия может содержать только буквы");
                isValid = false;
            }
            else
            {
                ClearTextBoxError(txtSurname);
            }

            // Проверка зарплат по месяцам
            string[] months = { "January", "February", "March" };
            TextBox[] monthTextBoxes = { txtJanuary, txtFebruary, txtMarch };
            TextBlock[] monthErrorTexts = { tbJanuaryError, tbFebruaryError, tbMarchError };

            bool hasAtLeastOneSalary = false;

            for (int i = 0; i < months.Length; i++)
            {
                string salaryText = monthTextBoxes[i].Text.Trim();

                if (!string.IsNullOrWhiteSpace(salaryText))
                {
                    hasAtLeastOneSalary = true;

                    if (ValidateSalary(salaryText, out decimal salaryAmount, out string errorMessage))
                    {
                        monthErrorTexts[i].Text = "";
                        monthErrorTexts[i].Visibility = Visibility.Collapsed;
                        totalSalary += salaryAmount;
                    }
                    else
                    {
                        monthErrorTexts[i].Text = errorMessage;
                        monthErrorTexts[i].Visibility = Visibility.Visible;
                        isValid = false;
                    }
                }
                else
                {
                    monthErrorTexts[i].Text = "";
                    monthErrorTexts[i].Visibility = Visibility.Collapsed;
                }
            }

            // Проверка, что заполнена хотя бы одна зарплата
            if (!hasAtLeastOneSalary)
            {
                tbGeneralError.Text = "Необходимо указать зарплату хотя бы за один месяц";
                tbGeneralError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                tbGeneralError.Visibility = Visibility.Collapsed;
            }

            // Обновление информации об общей сумме
            tbTotalInfo.Text = $"Общая сумма: {totalSalary:F2} руб.";

            // Активация кнопки только если все данные валидны
            btnAdd.IsEnabled = isValid && hasAtLeastOneSalary;
        }

        // Проверка валидности имени/фамилии
        private bool IsValidName(string name)
        {
            // Разрешаем только буквы (включая русские и английские) и дефисы
            return Regex.IsMatch(name, @"^[\p{L}\-']+(\s+[\p{L}\-']+)*$");
        }

        // Проверка валидности зарплаты
        private bool ValidateSalary(string salaryText, out decimal amount, out string errorMessage)
        {
            amount = 0;
            errorMessage = "";

            // Проверка на отрицательные числа
            if (salaryText.StartsWith("-"))
            {
                errorMessage = "Зарплата не может быть отрицательной";
                return false;
            }

            // Нормализация числа (замена запятой на точку)
            string normalizedSalary = salaryText.Replace(",", ".");

            // Проверка формата числа
            if (!Regex.IsMatch(normalizedSalary, @"^\d*\.?\d+$"))
            {
                errorMessage = "Неверный формат числа";
                return false;
            }

            // Парсинг числа
            if (decimal.TryParse(normalizedSalary, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
            {
                // Проверка на разумный диапазон
                if (amount <= 0)
                {
                    errorMessage = "Зарплата должна быть больше 0";
                    return false;
                }

                if (amount > 1000000) // Максимум 1 миллион
                {
                    errorMessage = "Слишком большая сумма";
                    return false;
                }

                // Проверка на не более 2 знаков после запятой
                if (normalizedSalary.Contains('.'))
                {
                    string decimalPart = normalizedSalary.Split('.')[1];
                    if (decimalPart.Length > 2)
                    {
                        errorMessage = "Не более 2 знаков после запятой";
                        return false;
                    }
                }

                return true;
            }

            errorMessage = "Неверный числовой формат";
            return false;
        }

        // Установка визуального оформления ошибки для TextBox
        private void SetTextBoxError(TextBox textBox, string errorMessage)
        {
            textBox.BorderBrush = Brushes.Red;
            textBox.ToolTip = errorMessage;
        }

        // Очистка визуального оформления ошибки для TextBox
        private void ClearTextBoxError(TextBox textBox)
        {
            textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179)); // Стандартный цвет
            textBox.ToolTip = null;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Финальная проверка перед добавлением
            if (!btnAdd.IsEnabled)
            {
                MessageBox.Show("Исправьте ошибки в форме перед добавлением", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                EmployeeData["name"] = txtName.Text.Trim();
                EmployeeData["surname"] = txtSurname.Text.Trim();

                // Добавляем только заполненные месяцы
                AddSalaryIfNotEmpty("january", txtJanuary.Text.Trim());
                AddSalaryIfNotEmpty("february", txtFebruary.Text.Trim());
                AddSalaryIfNotEmpty("march", txtMarch.Text.Trim());

                // Проверяем, что есть хотя бы одна зарплата
                if (EmployeeData.Count <= 2) // Только имя и фамилия
                {
                    MessageBox.Show("Необходимо указать зарплату хотя бы за один месяц", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSalaryIfNotEmpty(string month, string salary)
        {
            if (!string.IsNullOrWhiteSpace(salary))
            {
                // Нормализуем число для хранения
                string normalizedSalary = salary.Replace(",", ".");
                EmployeeData[month] = normalizedSalary;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

       
    }
}
