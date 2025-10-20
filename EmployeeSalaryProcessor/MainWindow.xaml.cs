using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace EmployeeSalaryProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Модель данных для отображения
        public class EmployeeDisplay
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string January { get; set; }
            public string February { get; set; }
            public string March { get; set; }
            public string TotalSalary { get; set; }
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sourceFile = ((ComboBoxItem)cmbSourceFile.SelectedItem).Content.ToString();

                // 1. XSLT преобразование
                string employeesXml = ApplyXsltTransformation(sourceFile);

                // 2. Добавление атрибута с суммой salary
                employeesXml = AddTotalSalaryAttribute(employeesXml);

                // 3. Сохранение Employees.xml
                File.WriteAllText("Employees.xml", employeesXml);

                // 4. Обновление исходного файла с суммами
                UpdateSourceFileWithTotals(sourceFile);

                // 5. Отображение данных
                DisplayEmployeesData(employeesXml);

                tbStatus.Text = "Обработка завершена успешно";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                tbStatus.Text = "Ошибка при обработке";
            }
        }

        private string ApplyXsltTransformation(string sourceFile)
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load("TransformToEmployees.xslt");

                using (StringWriter sw = new StringWriter())
                {
                    xslt.Transform(sourceFile, null, sw);
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка XSLT преобразования: {ex.Message}");
            }
        }

        private string AddTotalSalaryAttribute(string xmlContent)
        {
            XDocument doc = XDocument.Parse(xmlContent);

            foreach (XElement employee in doc.Descendants("Employee"))
            {
                decimal totalSalary = 0;

                foreach (XElement salary in employee.Descendants("salary"))
                {
                    string amountStr = salary.Attribute("amount")?.Value;
                    if (!string.IsNullOrEmpty(amountStr))
                    {
                        // Улучшенный парсинг чисел
                        decimal amount = ParseDecimal(amountStr);
                        totalSalary += amount;
                    }
                }
                employee.SetAttributeValue("totalSalary", totalSalary.ToString("F2"));
            }
            return doc.ToString();
        }

        //Метод для корректного парсинга десятичных чисел
        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            // Заменяем запятую на точку для корректного парсинга
            string normalizedValue = value.Replace(",", ".");

            // Удаляем лишние пробелы
            normalizedValue = normalizedValue.Trim();

            // Пытаемся распарсить число
            if (decimal.TryParse(normalizedValue,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal result))
            {
                return result;
            }

            // Если не удалось, пытаемся с текущим
            if (decimal.TryParse(normalizedValue, out result))
            {
                return result;
            }

            // Если все еще не удалось, возвращаем 0
            return 0;
        }

        private void UpdateSourceFileWithTotals(string sourceFile)
        {
            XDocument doc = XDocument.Load(sourceFile);
            if (sourceFile == "Data1.xml")
            {
                decimal totalAmount = 0;
                var items = doc.Descendants("item");
                foreach (var item in items)
                {
                    string amountStr = item.Attribute("amount")?.Value;
                    if (!string.IsNullOrEmpty(amountStr))
                    {
                        decimal amount = ParseDecimal(amountStr);
                        totalAmount += amount;
                    }
                }

                var payElement = doc.Element("Pay");
                if (payElement != null)
                {
                    payElement.SetAttributeValue("totalAmount", totalAmount.ToString("F2"));
                }
            }
            else if (sourceFile == "Data2.xml")
            {
                foreach (var monthElement in doc.Root.Elements())
                {
                    decimal monthTotal = 0;
                    var items = monthElement.Elements("item");

                    foreach (var item in items)
                    {
                        string amountStr = item.Attribute("amount")?.Value;
                        if (!string.IsNullOrEmpty(amountStr))
                        {
                            decimal amount = ParseDecimal(amountStr);
                            monthTotal += amount;
                        }
                    }
                    monthElement.SetAttributeValue("totalAmount", monthTotal.ToString("F2"));
                }
            }
            doc.Save(sourceFile);
        }

        private void DisplayEmployeesData(string employeesXml)
        {
            XDocument doc = XDocument.Parse(employeesXml);
            var employees = new List<EmployeeDisplay>();

            foreach (XElement employee in doc.Descendants("Employee"))
            {
                var display = new EmployeeDisplay
                {
                    Name = employee.Attribute("name")?.Value,
                    Surname = employee.Attribute("surname")?.Value,
                    TotalSalary = employee.Attribute("totalSalary")?.Value
                };

                // Заполняем данные по месяцам с форматированием
                foreach (XElement salary in employee.Descendants("salary"))
                {
                    string mount = salary.Attribute("mount")?.Value;
                    string amount = salary.Attribute("amount")?.Value;

                    // Форматируем число для красивого отображения
                    if (!string.IsNullOrEmpty(amount))
                    {
                        decimal parsedAmount = ParseDecimal(amount);
                        string formattedAmount = parsedAmount.ToString("F2");

                        switch (mount?.ToLower())
                        {
                            case "january":
                                display.January = formattedAmount;
                                break;
                            case "february":
                                display.February = formattedAmount;
                                break;
                            case "march":
                                display.March = formattedAmount;
                                break;
                        }
                    }
                }
                employees.Add(display);
            }
            dgEmployees.ItemsSource = employees;
        }

        // Дополнительный функционал - добавление сотрудника
        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEmployeeWindow addWindow = new AddEmployeeWindow();
                if (addWindow.ShowDialog() == true)
                {
                    string sourceFile = ((ComboBoxItem)cmbSourceFile.SelectedItem).Content.ToString();
                    AddEmployeeToFile(sourceFile, addWindow.EmployeeData);

                    // Перезапускаем обработку для пересчета
                    BtnProcess_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddEmployeeToFile(string sourceFile, Dictionary<string, string> employeeData)
        {
            XDocument doc = XDocument.Load(sourceFile);

            if (sourceFile == "Data1.xml")
            {
                foreach (var month in new[] { "january", "february", "march" })
                {
                    if (employeeData.ContainsKey(month))
                    {
                        XElement newItem = new XElement("item",
                            new XAttribute("name", employeeData["name"]),
                            new XAttribute("surname", employeeData["surname"]),
                            new XAttribute("amount", employeeData[month]),
                            new XAttribute("mount", month));

                        doc.Root.Add(newItem);
                    }
                }
            }
            else if (sourceFile == "Data2.xml")
            {
                foreach (var month in new[] { "january", "february", "march" })
                {
                    if (employeeData.ContainsKey(month))
                    {
                        XElement monthElement = doc.Root.Element(month);
                        if (monthElement != null)
                        {
                            XElement newItem = new XElement("item",
                                new XAttribute("name", employeeData["name"]),
                                new XAttribute("surname", employeeData["surname"]),
                                new XAttribute("amount", employeeData[month]),
                                new XAttribute("mount", month));

                            monthElement.Add(newItem);
                        }
                    }
                }
            }

            doc.Save(sourceFile);
        }
    }
}