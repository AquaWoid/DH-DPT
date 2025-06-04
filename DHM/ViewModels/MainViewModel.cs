using DHM.Utilities;
using DHM.Models;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Windows.Input;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DHM.Views;
using System;
using System.Linq;
using System.Text.RegularExpressions;
//using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using Avalonia.Collections;


using ReactiveUI;
using System.ComponentModel;
using System.Dynamic;
using Avalonia.Data;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using DynamicData;
using System.Diagnostics;





namespace DHM.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Variable Declarations

    [Reactive] public string debugText { get; set; } = "Debug";

    [Reactive] public ObservableCollection<Factoid>? factoids { get; set; }


    [Reactive] public ObservableCollection<Factoid>? filteredCollection { get; set; } = new ObservableCollection<Factoid>();


    [Reactive] public string parseMessage { get; set; }


    [Reactive] public DataTable gridDataTable { get; set; }


    public ObservableCollection<TableFilter> tableFilters { get; set; } = new ObservableCollection<TableFilter>();

    [Reactive] public DataView dataView { get; set; } = new DataView();


    List<string> tableColumns = new List<string>();


    public DataTable tableData { get; set; }


    [Reactive] public ObservableCollection<string> Columns { get; set; } = new ObservableCollection<string> { };


    [Reactive] public ITreeDataGridSource GridSource { get; set; }


    private ObservableCollection<Factoid> baseFactoids { get; set; }


    [Reactive] public ObservableCollection<string> AvailableColumns { get; set; } = new ObservableCollection<string>();


    [Reactive] public string columnNameSelected { get; set; }

    #endregion


    #region Data Handling

    //Entry function to pull data from the factoid json
    public async void getJson()
    {

        factoids = await JsonParse.ParseJson<Factoid>("C:\\Users\\luwa0\\Documents\\Coding\\jsonParse\\jsonParse\\Models\\factoidfull.json");

        baseFactoids = factoids;

        // filteredCollection = factoids;

        parseMessage = "Sucessfully Parsed JSON with " + factoids.Count.ToString() + " Entries";

        UpdateTableFilter();
    }

    //Json Export Function. Exports the edited Factoid as well as the filter settings
    public void ExportJson()
    {
        JsonExport.ExportJson<TableFilter>(tableFilters, "C:\\Users\\luwa0\\Desktop\\Code\\tableFilters.json");
        JsonExport.ExportJson<Factoid>(factoids, "C:\\Users\\luwa0\\Desktop\\Code\\factoidsExport.json");
    }

    //CSV export. Look at the Utilities/CsvExport class for more info
    public async void exportCsv()
    {
        CsvExport.DataTableToCsv(tableData);
    }


    #endregion


    #region DataTable Handling

    //Constructing the DataTable from the Factoid Collection
    public DataTable constructDataTable(List<string> filteredProperties, ObservableCollection<Factoid> factoidCollection)
    {
        //Initializing DataTable and Filtered Object Collection
        DataTable dt = new DataTable();
        List<object> filteredObjects = new List<object>();


        //Adding Columns based on the filtered properties
        foreach (string property in filteredProperties)
        {
            dt.Columns.Add(property);
        }

        //Adding Factoid Properties depending on their Condition
        foreach (Factoid factoid in factoidCollection)
        {

            filteredObjects.Clear();


            foreach (string prop in filteredProperties)
            {


                if (prop == "id")
                {
                    filteredObjects.Add(factoid.id);
                }

                if (prop == "name")
                {
                    if (factoid.name.Contains(","))
                    {
                        string filteredString = factoid.name.Replace(",", "");
                        filteredObjects.Add(filteredString);
                    }
                    else
                    {
                        filteredObjects.Add(factoid.name);
                    }

                }
                if (prop == "created_by")
                {
                    filteredObjects.Add(factoid.created_by);
                }
                if (prop == "created_when")
                {
                    filteredObjects.Add(factoid.created_when);
                }
                if (prop == "modified_when")
                {
                    filteredObjects.Add(factoid.modified_when);
                }
                if (prop == "modified_by")
                {
                    filteredObjects.Add(factoid.modified_by);
                }

                if (prop == "statements[0].id" && factoid.has_statements.Count > 0)
                {
                    filteredObjects.Add(factoid.has_statements[0].id);
                }
                if (prop == "statements[0].name" && factoid.has_statements.Count > 0)
                {
                    filteredObjects.Add(factoid.has_statements[0].name);
                }
                if (prop == "statements[0].__object_type__" && factoid.has_statements.Count > 0)
                {
                    filteredObjects.Add(factoid.has_statements[0].__object_type__);
                }
                if (prop == "statements[0].start_date_written" && factoid.has_statements.Count > 0)
                {
                    filteredObjects.Add(factoid.has_statements[0].start_date_written);
                }



                if (prop == "Handlung_ausgeführt_von" && factoid.has_statements.Count > 0)
                {
                    if( factoid.has_statements[0].Handlung_ausgeführt_von != null)
                    filteredObjects.Add(factoid.has_statements[0].Handlung_ausgeführt_von[0].label);
                }
                if (prop == "Ort_der_Handlung" && factoid.has_statements.Count > 0)
                {
                    if (factoid.has_statements[0].Ort_der_Handlung != null)
                        filteredObjects.Add(factoid.has_statements[0].Ort_der_Handlung[0].label);
                }

            }


            dt.Rows.Add(filteredObjects.ToArray());

        }

        return dt;

    }



    //UI Update from Filter settings 
    public async void UpdateTableFilter()
    {


        tableColumns.Clear();

        foreach (TableFilter filter in tableFilters)
        {
            if (filter.enabled == true)
            {
                tableColumns.Add(filter.name);
            }

        }

        tableData = constructDataTable(tableColumns, factoids);


        gridDataTable = tableData;
        dataView = tableData.DefaultView;


        TableDataToTreeGrid(tableData);

    }




    //Helper Function to convert the DataTable Data into a usable TreeGrid format.
    public void TableDataToTreeGrid(DataTable dt)
    {
        Columns.Clear();

        //Map columns from the datatable to a usable format
        foreach (DataColumn col in dt.Columns)
        {
            Columns.Add(col.ColumnName);
        }


        //Cast Rows to DataRows
        var rows = dt.Rows.Cast<DataRow>()
            .Select(row => row.ItemArray.Select(cell => cell?.ToString() ?? "").ToList())
            .ToList();

        //Convert into Dynamic Row
        var items = rows.Select(r => new DynamicRow(r)).ToList();


        //Fill Columns
        var columns = Columns
                .Select((header, index) => new TextColumn<DynamicRow, string>(
                    header,
                    row => row.Values[index]))
                .ToList<IColumn<DynamicRow>>();


        //Initialize UI Binding
        GridSource = new FlatTreeDataGridSource<DynamicRow>(items)
        {
            Columns = { }
        };

        //Populate Grid
        foreach (var col in columns)
            ((FlatTreeDataGridSource<DynamicRow>)GridSource).Columns.Add(col);
    }


    #endregion



    // Regex Querying function 
    public void LookupFilter()
    {
        factoids = baseFactoids;
        filteredCollection.Clear();

        string query = debugText;


        foreach (Factoid factoid in factoids)
        {
            if (Regex.IsMatch(factoid.name, query))
            {

                filteredCollection.Add(factoid);
            }
        }

        factoids = filteredCollection;
        UpdateTableFilter();
    }



    public void removeColumn()
    {

        debugText += columnNameSelected;

        foreach (TableFilter filter in tableFilters)
        {
            if(filter.name == columnNameSelected)
            {
                filter.enabled = false;
            }

        }

        UpdateTableFilter();

    }


    public void addColumn()
    {

        debugText += columnNameSelected;

        foreach (TableFilter filter in tableFilters)
        {
            if (filter.name == columnNameSelected)
            {
                filter.enabled = true;
            }

        }

        UpdateTableFilter();

    }

    public async void initializeFilters()
    {

        tableFilters = await JsonParse.ParseJson<TableFilter>("C:\\Users\\luwa0\\Desktop\\Code\\tableFilters.json");

        foreach (TableFilter filter in tableFilters)
        {
           AvailableColumns.Add(filter.name);
        }
    }

    #region Commands

    public ICommand updateTableDataCommand { get; }
    public ICommand applyQueryCommand { get; }

    public ICommand jsonExportCommand { get; }

    public ICommand csvExportCommand { get; }

    public ICommand parseCommand { get; }

    public ICommand lookupFilterCmd { get; }

    public ICommand removeColumnCommand { get; }

    public ICommand addColumnCommand { get; }



    #endregion

    //Class and Command Initialization
    public MainViewModel() {

        parseCommand = ReactiveCommand.Create(() => {getJson();});

        lookupFilterCmd = ReactiveCommand.Create(() => { LookupFilter(); }); 

        jsonExportCommand = ReactiveCommand.Create(() => { ExportJson(); });

        csvExportCommand = ReactiveCommand.Create(() => { exportCsv(); });

        updateTableDataCommand = ReactiveCommand.Create(() => {UpdateTableFilter();});

        applyQueryCommand = ReactiveCommand.Create(() => {LookupFilter();});

        removeColumnCommand = ReactiveCommand.Create(() => {removeColumn();});

        addColumnCommand = ReactiveCommand.Create(() => { addColumn(); });

        initializeFilters();

    }
}
