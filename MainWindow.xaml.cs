using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Кредитный_калькулятор
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public class Data
        {
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
            monpercent.Text = "";
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
                //var textType = "Аннуитентный";
                var dates = int.Parse(months.Text);
                var interestRateMonth = myPercent / 100 / 12;
                var interestСharges = creditAmount * interestRateMonth;
                double isSum;
                double overpayment;
                
                itog.Text = creditAmount.ToString("N2");
                monpercent.Text = interestRateMonth.ToString("N2");

                if (typeOne.IsChecked != null && (bool)typeOne.IsChecked)
                {
                    isSum = PaymentScheduleAnnuitet(sumCredit:creditAmount, interestRateMonth: interestRateMonth, 
                                                    creditPeriod:dates);
                    overpayment = (isSum * dates) - creditAmount;
                    var mainDebt = isSum - (creditAmount * interestRateMonth);
                    for (int i = 0; i != 1;)
                    {
                        Data tempData = new();
                        tempData.CreditAmount = creditAmount.ToString("N2");
                        tempData.Precent = interestСharges.ToString("N2");
                        tempData.MainDebt = mainDebt.ToString("N2");
                        MyData.Items.Add(tempData);

                        creditAmount = creditAmount - mainDebt;
                        interestСharges = creditAmount * interestRateMonth;
                        mainDebt = isSum - (creditAmount * interestRateMonth);
                        if (creditAmount <= 0) break;
                    }
                }
                else if (typeTwo.IsChecked != null && (bool)typeTwo.IsChecked)
                {
                    isSum = PaymentScheduleDiffer(sumCredit: creditAmount, creditPeriod: dates);
                    overpayment = (isSum * dates) - creditAmount;
                    for (int i = 0; i != 1;)
                    {
                        Data tempData = new();
                        tempData.CreditAmount = creditAmount.ToString("N2");
                        tempData.Precent = interestСharges.ToString("N2");
                        tempData.MainDebt = isSum.ToString("N2");
                        MyData.Items.Add(tempData);
                        
                        creditAmount = creditAmount - isSum;
                        interestСharges = creditAmount * interestRateMonth;
                        if (creditAmount <= 0) break;
                    }
                }
                else
                {
                    MessageBox.Show("Нужно выбрать вид платежа");
                    return;
                }
                
                overp.Text = (overpayment).ToString("N1");
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
            
            var saveFile = MessageBox.Show(
                "Сохранить данные в json формате?", "Сохранение файла", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (saveFile == MessageBoxResult.Yes)
            {
                string jsonSerialize = JsonConvert.SerializeObject(MyData.Items, Formatting.Indented);
                File.WriteAllText("details.json", jsonSerialize, Encoding.UTF8);
            }
        }
    }
}
