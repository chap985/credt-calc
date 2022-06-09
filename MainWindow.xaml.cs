using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using iTextSharp.text.pdf;
using OfficeOpenXml;

namespace Кредитный_калькулятор
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public class Data
        {
            public string DataPayment { get; set; }
            public int NumberPayment { get; set; }
            public string PaymentSum { get; set; }
            public string CreditAmount { get; set; }
            public string Precent { get; set; }
            public string MainDebt { get; set; }

        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Очистка данных
             */
            sum.Text = "";
            percent.Text = "14,5";
            typeOne.IsChecked = false;
            typeTwo.IsChecked = false;

            itog.Text = "";
            // monpercent.Text = "";
            overp.Text = "";
            everymon.Text = "";

            MyData.Items.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyData.Items.Clear();
            /*
             * Подсчёт всех данных
             * creditAmount | myPrecent: Сумма кредита | Процентная ставка
             * textType | dates: Вид платежа | Срок в месяцах
             * interestRateMonth: Формула { Процентная ставка / 100 / 12 }
             * isSum | overpayment: Получение суммы платежа | Переплата (начисленные проценты) { (a * b) - c }
             * mainDebt: Основной долг
             */
            try
            {
                var creditAmount = double.Parse(sum.Text);
                if (creditAmount == 0)
                {
                    MessageBox.Show("Сумма кредита не может быть равна 0!");
                    return;
                }
                
                var myPercent = double.Parse(percent.Text.Replace(".", ","));
                var dates = int.Parse(months.Text);
                var interestRateMonth = myPercent / 100 / 12;
                var interestСharges = creditAmount * interestRateMonth; // Получаем процентную часть
                double isSum;
                double overpayment;
                var today = DateTime.Now; // Получение сегодняшней даты
                int addDays = 0; // + кол-во дней для нужной даты платежа
                double paymentSum;
                double mainDebt;

                if (typeOne.IsChecked != null && (bool)typeOne.IsChecked)
                {
                    isSum = PaymentScheduleAnnuitet(sumCredit:creditAmount, interestRateMonth: interestRateMonth, 
                                                    creditPeriod:dates);
                    overpayment = (isSum * dates) - creditAmount;
                    mainDebt = isSum - (creditAmount * interestRateMonth);
                    creditAmount = creditAmount - mainDebt;
                    paymentSum = mainDebt + interestСharges;
                }
                else if (typeTwo.IsChecked != null && (bool)typeTwo.IsChecked)
                {
                    isSum = PaymentScheduleDiffer(sumCredit: creditAmount, creditPeriod: dates);
                    mainDebt = isSum;
                    overpayment = interestСharges;
                    creditAmount = creditAmount - isSum;
                    paymentSum = mainDebt + interestСharges;
                }
                else
                {
                    MessageBox.Show("Нужно выбрать вид платежа");
                    return;
                }
                
                for (int i = 1; interestСharges > 1; i++)
                {
                    Data tempData = new();
                    tempData.NumberPayment = Convert.ToInt32(i.ToString());
                    tempData.CreditAmount = creditAmount.ToString("N2");
                    tempData.Precent = interestСharges.ToString("N2");
                    tempData.MainDebt = mainDebt.ToString("N2");
                    tempData.DataPayment = today.AddDays(addDays).ToString("Y");
                    tempData.PaymentSum = paymentSum.ToString("N2");
                    MyData.Items.Add(tempData);

                    interestСharges = creditAmount * interestRateMonth;
                    mainDebt = isSum - (creditAmount * interestRateMonth);
                    if (typeTwo.IsChecked != null && (bool)typeTwo.IsChecked)
                    {
                         mainDebt = isSum;
                         overpayment += interestСharges;
                    }
                    creditAmount = creditAmount - mainDebt;
                    paymentSum = mainDebt + interestСharges;
                    addDays += 30;
                    if (creditAmount < 0) creditAmount = 0;
                }
                
                var sumPayment = double.Parse(sum.Text) + overpayment;
                itog.Text = sumPayment.ToString("N2");
                // monpercent.Text = interestRateMonth.ToString("N2");
                overp.Text = overpayment.ToString("N1");
                everymon.Text = isSum.ToString("N2");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Ошибка: {exception}");
            }
        }
        private double PaymentScheduleAnnuitet(double sumCredit, double interestRateMonth, int creditPeriod)
        {
            // Формула: { p = m × (1 + m)ⁿ | (1 + m)ⁿ — 1 }
            var payment = sumCredit * (interestRateMonth / (1 - Math.Pow(1 + interestRateMonth, -creditPeriod)));
            return payment;
        }

        private double PaymentScheduleDiffer(double sumCredit, int creditPeriod)
        {
            // Формула: { m = a / b }
            var mainPayment = sumCredit / creditPeriod;
            return mainPayment;
        }

        private void MyData_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*
             * Добавление данных в файл json формата
             */
            if (MyData.Items.Count == 0)
            {
                MessageBox.Show("Тут пусто!");
                return;
            }
            
            toFormat form = new toFormat();
            form.ShowDialog();
            string nameFile = form.nameFile.Text;
            string typeFile = form.formatFile.Text;
            string pathFile = $"saved_{typeFile}/{nameFile}.{typeFile}";
            // var saveFile = MessageBox.Show(
            //     "Сохранить данные в json формате?", "Сохранение файла", 
            //     MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (typeFile == "json")
            {
                string jsonSerialize = JsonConvert.SerializeObject(MyData.Items, Formatting.Indented);
                File.WriteAllText(pathFile, jsonSerialize, Encoding.UTF8);
                MessageBox.Show($"Файл успешно создан! Путь: {pathFile}");
            }
            if (typeFile == "pdf")
            {
                MessageBox.Show($"Файл успешно создан! Путь: {pathFile}");
            }
            if (typeFile == "xls")
            {
                byte[] generated = GenerateXls();
                File.WriteAllBytes(path:pathFile, bytes:generated);
                MessageBox.Show($"Файл успешно создан! Путь: {pathFile}");
            }
        }

        private byte[] GenerateXls()
        {
            /*
             * Добавление данных в excel файл
             */
            int row = 0;
            var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Credit Result");
            
            sheet.Cells[1, 1].Value = "Дата";
            sheet.Cells[1, 2].Value = "Сумма платежа";
            sheet.Cells[1, 3].Value = "Основной долг";
            sheet.Cells[1, 4].Value = "Начисленные проценты";
            sheet.Cells[1, 5].Value = "Остаток задолженности";
            sheet.Column(1).Width = 25;
            sheet.Column(2).Width = 15;
            sheet.Column(3).Width = 25;
            sheet.Column(4).Width = 25;
            sheet.Column(5).Width = 25;
            sheet.Cells["A1:E1"].Style.Font.Bold = true; // Жирный шрифт
            
            foreach (Data d in MyData.Items)
            {
                row = d.NumberPayment + 1;
                sheet.Cells[row, 1].Value = d.DataPayment;
                sheet.Cells[row, 2].Value = d.PaymentSum;
                sheet.Cells[row, 3].Value = d.MainDebt;
                sheet.Cells[row, 4].Value = d.Precent;
                sheet.Cells[row, 5].Value = d.CreditAmount;
            }
            
            sheet.Cells[row+2, 1].Value = "Итого по кредиту";
            sheet.Cells[$"A{row+2}:A{row+3}"].Style.Font.Bold = true; // Жирный шрифт
            sheet.Cells[row+3, 1].Value = "Ежемесячный платёж";

            sheet.Cells[row+2, 2].Value = itog.Text;
            sheet.Cells[row+2, 3].Value = sum.Text;
            sheet.Cells[row+2, 4].Value = overp.Text;
            sheet.Cells[row+3, 2].Value = everymon.Text;

            sheet.Protection.IsProtected = true; // Защита от редактирования
            return package.GetAsByteArray();
        }
    }
}
