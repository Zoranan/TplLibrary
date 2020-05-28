using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TplLib;
using TplLib.Extensions;

namespace TplGui
{
    /// <summary>
    /// Interaction logic for ResultsGrid.xaml
    /// </summary>
    public partial class ResultsGridWindow : Window
    {
        public ResultsGridWindow()
        {
            InitializeComponent();
        }

        internal void InitDataGrid(IEnumerable<TplResult> tplResults)
        {
            ResultsGrid.Columns.Clear();

            //TplResults = tplResults;
            var columns = tplResults
                .GetAllFields()
                .Select(f =>
                    new DataGridTextColumn()
                    {
                        Binding = new Binding($@"Fields[{f}]") { Mode = BindingMode.OneWay },
                        Header = f.Replace("_", "__"),
                    }
                );

            foreach (var c in columns)
                ResultsGrid.Columns.Add(c);

            ResultsGrid.ItemsSource = tplResults;
        }
    }
}
