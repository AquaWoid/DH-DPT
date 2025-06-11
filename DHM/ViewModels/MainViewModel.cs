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
using DynamicData;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Reflection.Metadata.Ecma335;


namespace DHM.ViewModels;

public class MainViewModel : ViewModelBase
{
    #region Variable Declarations

    //Text bindings
    [Reactive] public string debugText { get; set; } = "Debug";

    private string _filterText = "Input Query";

    //Regex Query Text Binding
    public string filterText {

        get => _filterText;
        set { 
            
            this.RaiseAndSetIfChanged(ref _filterText, value);
            LookupFilter();
       
        }
    }

    //Factoid Collections
    [Reactive] public ObservableCollection<Factoid>? factoids { get; set; }

    private ObservableCollection<Factoid> baseFactoids { get; set; }


    [Reactive] public ObservableCollection<Factoid>? filteredCollection { get; set; } = new ObservableCollection<Factoid>();

    //Table Collections and Variables
    public ObservableCollection<TableFilter> tableFilters { get; set; } = new ObservableCollection<TableFilter>();


    List<string> tableColumns = new List<string>();


    public DataTable tableData { get; set; }


    [Reactive] public ObservableCollection<string> Columns { get; set; } = new ObservableCollection<string> { };

    [Reactive] public ITreeDataGridSource GridSource { get; set; }

    [Reactive] public ObservableCollection<string> AvailableColumns { get; set; } = new ObservableCollection<string>();

    [Reactive] public ObservableCollection<string> ActiveColumns { get; set; } = new ObservableCollection<string>();


    [Reactive] public string columnNameSelected { get; set; }

    //Toggles
    [Reactive] public bool filterDates { get; set; } = false;

    [Reactive] public bool parseDynamic { get; set; } = false;

    //Progress Bar 
    [Reactive] public int progressBarValue { get; set; } = 0;


    [Reactive] public int progressBarMaximum { get; set; } = 100;

    //Global Paths
    string dynamicPath;


    #endregion

    #region Data Handling


    //Initializes the Filter Config file into AppData/Roaming/DHM if there is none found. If file exists > Load into the TableFilter class. 
    public async void initializeFilters()
    {

        //Checking if the file exists in AppData/Roaming/DHM
        try
        {
            tableFilters = await JsonParse.ParseJson<TableFilter>(Path.Join(OptionSerialization.GetOptionPath(), "tableFilters.json"));
        }
        //IF not exist > Initialize file and directory. 
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
            //Adding to the AvailableColumns Observablecollection that is used to control which columns can be added to the DataTable
            AvailableColumns.Add(filter.name);

            //Adds filter to the ActiveColumns List (Binding for the UI ListBox) and calls the Column Sort function to match AvailableColumns
            if (filter.enabled == true)
            {
                ActiveColumns.Add(filter.name);
                SortActiveColumns();
            }
        }

        //Initializing the selected filter to avoid erros while using the regex lookup. 
        columnNameSelected = AvailableColumns[0];

    }

    //Entry function to pull data from the a json provided through the file dialog
    public async void getJson(string path)
    {
        //Task call to parse input JSON as Factoid class
        factoids = await JsonParse.ParseJson<Factoid>(path);

        //Filtering years out of the single start_date_written entries if true
        if (filterDates == true)
        {
            foreach (Factoid factoid in factoids)
            {
                if (factoid.has_statements.Count > 0)
                {
                    try
                    {
                        factoid.has_statements[0].start_date_written = Regex.Match(factoid.has_statements[0].start_date_written, "\\d{4}").Value;
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Couldn't find regex pattern");
                    }
                }
            }
        }

        //Applying factoid collection to the baseFactoids collection to draw data from it once filtering has happened to the factoid collection. 
        baseFactoids = factoids;

       //UI Update Call
       UpdateTableFilter();
    }



    //Json Export Function. Exports the tableFilters class collection into tablefilters.json. Directory: AppData/Roaming/DHM
    public void SaveConfig()
    {
        string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DHM");
        Directory.CreateDirectory(exportPath);

        JsonExport.ExportJson<TableFilter>(tableFilters, Path.Combine(exportPath, "tableFilters.json"));
    }

    //CSV export. Look at the Utilities/CsvExport class for more info
    public async void exportCsv()
    {
        CsvExport.DataTableToCsv(tableData);
    }

    //JSON Export Function. Exports the current DataTable Data into a JSON file under Documents/DHM Exports. Calls an export function in the JsonExport class (see JsonExport.cs). 
    public void exportDataTableToJSON()
    {
        string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string exportPath = Path.Join(docsPath, "DHM Exports");
        Directory.CreateDirectory(exportPath);

        JsonExport.DataTableToJson(tableData, exportPath);
    }


    #endregion

    #region DataTable Handling


    public async Task<DataTable> ConstructDynamicDatatable() {

        //Pulling file contents from File Dialog Input
        var json = File.ReadAllText(dynamicPath, new UTF8Encoding(false));

        //JSON root token creation
        JToken root = JToken.Parse(json);

        //Checking if root is of type JArray and apply a Jarray cast of root to items
        JArray items = root.Type == JTokenType.Array ? (JArray)root : new JArray(root);


        //Datatable initialization
        DataTable dt = new DataTable();


        //Clearing Columns
        AvailableColumns.Clear();


        //Setting Progress bar max length
        progressBarMaximum = items.Count;


        //Iterating through the json array
        foreach (JToken item in items)
        {
            //Using the FlatJson utility class to 
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
            //For each Key Value Pair
            foreach (var kvp in flat)
                row[kvp.Key] = kvp.Value ?? DBNull.Value;
            //Add Row
            dt.Rows.Add(row);
            //Advance Progress Bar 
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


    //#1 UI Update Main Function 
    public async void UpdateTableFilter()
    {
        tableColumns.Clear();

        //Initializing columns based on selected filters
        foreach (TableFilter filter in tableFilters)
        {
            if (filter.enabled == true)
            {
                tableColumns.Add(filter.name);
            }

        }

        //Bound to the "Parse Dynamic?" CheckBox
        if (parseDynamic == true)
        {
            //Task call to construct Dynamic DataTable from the parsed JSON file
            tableData = await ConstructDynamicDatatable();
        }
        else {
            //Task Call to construct a DataTable from the class Collection
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


        //Check for Valid Regex query and add the matching results to the filtered collection which is transfered to the base object that is bound to the UI 
        foreach (Factoid factoid in factoids)
        {

            if (await Task.Run(() => isValidRegex(filterText)) && Regex.IsMatch(getActiveQuerySelection(factoid).ToString(), filterText))
            {

                filteredCollection.Add(factoid);
            }
        }

        factoids = filteredCollection;
        UpdateTableFilter();
    }

    //Function to return the active query selection to the regex filter.
    private object getActiveQuerySelection(Factoid factoid)
    {
        if(columnNameSelected == "id")
        {
            return factoid.id;
        }
        if (columnNameSelected == "name")
        {
            return factoid.name;
        }
        if (columnNameSelected == "created_by")
        {
            return factoid.created_by;
        }
        if (columnNameSelected == "created_when")
        {
            return factoid.created_when;
        }
        if (columnNameSelected == "modified_by")
        {
            return factoid.modified_by;
        }
        if (columnNameSelected == "modified_when")
        {
            return factoid.modified_when;
        }


        if (factoid.has_statements.Count > 0)
        {

            if (columnNameSelected == "statements[0].id" && factoid.has_statements.Count > 0)
            {
                return factoid.has_statements[0].id;
            }
            if (columnNameSelected == "statements[0].__object_type__" && factoid.has_statements.Count > 0)
            {
                return factoid.has_statements[0].__object_type__;
            }
            if (columnNameSelected == "statements[0].start_date_written" && factoid.has_statements[0].start_date_written != null)
            {
                return factoid.has_statements[0].start_date_written;
            }


            //TODO: Both don't seem to work properly for now, but should be theoreticaly. 
            if (columnNameSelected == "Handlung_ausgeführt_von" && factoid.has_statements[0].Handlung_ausgeführt_von != null)
            {
                return factoid.has_statements[0].Handlung_ausgeführt_von;
            }
            if (columnNameSelected == "Ort_der_Handlung" && factoid.has_statements[0].Ort_der_Handlung != null)
            {
                return factoid.has_statements[0].Ort_der_Handlung;
            }

        }

        return new Factoid();

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


    public ICommand applyQueryCommand { get; }

    public ICommand jsonExportCommand { get; }

    public ICommand csvExportCommand { get; }

    public ICommand lookupFilterCmd { get; }

    public ICommand removeColumnCommand { get; }

    public ICommand addColumnCommand { get; }
    public ICommand exportJsonCommand { get; }




    #endregion

    #region Class Constructor
    //Class and Command Initialization
    public MainViewModel() {

        lookupFilterCmd = ReactiveCommand.Create(() => { LookupFilter(); }); 

        jsonExportCommand = ReactiveCommand.Create(() => { SaveConfig(); });

        csvExportCommand = ReactiveCommand.Create(() => { exportCsv(); });

        applyQueryCommand = ReactiveCommand.Create(() => {LookupFilter();});

        removeColumnCommand = ReactiveCommand.Create(() => {removeColumn();});

        addColumnCommand = ReactiveCommand.Create(() => { addColumn(); });

        exportJsonCommand = ReactiveCommand.Create(() => { exportDataTableToJSON(); });

        initializeFilters();

    }

    #endregion
}
