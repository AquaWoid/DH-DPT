using Avalonia.Controls;
using Avalonia.Data;
using DHM.ViewModels;


namespace DHM.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

    }


    private void UpdateDataGrid()
    {
        var grid = this.FindControl<DataGrid>("MyGrid");
        var vm = (MainViewModel)DataContext;

        var cols = vm.dataView.Table.Columns;

        for (var i = 0; i < cols.Count; i++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = cols[i].ColumnName,
                Binding = new Binding($"Row.ItemArray[{i}]"),
            });
        }
    }

}
