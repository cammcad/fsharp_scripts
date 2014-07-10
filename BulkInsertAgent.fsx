#r @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v3.5\Desktop\System.Data.SqlServerCe.dll"
#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Transactions.dll"
#r "System.Xml.Linq"
#r "C:\Project Dragnet\svn\ITO\Libraries\DwfDataSupporter.dll"


open System
open System.Text
open System.Collections.Generic
open System.Data.SqlServerCe
open System.Transactions
open DwfDataSupporter.implementation
open System.Xml.Linq


///Alias for Mailbox processor - Agent is a friendly (more widely adopted) name.
type Agent<'T> = MailboxProcessor<'T>

(* Helper let bindings and functions *)
///sql mobile db 
let db = @"C:\data\Gilead\ITO.sdf"
let cn = new SqlCeConnection(sprintf "Data Source=%s" db)
/// Helper for quickly grabbing the identity 
let Get_identitystatement (cn: SqlCeConnection) = 
    let statement = "SELECT @@IDENTITY"
    let cmd = new SqlCeCommand(statement, cn)
    cmd 
// Helper for converting an object to int32
let to_int (obj: Object) = Convert.ToInt32(obj)
// Helper for opening a sql mobile connection
let open_connection (cn: SqlCeConnection) = 
    if not(cn.State = Data.ConnectionState.Open) then 
        cn.Open()
    cn



/// dwf * revision * markfordeletion * fkobjectid
let insert_objectproperties (cn: SqlCeConnection) (d: DwfDataSupporter.Entities.IDWFObject * int * int * int) =
    let insertstatement = 
        "INSERT INTO Properties (FKObjectId, PropertyName, PropertyValue, PropertyCategory,
                                       MarkForDeletion)
         VALUES (@fkobjectid, @propertyname, @propertyvalue, @propertycategory, @markfordeletion)" 
    let command = new SqlCeCommand(insertstatement, open_connection cn)
    let (dwfObject, revision, markedfordeletion, fkobjectid) = d
    for d in dwfObject.Properties do
        command.Parameters.Clear()
        command.Parameters.Add(new SqlCeParameter("@fkobjectid" , fkobjectid )) |> ignore
        command.Parameters.Add(new SqlCeParameter("@propertyname", d.Name)) |> ignore
        command.Parameters.Add(new SqlCeParameter("@propertyvalue", d.Value)) |> ignore
        command.Parameters.Add(new SqlCeParameter("@propertycategory", d.Category)) |> ignore
        command.Parameters.Add(new SqlCeParameter("@markfordeletion", markedfordeletion)) |> ignore
        command.ExecuteNonQuery() |> ignore
(* Bulk insert Dwf Objects *)
let BulkInsert (conn: SqlCeConnection) revision (commands: seq<SqlCeCommand * DwfDataSupporter.Entities.IDWFObject>) = 
    try
        //execute commands
        Seq.iter(fun (cmd: SqlCeCommand, dwf: DwfDataSupporter.Entities.IDWFObject ) 
                        -> 
                            cmd.ExecuteNonQuery() |> ignore
                            let identity = (Get_identitystatement (open_connection cn)).ExecuteScalar() |> to_int
                            insert_objectproperties (open_connection cn) (dwf,revision,0, identity)
                           ) commands
    finally
        cn.Close()

let dwfobject_to_sqlcecommand (cn: SqlCeConnection) (d: DwfDataSupporter.Entities.IDWFObject * int * int) =
    let insertstatement = "INSERT INTO Objects (ObjectId, ObjectLabel, FKRevisionId, MarkForDeletion)
             VALUES (@objectid, @label, @revision, @marked)" 
    let command = new SqlCeCommand(insertstatement, open_connection cn)
    let (dwfObject, revision, markedfordeletion)  = d
    command.Parameters.Add(new SqlCeParameter("@objectid", dwfObject.Id )) |> ignore
    command.Parameters.Add(new SqlCeParameter("@label", dwfObject.Label )) |> ignore
    command.Parameters.Add(new SqlCeParameter("@revision", revision)) |> ignore
    command.Parameters.Add(new SqlCeParameter("@marked", markedfordeletion)) |> ignore
    command,dwfObject
// (dwfObject, revisionid, markfordeletion)
let objects_to_commands xml revid = 
    let provider = new QueryDWFObjectData()
    let data = 
        seq {
                for o in provider.RetrieveAllObjectsInDWFFile xml do
                yield o
            } 
    data |> Seq.filter(fun o -> o.Label.Contains("[")) |> Seq.map(fun o -> dwfobject_to_sqlcecommand cn (o, revid, 0))

// remove all takeoff items and related takeoff audit items
let bulkremove_takeoffitems (cn: SqlCeConnection) (key: int) = 
    let takeoffauditstatement = "DELETE FROM TakeOffAudit WHERE TakeOffItemId in (
                                 SELECT TakeOffItemId FROM TakeOffSheet WHERE FKRevisionId = @revisionid )"
    let takeoffstatement = "DELETE FROM TakeOffSheet WHERE FKRevisionId = @revisionid"
    let cmdaudit = new SqlCeCommand(takeoffauditstatement, open_connection cn)
    let cmd = new SqlCeCommand(takeoffstatement, open_connection cn)
    cmdaudit.Parameters.Add(new SqlCeParameter(@"revisionid",key)) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@revisionid",key)) |> ignore
    cmdaudit.ExecuteNonQuery() |> ignore
    cmd.ExecuteNonQuery() |> ignore

let remove_objects_properties (cn: SqlCeConnection) (key: int) (takeoffFunc: SqlCeConnection -> int -> unit)  =
    takeoffFunc cn key (* Dependency Function / referential integrity *) 
    let removePropertiesStatement = 
        "DELETE FROM Properties WHERE FKObjectId 
                    in ( SELECT Id FROM Objects WHERE FKRevisionId = @revisionid )"
    let cmdRemoveProperties = new SqlCeCommand(removePropertiesStatement, open_connection cn)
    cmdRemoveProperties.Parameters.Add("@revisionid",key) |> ignore
    let removeObjectsStatement = "DELETE FROM Objects WHERE FKRevisionId = @revisionid"
    let cmdRemoveObjects = new SqlCeCommand(removeObjectsStatement, open_connection cn)
    cmdRemoveObjects.Parameters.Add("@revisionid",key) |> ignore
    cmdRemoveProperties.ExecuteNonQuery() |> ignore
    cmdRemoveObjects.ExecuteNonQuery() |> ignore

let does_temp_project_revision_exists (cn: SqlCeConnection) = 
    let projectStatement = "SELECT ProjectId FROM Project WHERE ProjectName like @projectname"
    let cmdproject = new SqlCeCommand(projectStatement, open_connection cn)
    cmdproject.Parameters.Add("@projectname","tempProject") |> ignore
    let revisionStatement = "SELECT RevisionId FROM Revisions WHERE RevisionName like @revname"
    let cmdrevision = new SqlCeCommand(revisionStatement, open_connection cn)
    cmdrevision.Parameters.Add("@revname","tempRevision") |> ignore
    let prj = cmdproject.ExecuteScalar() 
    let rev = cmdrevision.ExecuteScalar() 
    if not(prj = null) && not(rev = null) then 
        let prjid = prj |> to_int
        let revid = rev |> to_int
        true,prjid,revid
    else
        false,0,0

let create_temp_project_revision (cn: SqlCeConnection) = 
    let result,prjid,revid = does_temp_project_revision_exists cn
    match result with
    | true -> prjid,revid
    | false ->
        (* Project *)
        let projectStatement = "INSERT INTO Project (ProjectName) VALUES (@projectname)"
        let cmdProject = new SqlCeCommand(projectStatement, open_connection cn)
        cmdProject.Parameters.Add("@projectname", "tempProject") |> ignore
        cmdProject.ExecuteNonQuery() |> ignore
        let prjidentity = (Get_identitystatement (open_connection cn)).ExecuteScalar() |> to_int
        (* Revision *)
        let revisionStatement = "INSERT INTO Revisions (FKProjectId, RevisionName) VALUES (@fkprojectid, @revname)"
        let cmdRevision = new SqlCeCommand(revisionStatement, open_connection cn)
        cmdRevision.Parameters.Add("@fkprojectid",prjidentity) |> ignore
        cmdRevision.Parameters.Add("@revname", "tempRevision") |> ignore
        cmdRevision.ExecuteNonQuery() |> ignore
        let revIdentity = (Get_identitystatement (open_connection cn)).ExecuteScalar() |> to_int
        prjidentity, revIdentity


let update_temp_project_revision info  = 
    let cn, (prjname: string), (revname: string), (prjid: int), (revid: int) = info
    let projectStatement = "UPDATE Project SET ProjectName = @projectname WHERE ProjectId = @prjid"
    let cmdProject = new SqlCeCommand(projectStatement,open_connection cn)
    cmdProject.Parameters.Add("@projectname",prjname) |> ignore
    cmdProject.Parameters.Add("@prjid",prjid) |> ignore
    cmdProject.ExecuteNonQuery() |> ignore
    let revisionStatement = "UPDATE Revisions SET RevisionName = @revname WHERE RevisionId = @revid"
    let cmdRevision = new SqlCeCommand(revisionStatement, open_connection cn)
    cmdRevision.Parameters.Add("@revname",revname) |> ignore
    cmdRevision.Parameters.Add("@revid",revid) |> ignore
    cmdRevision.ExecuteNonQuery() |> ignore

type msg = Insert of XContainer | Update of string * string | Remove | Stop
exception StopException
let DwfBulkInsertAgent = Agent<msg>.Start(fun m -> async {
                            let tempPlacement = new Dictionary<string,int>()
                            while true do
                               let! msg = m.Receive()
                               match msg with
                               | Insert xml -> 
                                        if tempPlacement.Keys.Count >=2 then
                                            let revisionid = tempPlacement.Item "revision"
                                            objects_to_commands xml revisionid |> BulkInsert cn revisionid
                                        else
                                            let prj,rev = create_temp_project_revision cn
                                            tempPlacement.Add("project",prj)
                                            tempPlacement.Add("revision", rev)
                                            objects_to_commands xml rev |> BulkInsert cn rev
                               | Update (project, revision) -> 
                                        update_temp_project_revision (cn,project,revision, (tempPlacement.Item "project"), (tempPlacement.Item "revision"))
                                        tempPlacement.Clear()
                               | Remove -> remove_objects_properties (cn) (tempPlacement.Item "revision") bulkremove_takeoffitems
                               | Stop -> raise(StopException)
                        })

(* inital xml to hand off to the agent (myhouse) *)
let xml = @"C:\Project Dragnet\svn\ITO\Winestimator.ITO\bin\Debug\447875BD-7E50-49CA-84FE-5C3280693731.content.xml"
         |> XDocument.Load
(* new xml to hand off to the agent (Gilead) *)
let gilead = @"C:\Users\cfrederick\Documents\Gilead Concrete Structure 2008 07 09x-0002_phase3\55E3B16A-F832-4600-9E2B-BFEC4F489491.content.xml" 
             |> XDocument.Load

//cn.Open()
//Bulk Add My House Objects and Properties to ITO Mobile database
DwfBulkInsertAgent.Post(Insert(xml))
//Bulk Add Gilead Objects and Properties to ITO Mobile database
DwfBulkInsertAgent.Post(Insert(gilead))
//Bulk Remove Objects and Properties from ITO Mobile database for temp project and revision
DwfBulkInsertAgent.Post(Remove)
//Update temp project and revision (User has invoked save as new project)
DwfBulkInsertAgent.Post(Update("Project3","Yo"))
//Choke point on the agent (really enables messages to be ignored / not handled by the agent)
DwfBulkInsertAgent.Post(Stop)




let ids = ["e7040626-cf8b-4cd3-abb3-a2bac195a003";"990deca9-4dd2-4895-bea8-41b71879816b"; "ec580305-be1d-40a4-8b7d-c228f16eb14d"];
let args ids =
    let builder = new StringBuilder()
    List.iter(fun id -> builder.Append(sprintf "\'%s\'%s" id ",") |> ignore) ids
    let s = builder.ToString()
    s.Remove(s.Length - 1)


let real_args = args ids    
let statement = "select ObjectId from Objects where Id in ( select fkobjectid from TakeOffAudit where FKTakeOffItemId in ( select TakeOffItemId from TakeOffSheet where TakeOffIdentity in ( " + real_args + ")))"
let cmd = new SqlCeCommand(statement, open_connection(new SqlCeConnection(@"DataSource = C:\Users\cfrederick\AppData\Local\Temp\ITO.sdf") ))
let reader = cmd.ExecuteReader()
while reader.Read() do
    let v = reader.GetString(0)
    printf "id: %s \n" v 