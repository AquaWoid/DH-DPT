using DHM.Utilities;
using DHM.Models;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Windows.Input;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using Avalonia.Data;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using DynamicData;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DHM.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Variable Declarations

    [Reactive] public string debugText { get; set; } = "Debug";

    private string _filterText = "Input Query";

    public string filterText {

        get => _filterText;
        set { 
            
            this.RaiseAndSetIfChanged(ref _filterText, value);
            LookupFilter();
       
        }
    }

    [Reactive] public ObservableCollection<Factoid>? factoids { get; set; }


    [Reactive] public ObservableCollection<Factoid>? filteredCollection { get; set; } = new ObservableCollection<Factoid>();


    [Reactive] public string parseMessage { get; set; }


    [Reactive] public DataTable gridDataTable { get; set; }


    public ObservableCollection<TableFilter> tableFilters { get; set; } = new ObservableCollection<TableFilter>();



    List<string> tableColumns = new List<string>();


    public DataTable tableData { get; set; }


    [Reactive] public ObservableCollection<string> Columns { get; set; } = new ObservableCollection<string> { };


    [Reactive] public ITreeDataGridSource GridSource { get; set; }


    private ObservableCollection<Factoid> baseFactoids { get; set; }


    [Reactive] public ObservableCollection<string> AvailableColumns { get; set; } = new ObservableCollection<string>();

    [Reactive] public ObservableCollection<string> ActiveColumns { get; set; } = new ObservableCollection<string>();


    [Reactive] public string columnNameSelected { get; set; }

    private string filterOptionPath;

    [Reactive] public bool filterDates { get; set; } = false;

    [Reactive] public bool parseDynamic { get; set; } = false;


    [Reactive] public int progressBarValue { get; set; } = 0;


    [Reactive] public int progressBarMaximum { get; set; } = 100;

    string dynamicPath;

    #endregion

    #region Data Handling


    //Initializes the Filter Config file into AppData/Roaming/DHM if there is none found. If file exists > Load into the TableFilter class. 
    public async void initializeFilters()
    {
        try
        {
            tableFilters = await JsonParse.ParseJson<TableFilter>(Path.Join(OptionSerialization.GetOptionPath(), "tableFilters.json"));
        }
        catch (Exception ex)
        {
            if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                OptionSerialization.InitializeOptionSaveFile();
                initializeFilters();
            }
        }

        //Checking if the filters loaded from the file are enabled
        foreach (TableFilter filter in tableFilters)
        {
            //Adding to the AvailableColumns Observablecollection which is used to control which columns can be added to the DataTable
            AvailableColumns.Add(filter.name);

            if (filter.enabled == true)
            {
                ActiveColumns.Add(filter.name);
                SortActiveColumns();
            }
        }
    }

    //Entry function to pull data from the a json provided through the file dialog
    public async void getJson(string path)
    {

        factoids = await JsonParse.ParseJson<Factoid>(path);

        if (filterDates == true)
        {
            foreach (Factoid factoid in factoids)
            {
                if (factoid.has_statements.Count > 0)
                {
                    try
                    {
                        factoid.has_statements[0].start_date_written = Regex.Match(factoid.has_statements[0].start_date_written, "\\d{4}").Value;
                        //filteredObjects.Add(extractYear(factoid.has_statements[0].start_date_written));
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Couldn't find regex pattern");

                    }
                }
            }
        }

        baseFactoids = factoids;

       // parseMessage = "Sucessfully Parsed JSON with " + factoids.Count.ToString() + " Entries";

       UpdateTableFilter();
    }



    //Json Export Function. Exports the tableFilters class collection into tablefilters.json. Directory: AppData/Roaming/DHM
    public void SaveConfig()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string exportPath = Path.Combine(appDataPath, "DHM");
        Directory.CreateDirectory(exportPath);

        JsonExport.ExportJson<TableFilter>(tableFilters, Path.Combine(exportPath, "tableFilters.json"));

        //JsonExport.ExportJson<Factoid>(factoids, "C:\\Users\\luwa0\\Desktop\\Code\\factoidsExport.json");
    }

    //CSV export. Look at the Utilities/CsvExport class for more info
    public async void exportCsv()
    {
        CsvExport.DataTableToCsv(tableData);
    }


    #endregion

    #region DataTable Handling


    public async Task<DataTable> ConstructDynamicDatatable() {


        var json = File.ReadAllText(dynamicPath, new UTF8Encoding(false));




        JToken root = JToken.Parse(json);
        JArray items = root.Type == JTokenType.Array ? (JArray)root : new JArray(root);

        DataTable dt = new DataTable();

        AvailableColumns.Clear();

        progressBarMaximum = items.Count;

        foreach (JToken item in items)
        {
            var flat = FlatJson.FlattenJToken(item);

            // Add columns if not already present
            foreach (var key in flat.Keys)
            {
                if (!dt.Columns.Contains(key)) {
                    dt.Columns.Add(key, flat[key]?.GetType() ?? typeof(object));
                    AvailableColumns.Add(key);
                
                }
  
            }

            // Create row with values in column order
            var row = dt.NewRow();
            foreach (var kvp in flat)
                row[kvp.Key] = kvp.Value ?? DBNull.Value;

            dt.Rows.Add(row);
            progressBarValue += 1;



        }

        return dt;  


    }

    //Constructing the DataTable from the Factoid Collection
    public async Task<DataTable> constructDataTable(List<string> filteredProperties, ObservableCollection<Factoid> factoidCollection)
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

            //Mapping the parsed class variables to the filtered collection
            foreach (string prop in filteredProperties)
            {


                if (prop == "id")
                {
                    filteredObjects.Add(factoid.id);
                }

                if (prop == "name")
                {

                    filteredObjects.Add(CheckAndRemoveCommas(factoid.name));

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
                    filteredObjects.Add(CheckAndRemoveCommas(factoid.has_statements[0].Handlung_ausgeführt_von[0].label));
                }
                if (prop == "Ort_der_Handlung" && factoid.has_statements.Count > 0)
                {
                    if (factoid.has_statements[0].Ort_der_Handlung != null)
                    {
                        filteredObjects.Add(CheckAndRemoveCommas(factoid.has_statements[0].Ort_der_Handlung[0].label));
                    }

                }

            }

            //Adding all objects as Rows
            dt.Rows.Add(filteredObjects.ToArray());

        }

        return dt;

    }


    //#1 UI Update from Filter settings 
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




        if (parseDynamic == true)
        {
            tableData = await ConstructDynamicDatatable();
        }
        else {
            //Method Call to construct a DataTable from the class Collection
            tableData = await constructDataTable(tableColumns, factoids);
        }

        //tableData = ConstructDynamicDatatable();
        TableDataToTreeGrid(tableData);

    }

    //Helper Function to convert the DataTable Data into a usable TreeGrid format.
    public void TableDataToTreeGrid(DataTable dt)
    {
        //Clear Previous Display
        Columns.Clear();

        //Map columns from the datatable to a usable format
        foreach (DataColumn col in dt.Columns)
        {
            Columns.Add(col.ColumnName);
        }


        //Cast DataTable Rows to DataRows
        var rows = dt.Rows.Cast<DataRow>()
            .Select(row => row.ItemArray.Select(cell => cell?.ToString() ?? "").ToList())
            .ToList();

        //Convert into Dynamic Row (List<string>)
        var items = rows.Select(row => new DynamicRow(row)).ToList();


        //Fill Columns
        var columns = Columns
                .Select((header, index) => new TextColumn<DynamicRow, string>(
                    header,
                    row => row.Values[index]
                    //(row, value) => row.Values[index] = value
                    )
                )
                .ToList<IColumn<DynamicRow>>();


        //Initialize UI Binding through GridSource 
        GridSource = new FlatTreeDataGridSource<DynamicRow>(items)
        {
            Columns = { }
        };

        //Populate Grid
        foreach (var col in columns)
            ((FlatTreeDataGridSource<DynamicRow>)GridSource).Columns.Add(col);
    }



    // Regex Querying function 
    public async void LookupFilter()
    {
        // Resetting Working Factoids to Initial Data
        factoids = baseFactoids;
        // Re-Initializing the Filtered Collection
        filteredCollection.Clear();

        string query = filterText;

        //Check for Valid Regex query and add the matching results to the filtered collection which is transfered to the base object that is bound to the UI 
        foreach (Factoid factoid in factoids)
        {

            if (await Task.Run(() => isValidRegex(filterText)) && Regex.IsMatch(factoid.name, filterText))
            {

                filteredCollection.Add(factoid);
            }
        }

        factoids = filteredCollection;
        UpdateTableFilter();
    }


    //Removes column from the current filtering and updates all filters
    public void removeColumn()
    {

        debugText += columnNameSelected;

        foreach (TableFilter filter in tableFilters)
        {
            if (filter.name == columnNameSelected && filter.enabled == true)
            {
                filter.enabled = false;

                ActiveColumns.Remove(filter.name);
                SortActiveColumns();
            }

        }

        UpdateTableFilter();

    }

    //Adds column to the current filter and updates all filters
    public void addColumn()
    {

        debugText += columnNameSelected;

        foreach (TableFilter filter in tableFilters)
        {
            if (filter.name == columnNameSelected && filter.enabled == false)
            {
                filter.enabled = true;
                ActiveColumns.Add(filter.name);
                SortActiveColumns();
            }


        }

        UpdateTableFilter();

    }


    #endregion

    #region Helper Functions 

    //Text filtering function to remove unwanted commas to prevent breaking csv parsing
    private string CheckAndRemoveCommas(string input)
    {
        if (input.Contains(","))
        {

            string filteredString = input.Replace(",", "");
            return filteredString;

        }
        else
        {
            return input;
        }
    }



    //Regex Validation Function to avoid Parse Errors
    private async Task<bool> isValidRegex(string query)
    {
        try
        {
            Regex.Match("test", query);
            return true;
        }
        catch (RegexParseException)
        {

            return false;
        }
    }

    //Sort Indexes of the ActiveColumn ObservableCollection by the indexes of the AvailableColumns collection.
    private void SortActiveColumns()
    {
        var indexMap = AvailableColumns.Select((item, index) => new { item, index }).ToDictionary(x => x.item, x => x.index);

        var sorted = ActiveColumns.OrderBy(item => indexMap[item]).ToList();

        ActiveColumns.Clear();
        foreach (var item in sorted)
        {
            ActiveColumns.Add(item);
        }
    }

    //Is called from MainView.axaml.cs to redirect the path of the file dialog input
    public void receiveFileDialog(string path)
    {
        dynamicPath = path;
        getJson(path);
    }

    #endregion

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

    #region Class Constructor
    //Class and Command Initialization
    public MainViewModel() {

      //  parseCommand = ReactiveCommand.Create(() => {getJson();});

        lookupFilterCmd = ReactiveCommand.Create(() => { LookupFilter(); }); 

        jsonExportCommand = ReactiveCommand.Create(() => { SaveConfig(); });

        csvExportCommand = ReactiveCommand.Create(() => { exportCsv(); });

      //  updateTableDataCommand = ReactiveCommand.Create(() => {UpdateTableFilter();});

        applyQueryCommand = ReactiveCommand.Create(() => {LookupFilter();});

        removeColumnCommand = ReactiveCommand.Create(() => {removeColumn();});

        addColumnCommand = ReactiveCommand.Create(() => { addColumn(); });

        initializeFilters();

    }

    #endregion
}
