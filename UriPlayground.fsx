#I @"C:\RavenDB-Build-701\Client"
#I @"C:\RavenDB-Build-701\F# ClientAPI"
#r @"FSharp.PowerPack.Linq.Fixed.dll"
#r @"FSharpEnt.RavenDB.dll"
#r @"Raven.Client.Lightweight.dll"
#r @"Raven.Abstractions.dll"

open System
open System.Collections.Generic
open Raven.Client
open Raven.Client.Document
open FSharpEnt.RavenDB
open Microsoft.FSharp.Core
open Microsoft.FSharp.Linq


type UriParsedValue = | Invalid of string | Valid of Dictionary<string,Object>
type Appcontext = {mutable Id: string; Company: string; Property: string; App: string; Context: Dictionary<string,Object>; }

(* Helpers *)
let store (entity: Object) (session: IDocumentSession) = 
    session.Store(entity)
    session.SaveChanges()

let validUrl_ctx_lookup args (ds: DocumentStore) store = 
    let s = ds.OpenSession()
    let comp,prop,app = args
    query(where <@ fun x -> x.Company = comp && x.Property = prop && x.App = app @>) (s)
    
    
let kvs = Dictionary<string,Object>()
kvs.Add("companyid",1)
kvs.Add("connectionstring","blah-blah-blah-connect!")

let mediaGeneral_adminweb = {Id = null; Company = "mg"; Property = "rtd"; App = "AdminWeb"; Context = kvs}
let mediaGeneral_syncaccess = {Id = null; Company = "mg"; Property = "rtd"; App = "SyncAccess"; Context = kvs}

(* Establish connection to Raven Db *)
let ds = new DocumentStore()
ds.Url <- "http://localhost:8080/"
ds.DefaultDatabase <- "myStore"
ds.Initialize()

(* Insert Data *)
store mediaGeneral_adminweb (ds.OpenSession())



let paywall = Uri("http://images.google.com/chips/doritos")
let segments = paywall.Segments
(* Make sure we have a valid number of arguments *)
let validArgs args = if Seq.length args > 4 then true else false
let clean (arg: string) = arg.Replace("/","")

let result = 
    let args =
        if validArgs segments then
            clean segments.[1], clean segments.[2], clean segments.[3]
        else
            "","","" 
    match args with 
    | "","","" -> Invalid("not enough route information supplied")
    | "",prop,app -> Invalid("invalid url: companyName is missing")
    | comp,"",app -> Invalid("invalid url: propertyName is missing")
    | comp,prop,"" -> Invalid("invalid url: applicationName is missing")
    | comp,prop,app -> 
        let r = validUrl_ctx_lookup (comp,prop,app) ds "myStore"
        match Seq.length(r) = 1 with
        | true -> 
            Seq.head(r) |> fun (x: Appcontext) -> Valid(x.Context)
        | false ->
            Invalid("insufficent route information supplied")



