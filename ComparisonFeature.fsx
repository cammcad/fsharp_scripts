#r @"C:\Project Dragnet\ADR2010Libraries\DwfDataSupporter\bin\Debug\DwfDataSupporter.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.Xml.Linq.dll"

(* SqlCe references *)
#r @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v3.5\Desktop\System.Data.SqlServerCe.dll"
#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Transactions.dll"
#r @"C:\Program Files (x86)\FSharpPowerPack-1.9.9.9\bin\FSharp.PowerPack.Parallel.Seq.dll"


(* Depends *)
open System
open System.Collections.Generic
open DwfDataSupporter.Entities
open DwfDataSupporter.implementation
open System.Xml.Linq
open System.Data.SqlServerCe
open System.Transactions
open Microsoft.FSharp.Collections


(* Helper let bindings and functions *)
    module SqlMobileHelper = 
        type CommandExecutionType = Single | All | None
        type CommandResultType = Scalar of Object | ResultSet of SqlCeDataReader | Action of unit
         
        ///sql mobile db 
        let db = ref ""
        (* Helper function for SqlCe data reader *)
        let readval (reader: SqlCeDataReader) (position: int): string = reader.GetString(position)
        /// Helper for quickly grabbing the identity 
        let Get_identitystatement (cn: SqlCeConnection) = 
            let statement = "SELECT @@IDENTITY"
            let cmd = new SqlCeCommand(statement, cn)
            cmd 
        /// Helper for converting an object to int32
        let to_int (obj: Object) = Convert.ToInt32(obj)
        /// Helper for opening a sql mobile connection
        let open_connection (cn: SqlCeConnection) = 
            if not(cn.State = Data.ConnectionState.Open) then 
                cn.Open()
            cn
        /// Helper for creating a sqlce connection
        let fetch_connection c = 
               db := c
               new SqlCeConnection(sprintf "Data Source=%s" !db)
        let cn = fetch_connection @"C:\Saved ITO Projects\demo1\ITO.sdf"
        /// apply query parameters
        let args lst (cmd: SqlCeCommand) = 
            List.iter(fun (k,v: 'a) -> 
                        cmd.Parameters.Add(new SqlCeParameter(k, v)) |> ignore ) lst
            cmd
        /// query
        let query q cn = new SqlCeCommand(q, open_connection cn)
        
        /// exectute sqlcecommand
        let execute (cet: CommandExecutionType) (cmd: SqlCeCommand) : CommandResultType = 
            match cet with
            | Single -> 
                Scalar(cmd.ExecuteScalar())
            | All -> 
                ResultSet(cmd.ExecuteReader())
            | None ->
                Action(cmd.ExecuteNonQuery() |> ignore)
(*============= Open Helper Module ===============*)
open SqlMobileHelper

(* M = modified  D = deleted *)
type takeoffConflictType = M | D
(* Records *)
type comparison = {original: IDWFObject seq; revised: IDWFObject seq; comparisonType: IEqualityComparer<IDWFObject>; msg: string; }
type takeoffconflict = { ID: int; Description: string; Count: string;
                         Area: string; Length: string; Volume: string; conflictType: takeoffConflictType;
                         Notes: string; relatedObjects: System.Collections.Generic.List<IDWFObject> ; conflictObjects: System.Collections.Generic.List<IDWFObject>; }
type takeoffitem = {ID: string; Description: string; Count: string; Area: string; Length: string; Volume: string; Notes: string;}

//type takeoffresults = { takeoffitem: takeoffitem; modifified_deleted_objects: string seq; related_objects: IDWFObject seq;}
type takeoffresults = { takeoffitem: takeoffitem; changed_relatedObjects: Dictionary<string,seq<IDWFObject>>;}

let produce_xml (s: string) = s |> XDocument.Load
let original_revision = produce_xml @"C:\Project Dragnet\data\Gilead Comparison Feature\extracted\Gilead Concrete Structure_a\D695692D-E2E8-42BD-A319-98E5A6C57AB5.content.xml"
let updated_revision = produce_xml @"C:\Project Dragnet\data\Gilead Comparison Feature\extracted\Gilead Concrete Structure_b\3C51685A-5ED0-4E5C-B877-722F0B393BD3.content.xml"

let dwfqueryengine = new QueryDWFObjectData()
(* Helper to produce a list of IDWFObjects from xml revision data *)
let to_IDWF_list = dwfqueryengine.RetrieveAllObjectsInDWFFile
(* Revision we're comparing against *)
let original_objects = to_IDWF_list original_revision
(* Revision that is most up to date (and currently open) *)
let updated_objects = to_IDWF_list updated_revision

let diff (c: comparison) = 
    match c.msg with
    | "Added" -> "Added" , System.Linq.Enumerable.Except(c.revised,c.original, c.comparisonType)
    | "Modified" -> "Modified", System.Linq.Enumerable.Except(c.original,c.revised, c.comparisonType)
    | "Deleted" -> "Deleted", System.Linq.Enumerable.Except(c.original,c.revised, c.comparisonType)
    | _ -> failwith "Unknown msg type"


(* Async functions for Added, Modified, Deleted *)
let added_modified_deleted = 
      seq {
            yield async { return diff {original = original_objects; revised = updated_objects; comparisonType = (new DWFObjectEqualityComparer()); msg = "Added";} } 
            yield async {return diff {original = original_objects; revised = updated_objects; comparisonType =  (new DWFObjectPropertiesEqualityComparer()); msg = "Modified";} }
            yield async {return diff {original = original_objects; revised = updated_objects; comparisonType =  (new DWFObjectEqualityComparer()); msg = "Deleted"; } }
          }

(* Run sequence of async functions in parallel and return results as Dictionary *)
let all = Async.RunSynchronously(Async.Parallel added_modified_deleted ) 
          |> fun all -> dict [ for p in all do yield p ]

(* Collect the modified and deleted IDWFObject sequences  as IDWFObject seq for both modified and deleted *)
let fetch_labels_from_objects = Seq.map(fun (x: IDWFObject) -> x.Label)
let modifiedObjectLabels = fetch_labels_from_objects (all.Item "Modified")
let deletedObjectLabels = fetch_labels_from_objects (all.Item "Deleted")

(* Computation that converts a data reader of takeoff item ids to a seq<string> *)
let fetch_takeoffitemids (key: string) (results: CommandResultType) = 
        match results with
        | ResultSet(reader) ->
                let takeoffitemids = 
                    seq {  while reader.Read() do
                                yield Convert.ToString(reader.GetInt32(0)); }
                (key, takeoffitemids) 
        | _ ->  (key, Seq.empty)
(* Query database for all TakeOff items which have the given Object Id as one of the objects
   which make up the TakeOff item *)
let lookup_dwf_id_and_related_takeoffitems = 
        fun (x: string) ->
            open_connection cn
            |> query "SELECT Id FROM Objects WHERE ObjectLabel like @objlabel"
            |> args [("@objlabel",x)]
            |> execute Single
            |> fun r -> 
                match r with
                | Scalar(v) -> (x, v |> to_int)
                | _ -> failwith "invalid argument recieved from lookup_dwf_id_and_related_takeoffitems"  
        >>
        fun (x: string, id: int) ->
            open_connection cn
            |> query "SELECT TakeOffItemId, Description, Count, Area, Length, Volume, Notes
                      FROM TakeOffSheet WHERE TakeOffItemId in 
                      ( SELECT FKTakeOffItemId FROM TakeOffAudit WHERE FKObjectId = @id )"
            |> args [("@id",id)]
            |> execute All
            |> fetch_takeoffitemids x
(* Modified object and takeoff items & deleted object and takeoff items *)
let M_object_and_takeoffitems = Seq.map(lookup_dwf_id_and_related_takeoffitems) modifiedObjectLabels
let D_object_and_takeoffitems = Seq.map(lookup_dwf_id_and_related_takeoffitems) deletedObjectLabels

(* Simple call to the database to retrieve related objects for any given takeoff item *)
let lookup_takeoffitemid_related_objects (itemid: string) =
    let k = Convert.ToInt32(itemid) 
    open_connection cn
    |> query "SELECT ObjectId, ObjectLabel FROM Objects WHERE Id in
                    (SELECT FKObjectId FROM TakeOffAudit WHERE FKTakeOffItemId in 
                    (SELECT TakeOffItemId FROM TakeOffSheet WHERE TakeOffItemId = @takeoffid))"
    |> args [("@takeoffid",k)]
    |> execute All
    |> fun crt -> 
        match crt with
        | ResultSet(reader) ->
             seq { // yield a tuple of objects -> (objectId, objectlabel)
                            while reader.Read() do
                                yield   (readval reader 0, readval reader 1) }
        | _ -> failwith "invalid argument recieved from lookup_takeoffitem_related_objects"
    |>
    fun (objectid_objectlabel) -> 
            (* Fetch Properties For Objects and produce final takeoff results *)
            Seq.map(fun ((id: string),(label: string)) -> 
                        let query = "SELECT PropertyName, PropertyValue, PropertyCategory FROM Properties
                                        WHERE FKobjectId in ( SELECT Id FROM Objects WHERE ObjectLabel like @objectlabel )"
                        let cmd = new SqlCeCommand(query, open_connection cn)
                        cmd.Parameters.Add(new SqlCeParameter("@objectlabel",label)) |> ignore
                        let reader = cmd.ExecuteReader()
                        let properties =
                                    [
                                        while reader.Read() do
                                            let name = readval reader 0
                                            let value = readval reader 1
                                            let category = readval reader 2
                                            yield new DwfDataSupporter.Entities.DWFObjectProperty(Name=name,Value=value,Category=category) :> IDWFObjectProperty
                                    ]  |> (fun props -> new System.Collections.Generic.List<IDWFObjectProperty>(props))
                        (new DwfDataSupporter.Entities.DWFObject(Id=id, Label=label.ToString(), Properties=properties) :> IDWFObject)) objectid_objectlabel

let get_takeoffconflict_info id conflctType = 
    open_connection cn
    |> query "SELECT TakeOffItemId, Description, Count, Area, Length, Volume, Notes
                FROM TakeOffSheet WHERE TakeOffItemId = @id"
    |> args [("@id",id |> to_int)]
    |> execute All
    |> fun crt -> 
       match crt with 
       | ResultSet(reader) ->
              seq {
                     while reader.Read() do
                            yield { ID = reader.GetInt32(0);
                                    Description = readval reader 1;
                                    Count = readval reader 2;
                                    Area = readval reader 3;
                                    Length = readval reader 4;
                                    Volume = readval reader 5;
                                    Notes = readval reader 6;
                                    conflictType = if conflctType = "modified" then M else D
                                    relatedObjects = new System.Collections.Generic.List<IDWFObject>()
                                    conflictObjects = new System.Collections.Generic.List<IDWFObject>() } } |> Seq.head
       | _ -> failwith "invalid argument to get_takeoffconflict_info"
            
(* Construct function to create IDWFObjects from the data in the database *)
let construct_IDWFObjects_from_db (kvp: KeyValuePair<string,System.Collections.Generic.List<string>>) (conflict_type: string)  = 
     let items = kvp.Value
     let key = kvp.Key
     let c = open_connection cn
     let conflict_info = get_takeoffconflict_info key conflict_type
     let conflictobjs = 
              Seq.map(fun label ->
                                c
                                |> query "SELECT obj.ObjectId, PropertyName, PropertyValue, PropertyCategory FROM Properties prop
                                          LEFT JOIN Objects obj ON prop.FKObjectId = obj.Id
                                          WHERE FKobjectId in ( SELECT Id FROM Objects WHERE ObjectLabel like @objectlabel )"    
                                |> args [("@objectlabel",label)]
                                |> execute All
                                |> fun crt ->
                                        match crt with
                                        | ResultSet(reader) -> 
                                            let properties =
                                                    seq {
                                                        while reader.Read() do
                                                            let id = readval reader 0
                                                            let name = readval reader 1
                                                            let value = readval reader 2
                                                            let category = readval reader 3
                                                            yield id, new DwfDataSupporter.Entities.DWFObjectProperty(Name=name,Value=value,Category=category) :> IDWFObjectProperty
                                                    }  |> (fun seqvals -> 
                                                                let props = Seq.map(fun (id,prop) -> prop) seqvals
                                                                let id = Seq.head seqvals |> (fun (id,prop) -> id)
                                                                id,new System.Collections.Generic.List<IDWFObjectProperty>(props))
                                            let id,props = properties
                                            (new DwfDataSupporter.Entities.DWFObject(Id=id, Label=label, Properties=props) :> IDWFObject)
                                        | _ -> failwith "invalid argument recieved from Construct_IDWFObjects_from_db_Async" ) items
     let relatedobjs = lookup_takeoffitemid_related_objects key
     { conflict_info with conflictObjects = new System.Collections.Generic.List<IDWFObject>(conflictobjs)
                          relatedObjects = new System.Collections.Generic.List<IDWFObject>(relatedobjs) }
            
(* Construct a dictionary of takeoffitem id & object label list *)
let fetch_dictionary_takeoffitems_objectlabels (items: seq<string * seq<string>>) = 
    let accumulator = new Dictionary<string,System.Collections.Generic.List<string>>()
    Seq.fold(fun (acc: Dictionary<string,System.Collections.Generic.List<string>>) (elem: string * seq<string>) ->  
                let label, items = elem
                Seq.iter(fun ti -> 
                            if acc.ContainsKey ti then
                                let labels = acc.[ti]
                                if not <| labels.Contains(label) then
                                    labels.Add(label)
                                    acc.[ti] <- labels                                    
                            else
                               let lst = new System.Collections.Generic.List<string>()
                               lst.Add(label)
                               acc.Add(ti, lst)   ) items
                acc ) accumulator items



(* Transform object labels and conflict type into takeoff conflict items *)
(* ===== e.g. modified_object_labels & "modified"   ||  deleted_object_labels & "deleted" ======
   ===== returns -> takeoffitem conflict seq ======= *)
let fetch_takeoff_conflicts (objlabels: seq<string * seq<string>>) (conflict_type: string) = 
    fetch_dictionary_takeoffitems_objectlabels objlabels
    |> fun dict -> seq {for kvp: KeyValuePair<string,System.Collections.Generic.List<string>> in dict do
                             yield construct_IDWFObjects_from_db kvp conflict_type }//construct_sequence_of_async
