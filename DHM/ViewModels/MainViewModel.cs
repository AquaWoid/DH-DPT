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


namespace DHM.ViewModels;

public class MainViewModel : ViewModelBase
{


    public ObservableCollection<Factoid>? factoids;


      ObservableCollection<Factoid> filteredCollection = new ObservableCollection<Factoid>();

    [Reactive] public Factoid currentFactoid { get; set; }


    [Reactive] public string parseMessage { get; set; }
    [Reactive] public string displayText {get; set;}

    [Reactive] public string createdWhen { get; set; } = "Created When";
    [Reactive] public string createdBy { get; set; } = "Created By";
    [Reactive] public string modifiedBy { get; set; } = "Modified By";
    [Reactive] public string modifiedWhen { get; set; } = "Modified When";
    [Reactive] public string objectName { get; set; } = "Object Name";
    [Reactive] public int jsonObjectIndex { get; set; } = 0;

    [Reactive] public string statementCount { get; set; } = "Statement Count";

    [Reactive] public int statementIndex { get; set; } = 0;

    [Reactive] public string statementObjectType { get; set; } = "Statements";
    [Reactive] public string statementID { get; set; }

    [Reactive] public string statementName { get; set; }
    [Reactive] public string statementStartDate { get; set; }
    [Reactive] public string statementEndDate { get; set; }
    [Reactive] public string statementNotes { get; set; }
    [Reactive] public string statementInternalNotes { get; set; }
    [Reactive] public string statementHeadStatement { get; set; }



    [Reactive] public string jsonIndexInput { get; set; }



    [Reactive] public string filterResults { get; set; }


    private List<int> queryEntries = new List<int>();

    [Reactive] public List<int> publicQuaryEntries { get; set; }


    private int _cboSelect;

    public int cboSelect
    {
        get => _cboSelect;
        set  {
            this.RaiseAndSetIfChanged(ref _cboSelect, value);
            cboUpdateJsonIndex();
            //UpdateJsonDisplay();
        }
    }


    public ICommand parseCommand { get; }

    public ICommand nextEntry { get; }
    public ICommand previousEntry { get; }

    public ICommand nextStatement { get; }  
    public ICommand previousStatement { get; }  

    public ICommand jsonIndexChange { get; }    

    public ICommand lookupFilterCmd { get; }

    public async void getJson()
    {

        /*
         
            string path = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Factoid Json",
            AllowMultiple = false
        });     
         */

        factoids = await JsonParse.ParseJson("C:\\Users\\luwa0\\Documents\\Coding\\jsonParse\\jsonParse\\Models\\factoidfull.json");
        UpdateJsonDisplay();
        parseMessage = "Sucessfully Parsed JSON with " + factoids.Count.ToString() + " Entries";
    }


    public void selectNextEntry()
    {
        statementIndex = 0;
        jsonObjectIndex++;
        if (jsonObjectIndex >= factoids.Count) { jsonObjectIndex = factoids.Count -1; }

        UpdateJsonDisplay();      
    }


    public void selectPreviousEntry() {

        statementIndex = 0;
        jsonObjectIndex--;
        if (jsonObjectIndex < 0 ) { jsonObjectIndex = 0; }

        UpdateJsonDisplay();
    }


    public void selectNextStatement() {

        if (statementIndex < factoids[jsonObjectIndex].has_statements.Count -1) {
            statementIndex++;
            UpdateJsonDisplay();
        }       
    }


    public void selectPreviousStatement()
    {
        if(statementIndex > 0) { 
            statementIndex--; 
            UpdateJsonDisplay() ;     
        }
    }

    public void removeStatement() {

        factoids[jsonObjectIndex].has_statements.RemoveAt(statementIndex);    
    
    }

    public void updateJsonIndex () {

        try
        {
            if (int.Parse(jsonIndexInput) < factoids.Count - 1)
            {
                jsonObjectIndex = int.Parse(jsonIndexInput);
                UpdateJsonDisplay();
            }
        }
        catch (Exception Ex)
        {
            jsonIndexInput = Ex.Message;
        }
    }

    public void LookupFilter()
    {
        int count = 0;
        filterResults = string.Empty;
        queryEntries.Clear();

        int stringLength = 0;

        string query = jsonIndexInput;


        foreach (Factoid factoid in factoids)
        {

            if(Regex.IsMatch(factoid.name, query))
            {
                queryEntries.Add(count);
                stringLength += count.ToString().Length;
                filteredCollection.Add(factoid);
            }

            count++;
        }

        foreach(int entry in queryEntries)
        {
            filterResults += entry + ", ";
        }


        filterResults += queryEntries;
        publicQuaryEntries = queryEntries;
    }

    private void UpdateJsonDisplay()
    {
        if(factoids != null)
        {
            currentFactoid = factoids[jsonObjectIndex];


            displayText = "ID: " + currentFactoid.id.ToString() + " | " + jsonObjectIndex.ToString() +  " / " + (factoids.Count -1).ToString();
            objectName = "Name: " + currentFactoid.name;
            createdWhen = "Date Created: " + currentFactoid.created_when.ToString();

            createdBy = "Created By: " + currentFactoid.created_by;

            modifiedBy = "Modified By: " + currentFactoid?.modified_by;
            modifiedWhen = "Modified When: " + currentFactoid?.modified_when.ToString();

            statementCount = "Has Statements: " + currentFactoid.has_statements.Count.ToString();


            if(currentFactoid.has_statements.Count != 0)
            {
                statementObjectType = "Object Type " + currentFactoid.has_statements[statementIndex].__object_type__;
                statementID = "ID: " + currentFactoid.has_statements[statementIndex].id.ToString();
                statementName = "Name: " + currentFactoid.has_statements[statementIndex].name;
                statementStartDate = "Start Date: " + currentFactoid.has_statements[statementIndex].start_date_written;
                statementEndDate = "End Date: " + currentFactoid.has_statements[statementIndex].end_date_written;
                statementNotes = "Notes: " + currentFactoid.has_statements[statementIndex].notes;
                statementInternalNotes = "Internal Notes: " + currentFactoid.has_statements[statementIndex].internal_notes;
                statementHeadStatement = "Head Statement: " + currentFactoid.has_statements[statementIndex].head_statement.ToString();

            } else
            {
                statementObjectType = "Object Type ";
                statementName = "Name: ";
                statementStartDate = "Start Date: ";
                statementEndDate = "End Date: ";
                statementNotes = "Notes: ";
                statementInternalNotes = "Internal Notes: ";
                statementHeadStatement = "Head Statement: ";
            }
        }

    }

    private void cboUpdateJsonIndex()
    {
        jsonObjectIndex = cboSelect;
        UpdateJsonDisplay();
    }


    public MainViewModel() {

        parseCommand = ReactiveCommand.Create(() => {getJson();});

        nextEntry = ReactiveCommand.Create(() => {selectNextEntry();});

        previousEntry = ReactiveCommand.Create(() => { selectPreviousEntry();});

        nextStatement = ReactiveCommand.Create(() => { selectNextStatement();});

        previousStatement = ReactiveCommand.Create(() => { selectPreviousStatement();});    

        jsonIndexChange = ReactiveCommand.Create(() => {  updateJsonIndex(); });

        lookupFilterCmd = ReactiveCommand.Create(() => { LookupFilter(); }); 
   
    }


}
