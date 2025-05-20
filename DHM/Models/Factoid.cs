using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;

namespace DHM.Models
{
    public class Factoid
    {
        public int? id { get; set; }
        public Review? review { get; set; }
        public string? name { get; set; }

        public Source? source { get; set; }

        public List<HasStatements>? has_statements { get; set; }

        public string? created_by { get; set; }
        public DateTime? created_when { get; set; }
        public string? modified_by { get; set; }
        public DateTime? modified_when { get; set; }

    }

    public class HasStatements
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? name { set; get; }
        public string? start_date_written { get; set; }
        public string? end_date_written { get; set; }
        public string? notes { get; set; }
        public string? internal_notes { get; set; }
        public bool head_statement { get; set; }

        public Certainty? certainty { get; set; }

        public CertaintyValues? certainty_values { get; set; }

        [JsonPropertyName("Handlung_ausgeführt_von")]
        public List<HandlungAusgefuehrtVon>? Handlung_ausgeführt_von { get; set; }

        [JsonPropertyName("Ort_der_Handlung")]
        public List<OrtDerHandlung>? Ort_der_Handlung { get; set; }

        [JsonPropertyName("ausgeführte_Tätigkeit")]
        public List<AusgefuehrteTaetigkeit>? ausgeführte_Tätigkeit { get; set; }

        [JsonPropertyName("Ausführende")]
        public List<Ausfuehrende>? Ausführende { get; set; }

        public List<Produkt>? Produkt { get; set; }
        public List<GenanntePerson>? genannte_Person { get; set; }

        public List<Statement>? Übertragenes_Objekt { get; set; }
        public List<Statement>? vorbesitzer { get; set; }
        public List<Statement>? empfänger { get; set; }


        public List<Person>? person { get; set; }

        public string? gender { get; set; }
        public string? forename { get; set; }
        public string? surname { get; set; }
        public string? role_name { get; set; }
        public string? add_name { get; set; }

    }


    public class GenanntePerson
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class Person
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class Statement
    {
        [JsonPropertyName("__object_type__")]
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class CertaintyValues
    {
        public Certainty? certainty { get; set; }
        public AusfuehrendeCertainty? Ausführende { get; set; }
        public OrtDerHandlungCertainty? Ort_der_Handlung { get; set; }
        public CertaintyValuesEntry? certainty_values { get; set; }
        public EndDateWritten? end_date_written { get; set; }
        public StartDateWritten? start_date_written { get; set; }
        public AusgefuehrteTaetigkeitCertainty? ausgeführte_Tätigkeit { get; set; }
        public HandlungAusgefuehrtVonCertainty? Handlung_ausgeführt_von { get; set; }
        public PersonCertainty? person { get; set; }
        public GenanntePersonCertainty? genannte_Person { get; set; }

        public ProduktCertainty? Produkt { get; set; }

    }

    public class Certainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class AusfuehrendeCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class OrtDerHandlungCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class CertaintyValuesEntry
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class EndDateWritten
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class StartDateWritten
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class AusgefuehrteTaetigkeitCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class HandlungAusgefuehrtVonCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class ProduktCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class PersonCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class GenanntePersonCertainty
    {
        public string? notes { set; get; }
        public int? certainty { get; set; }
    }

    public class Produkt
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class HandlungAusgefuehrtVon
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class OrtDerHandlung
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class AusgefuehrteTaetigkeit
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class Ausfuehrende
    {
        public string? __object_type__ { get; set; }
        public int? id { get; set; }
        public string? label { get; set; }
    }

    public class Review
    {
        public bool? reviewed { get; set; }
        public string? reviewed_by { get; set; }
        public string? review_notes { get; set; }
        public bool? problem_flagged { get; set; }

    }

    public class Source
    {
        public string? id { get; set; }
        public string? text { get; set; }
        public object? pages_start { get; set; }
        public object? pages_end { get; set; }
        public string? folio { get; set; }
    }



}