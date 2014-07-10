#r @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v3.5\Desktop\System.Data.SqlServerCe.dll"
#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Transactions.dll"

open System
open System.Collections.Generic
open System.Data.SqlServerCe
open System.Transactions


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

///Alias for Mailbox processor - Agent is a friendly (more widely adopted) name.
type Agent<'T> = MailboxProcessor<'T>

///Important Notes: The Application will need to generate a guid (identity for each takeoff item)
/// Also need to change the add takeoff item function / update item function to use Id from takeoff item dictionary
let id = "1ee7b76c-3721-49da-9f6d-444c996122bd"
let item = new Dictionary<string,string>()
item.Add("Description", "Walls")
item.Add("Count", "300")
item.Add("Area", "220")
item.Add("Length", "212\'- 0\"")
item.Add("Volume","110")
item.Add("Notes","This is a cool takeoff item")
item.Add ("RevisionId", "3")
item.Add("Id", id)

let modifieditem = new Dictionary<string, string>()
modifieditem.Add("Description", "Precast Walls")
modifieditem.Add("Count", "300")
modifieditem.Add("Area", "220")
modifieditem.Add("Length", "212\'- 0\"")
modifieditem.Add("Volume","110")
modifieditem.Add("Notes","This is a cool takeoff item")
modifieditem.Add ("RevisionId", "3")
modifieditem.Add("Id", "1ee7b76c-3721-49da-9f6d-444c996122bd")


let relatedObjectIds = 
    seq {
            yield "Fixed [155091]"
            yield "Fixed [155220]"
            yield "Fixed [155257]"
            yield "Fixed [183760]"
        }

(* Add TakeOff Item*)
let addTakeOffItem_and_yield_identity (takeoffitem: Dictionary<string,string>) = 
    let insertstatement = "INSERT INTO TakeOffSheet (Description, Count, Area, Length, Volume, Notes, FKRevisionId,    
                                                     TakeOffIdentity)
                           VALUES (@Description, @Count, @Area, @Length, @Volume, @Notes, @FKRevisionId,
                                   @TakeOffIdentity)"

    let cmd = new SqlCeCommand(insertstatement, open_connection cn)
    cmd.Parameters.Add(new SqlCeParameter("@Description",takeoffitem.Item "Description")) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@Count",takeoffitem.Item "Count")) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@Area",takeoffitem.Item "Area")) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@Length",takeoffitem.Item "Length")) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@Volume",takeoffitem.Item "Volume")) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@Notes",takeoffitem.Item "Notes")) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@FKRevisionId", takeoffitem.Item "RevisionId" |> to_int)) |> ignore
    cmd.Parameters.Add(new SqlCeParameter("@TakeOffIdentity",takeoffitem.Item "Id")) |> ignore
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    let identity = (Get_identitystatement (open_connection cn)).ExecuteScalar() :?> decimal |> to_int
    identity
(* Add TakeOff Audit *)
let addRelatedObjects_to_TakeoffAudit (objids: seq<string>) (takeoffitemIdentity: int) = 
    if not <| Seq.isEmpty objids then
        let getobjectidStatement = "SELECT Id FROM Objects WHERE ObjectLabel like @objlabel"
        let insertTakeOffAudit = "INSERT INTO TakeOffAudit (FKTakeOffItemId, FKObjectId) VALUES (@FKTakeOffItemId, @FKObjectId)"
        let ids = Seq.map(fun (id: string) -> 
                          let cmd = new SqlCeCommand(getobjectidStatement, open_connection cn)
                          cmd.Parameters.Add(new SqlCeParameter("@objlabel",id)) |> ignore
                          cmd.ExecuteScalar() |> to_int) objids
        Seq.iter(fun (id: int) -> 
                          let cmd = new SqlCeCommand(insertTakeOffAudit, open_connection cn)
                          cmd.Parameters.Add(new SqlCeParameter("@FKTakeOffItemId", takeoffitemIdentity)) |> ignore
                          cmd.Parameters.Add(new SqlCeParameter("@FKObjectId", id)) |> ignore
                          cmd.ExecuteNonQuery() |> ignore ) ids


let add (takeoffitem: Dictionary<string,string>, robjs) = 
    addTakeOffItem_and_yield_identity item |> addRelatedObjects_to_TakeoffAudit robjs    
   

let update (takeoffitem: Dictionary<string, string>) = 
    if takeoffitem.Keys.Count > 0 then
        let updatestatement = "UPDATE TakeOffSheet SET Description = @Description, Count = @Count, Area = @Area,
                               Length = @Length, Volume = @Volume, Notes = @Notes WHERE TakeOffIdentity = @takeoffidentity"
        let cmdUpdate = new SqlCeCommand(updatestatement, open_connection cn)
        cmdUpdate.Parameters.Add(new SqlCeParameter("@Description", takeoffitem.Item "Description")) |> ignore
        cmdUpdate.Parameters.Add(new SqlCeParameter("@Count", takeoffitem.Item "Count")) |> ignore
        cmdUpdate.Parameters.Add(new SqlCeParameter("@Area", takeoffitem.Item "Area")) |> ignore
        cmdUpdate.Parameters.Add(new SqlCeParameter("@Length", takeoffitem.Item "Length")) |> ignore
        cmdUpdate.Parameters.Add(new SqlCeParameter("@Volume", takeoffitem.Item "Volume")) |> ignore
        cmdUpdate.Parameters.Add(new SqlCeParameter("@Notes", takeoffitem.Item "Notes")) |> ignore
        cmdUpdate.Parameters.Add(new SqlCeParameter("@takeoffidentity", takeoffitem.Item "Id")) |> ignore
        cmdUpdate.ExecuteNonQuery() |> ignore

/// key -> TakeOffItemId
let remove_relatedObjects_takeoffAudit (key: int) = 
    let removeStatement = "DELETE FROM TakeOffAudit WHERE FKTakeOffItemId = @takeoffitemId"
    let cmdremove = new SqlCeCommand(removeStatement, open_connection cn)
    cmdremove.Parameters.Add("@takeoffitemId",key) |> ignore
    cmdremove.ExecuteNonQuery() |> ignore

let remove (takeoffitem: Dictionary<string, string>) = 
    let takeoffitemidStatement = "SELECT TakeOffItemId FROM TakeOffSheet WHERE TakeOffIdentity = @takeoffidentity"
    let cmdtakeoffitemid = new SqlCeCommand(takeoffitemidStatement,open_connection cn)
    cmdtakeoffitemid.Parameters.Add(new SqlCeParameter("@takeoffidentity",takeoffitem.Item "Id")) |> ignore
    //remove from takeoff audit table
    cmdtakeoffitemid.ExecuteScalar() |> to_int |> remove_relatedObjects_takeoffAudit 
    let removestatement = "DELETE from TakeOffSheet WHERE TakeOffIdentity = @takeoffidentity"
    let cmdRemove = new SqlCeCommand(removestatement, open_connection cn)
    cmdRemove.Parameters.Add(new SqlCeParameter("@takeoffidentity", takeoffitem.Item "Id")) |> ignore
    cmdRemove.ExecuteNonQuery() |> ignore

let takeoffAgent = Agent<(Dictionary<string, string> * seq<string> * string)>.Start(fun m -> async {
                     while true do                     
                           let! (ti,obj,msg) = m.Receive()
                           match msg with 
                           | "Add" -> add(ti,obj)
                           | "Update" -> update(ti)
                           | "Remove" -> remove(ti)
                           | _ -> None |> ignore
                  })
// Post message to the agent
///Add to takeoff
takeoffAgent.Post((item,relatedObjectIds,"Add"))
///Update takeoff

takeoffAgent.Post( modifieditem, Seq.empty ,"Update" )
///Remove takeoff
takeoffAgent.Post((modifieditem, Seq.empty, "Remove"))
