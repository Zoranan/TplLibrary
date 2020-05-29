using FastColoredTextBoxNS;
using Irony;
using Irony.WinForms;
using Irony.WinForms.Highlighter;
using Microsoft.Win32;
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
        private readonly FastColoredTextBoxHighlighter _highlighter;
        private readonly IronyTextBox _textBox;
        private bool _running = false;
        private string _currentFilePath = null;

        public MainWindow()
        {
            InitializeComponent();

            //Set up the Irony FCTB
            _textBox = new IronyTextBox();
            _textBox.FastColoredTextBox.BackColor = System.Drawing.Color.FromArgb(255, 30, 30, 30);
            _textBox.FastColoredTextBox.ForeColor = System.Drawing.Color.White;
            _textBox.FastColoredTextBox.SelectionColor = System.Drawing.Color.FromArgb(150, 50, 180, 200);
            _textBox.FastColoredTextBox.CaretColor = System.Drawing.Color.White;
            
            _textBox.FastColoredTextBox.Zoom = 125;

            textBoxHolder.Child = _textBox;
            _textBox.FastColoredTextBox.ChangeFontSize(2);
            _textBox.Load += (s, e) => _textBox.FastColoredTextBox.Focus();

            var colorSettings = new ColorSettings()
            {
                Default =   new TextStyle(ColorUtil.RGB(255, 255, 255), null, System.Drawing.FontStyle.Regular),
                Comment =   new TextStyle(ColorUtil.RGB(90, 160, 70), null, System.Drawing.FontStyle.Regular),
                Keyword =   new TextStyle(ColorUtil.RGB(60, 130, 170), null, System.Drawing.FontStyle.Regular),
                Identifier = new TextStyle(ColorUtil.RGB(170, 200, 220), null, System.Drawing.FontStyle.Regular),
                Number =    new TextStyle(ColorUtil.RGB(180, 200, 155), null, System.Drawing.FontStyle.Regular),
                String =    new TextStyle(ColorUtil.RGB(215, 160, 130), null, System.Drawing.FontStyle.Regular),
                Text =      new TextStyle(ColorUtil.RGB(255, 255, 255), null, System.Drawing.FontStyle.Regular),
            };

            _highlighter = new FastColoredTextBoxHighlighter(_textBox.FastColoredTextBox, new Irony.Parsing.LanguageData(new TplGrammar()), colorSettings);
            _highlighter.Adapter.Activate();

            //Auto Save
            AppDomain.CurrentDomain.ProcessExit += (s, e) => File.WriteAllText(_tempPath, _textBox.Text);

            if (File.Exists(_tempPath))
                _textBox.Text = File.ReadAllText(_tempPath);
        }

        private async void RunTplQuery()
        {
            if (_running)
                return;

            try
            {
                _running = true;

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
                    ProcessingOverlay.Visibility = Visibility.Visible;
                    textBoxHolder.Visibility = Visibility.Hidden;
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    var results = await Task.Run(() => query.Process());
                    sw.Stop();
                    TimeTextBlock.Text = sw.ElapsedMilliseconds + "ms";

                    var resultWindow = new ResultsGridWindow();
                    resultWindow.InitDataGrid(results);
                    resultWindow.Owner = this;
                    resultWindow.Show();
                }
            }
            catch (Exception e)
            {
                errorPane.Visibility = Visibility.Visible;
                errorListBox.ItemsSource = new List<string>() { e.Message };
                File.WriteAllText("Error.txt", e.ToString());
            }
            finally
            {
                _running = false;
                textBoxHolder.Visibility = Visibility.Visible;
                ProcessingOverlay.Visibility = Visibility.Hidden;
            }
        }

        #region File save and load
        private void SaveCurrentText()
        {
            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                SaveCurrentTextAs();
                return;
            }

            try
            {
                File.WriteAllText(_currentFilePath, _textBox.Text);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error saving file. {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCurrentTextAs()
        {
            var sfd = new SaveFileDialog()
            {
                Filter = "Text Processing Language File (*.tpl) | *.tpl",
                DefaultExt = "*.tpl",
            };
            var dialogResult = sfd.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                _currentFilePath = sfd.FileName;
                SaveCurrentText();
                Title = _currentFilePath;
            }
        }

        private void OpenFile()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Text Processing Language File (*.tpl) | *.tpl",
                DefaultExt = "*.tpl",
            };
            var dialogResult = ofd.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                _currentFilePath = ofd.FileName;
                Title = _currentFilePath;
                try
                {
                    _textBox.Text = File.ReadAllText(_currentFilePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error saving file. {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NewTplFile()
        {
            _currentFilePath = null;
            _textBox.Text = "";
            Title = "Tpl";
        }

        #endregion

        private void CloseErrorPane_ButtonClick(object sender, RoutedEventArgs e)
        {
            errorPane.Visibility = Visibility.Collapsed;
        }

        private void TextBoxHolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                e.Handled = true;
                RunTplQuery();
            }

            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                SaveCurrentText();
            }

            else if (e.Key == Key.S && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                e.Handled = true;
                SaveCurrentTextAs();
            }

            else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                OpenFile();
            }

            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                NewTplFile();
            }
        }

        private void RunMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RunTplQuery();
        }

        private void SaveMenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveCurrentText();
        }

        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentTextAs();
        }

        private void OpenMenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NewTplFile();
        }
    }
}
