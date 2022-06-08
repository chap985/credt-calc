
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Кредитный_калькулятор
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class toFormat
    {
        public toFormat()
        {
            InitializeComponent();
        }

        private void CreateFile(object sender, RoutedEventArgs e)
        {
            //ComboBoxItem typeItem = (ComboBoxItem)formatFile.SelectedItem;
            string typeFile = formatFile.Text;//.SelectedItem.ToString();
            try
            {
                string nFile = $"saved_{typeFile}/{nameFile.Text}.{typeFile}";
                FileInfo file = new FileInfo(nFile);
                if (file.Exists) { MessageBox.Show("Такой файл уже существует!"); return; }
                File.WriteAllText(nFile, "");
            }
            catch
            {
                MessageBox.Show(@"Название файла не может содержать ( \ / : < > )"); return;
            }
            Close();
        }
    }
}