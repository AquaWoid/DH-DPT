using DHM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DHM.Utilities
{
    public static class OptionFileCreator
    {

        private static TableFilter fName = new TableFilter();
        private static TableFilter id = new TableFilter();
        private static TableFilter created_by = new TableFilter();
        private static TableFilter created_when = new TableFilter();
        private static TableFilter modified_by = new TableFilter();
        private static TableFilter modified_when = new TableFilter();
        private static TableFilter statements0id = new TableFilter();
        private static TableFilter statements0objectType = new TableFilter();
        private static TableFilter statements0startDate = new TableFilter();
        private static TableFilter handlung = new TableFilter();
        private static TableFilter ortderhandlung = new TableFilter();
        private static ObservableCollection<TableFilter> tableFilters = new ObservableCollection<TableFilter>();

        public static ObservableCollection<TableFilter> GetAvailableFilters()
        {

            fName.name = "name";
            fName.enabled = true;   

            id.name = "id";
            id.enabled = true;

            created_by.name = "created_by";
            created_by.enabled = true;

            created_when.name = "created_when";
            created_when.enabled = true;

            modified_by.name = "modified_by";
            modified_by.enabled = true;

            modified_when.name = "modified_when";
            modified_when.enabled = true;   

            statements0id.name = "statements[0].id";
            statements0id.enabled = true;

            statements0objectType.name = "statements[0].__object_type__";
            statements0objectType.enabled = true;

            statements0startDate.name = "statements[0].start_date_written";
            statements0startDate.enabled = true;

            handlung.name = "Handlung_ausgeführt_von";
            handlung.enabled = true;

            ortderhandlung.name = "Ort_der_Handlung";
            ortderhandlung.enabled = true;

            tableFilters.Add(id);
            tableFilters.Add(fName);
            tableFilters.Add(created_by);
            tableFilters.Add(created_when);
            tableFilters.Add(modified_by);
            tableFilters.Add(modified_when);
            tableFilters.Add(statements0id);
            tableFilters.Add(statements0objectType);
            tableFilters.Add(statements0startDate);
            tableFilters.Add(handlung);
            tableFilters.Add(ortderhandlung);

            return tableFilters;
   
        }
    }
}
