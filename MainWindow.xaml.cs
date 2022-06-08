using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using iTextSharp.text.pdf;

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
            public string NumberPayment { get; set; }
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
                    tempData.NumberPayment = i.ToString();
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
                monpercent.Text = interestRateMonth.ToString("N2");
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
        }
    }
}
