using FastColoredTextBoxNS;
using Irony;
using Irony.WinForms;
using Irony.WinForms.Highlighter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TplLib;
using TplParser;

namespace TplGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string _tempPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "~TEMP.tpl");
        private FastColoredTextBoxHighlighter _highlighter;
        private IronyTextBox _textBox;

        public MainWindow()
        {
            InitializeComponent();
            _textBox = new IronyTextBox();
            _textBox.FastColoredTextBox.Zoom = 125;
            textBoxHolder.Child = _textBox;
            _textBox.FastColoredTextBox.ChangeFontSize(2);
            _textBox.Load += (s, e) => _textBox.Focus();

            _highlighter = new FastColoredTextBoxHighlighter(_textBox.FastColoredTextBox, new Irony.Parsing.LanguageData(new TplGrammar()));
            _highlighter.Adapter.Activate();

            AppDomain.CurrentDomain.ProcessExit += (s, e) => File.WriteAllText(_tempPath, _textBox.Text);

            if (File.Exists(_tempPath))
                _textBox.Text = File.ReadAllText(_tempPath);
        }

        private void RunTplQuery()
        {
            try
            {
                var query = Tpl.Create(_textBox.Text, out IReadOnlyList<LogMessage> parseErrors);
                if (query == null)
                {
                    errorPane.Visibility = Visibility.Visible;
                    errorListBox.ItemsSource = parseErrors.Select(e => $"{e.Message}. Line: {e.Location.Line}, Col: {e.Location.Column}");
                }
                else
                {
                    errorPane.Visibility = Visibility.Collapsed;
                    Console.WriteLine("Pipeline Created");
                    //Execute

                    var results = query.Process();
                    var resultWindow = new ResultsGridWindow();
                    resultWindow.InitDataGrid(results);
                    resultWindow.Show();
                }
            }
            catch (Exception e)
            {
                errorPane.Visibility = Visibility.Visible;
                errorListBox.ItemsSource = new List<string>() { e.Message };
            }
        }

        private void closeErrorPane_ButtonClick(object sender, RoutedEventArgs e)
        {
            errorPane.Visibility = Visibility.Collapsed;
        }

        private void textBoxHolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                e.Handled = true;
                RunTplQuery();
            }
        }
    }
}
