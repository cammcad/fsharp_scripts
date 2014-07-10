#I @"D:\ODev Solutions\MongoDb\CSharpDriver"
#r "MongoDB.Bson.dll"
#r "MongoDB.Driver"

#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF\WindowsBase.dll"
#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF\PresentationFramework.dll"
#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF\PresentationCore.dll"
#r @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Xaml.dll"

open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Driver
open MongoDB.Bson.IO
open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open System.Windows
open System.Windows.Controls
open System.Windows.Media.Animation
open System.Collections.Generic


let Bson_NameValue (name,value) = 
    new BsonElement(name,BsonValue.Create value)

let getkeys (document: BsonDocument) = 
    document.Names


let printval (document: BsonDocument) = 
    let locationx = document.GetValue("LocationX").AsDouble
    let locationy = document.GetValue("LocationY").AsDouble
    let uimarker = document.GetValue("UIMarker").AsString
    printf "X: %A Y: %A  MarkerType: %s \n" locationx locationy uimarker

(* Connect to Mongo Server which should just be a process running on the machine *)
//let connectionString = "mongodb://localhost"
//let server = MongoServer.Create connectionString (local db)
let mongodb = "mongodb://dbuser:admin@flame.mongohq.com:27094/formationsApp"
// example: mongodb://username:password@flame.mongohq.com:portnumber/databasename
let server = MongoServer.Create mongodb
server.Connect
(* Get Database *)
let test = server.GetDatabase "test"
let formationhelperdb = server.GetDatabase "formationsApp"






(* Store usernames(email) and passwords *)
let profilescollection = formationhelperdb.GetCollection "profiles"
profilescollection.Insert(new BsonDocument())

(* Store Sequences *)
let sequencesCollection = formationhelperdb.GetCollection "sequences"
sequencesCollection.Insert(new BsonDocument())

(* Store Formations *)
let formationsCollection = formationhelperdb.GetCollection "formations"
formationsCollection.Insert(new BsonDocument())

(* Store Markers *)
let markerCollection = formationhelperdb.GetCollection "markers"
markerCollection.Insert(new BsonDocument())



// example retrieval 
let profiles = profilescollection.FindOne() 
let r = formationhelperdb.GetCollectionNames()






        



(* Formation Helper mocking out types to see if they'll go in Mongo *)
type profile = {UserName : string; Password : string; }
type sequence = { SequenceID : string; User : string; Name : string; }

type marker = { FormationID : string;
                Description : string;
                LocationX : float;
                LocationY : float;
                UIMarker : string;
                ToolTipName : string;  }

type formation = { SequenceID : string;
                   FormationID : string;
                   Description : string;
                   Image : byte array;
                   BeginTimeDelay : string; }

let seqID = Guid.NewGuid().ToString("N")
let formationID = Guid.NewGuid().ToString("N")

let p = { UserName = "cameron.frederick@gmail.com"; Password = "cam2378"; }

let s = { SequenceID = seqID; User = "cameron.frederick@gmail.com"; Name = "My First Sequence"; }
    
let m =  { FormationID = formationID; Description = "markerone"; LocationX = 1.0; LocationY = 1.0; UIMarker = "TextBlock"; 
               ToolTipName = "" }
let f = { SequenceID = seqID; FormationID = formationID; Description = "formation1"; Image = "somebytes"B; BeginTimeDelay = ""; }  



let convert_profile_to_BsonDocument (p: profile) = 
    let d = new BsonDocument()
    d.Add(Bson_NameValue("UserName",p.UserName)) |> ignore
    d.Add(Bson_NameValue("Password",p.Password)) |> ignore
    d

let convert_sequence_to_BsonDocument (s: sequence) = 
    let sequence = new BsonDocument()
    sequence.Add(Bson_NameValue("SequenceID", s.SequenceID)) |> ignore
    sequence.Add(Bson_NameValue("User", s.User)) |> ignore
    sequence.Add(Bson_NameValue("Name", s.Name)) |> ignore
    sequence

let convert_formation_to_BsonDocument (f: formation)  = 
    let formation = new BsonDocument()
    formation.Add(Bson_NameValue("SequenceID", f.SequenceID)) |> ignore
    formation.Add(Bson_NameValue("FormationID", f.FormationID)) |> ignore
    formation.Add(Bson_NameValue("Description", f.Description)) |> ignore
    formation.Add(Bson_NameValue("Image", f.Image)) |> ignore
    formation.Add(Bson_NameValue("BeginTimeDelay", f.BeginTimeDelay)) |> ignore
    formation

let convert_marker_to_BsonDocument (m: marker) = 
    let marker = new BsonDocument()
    marker.Add(Bson_NameValue("FormationID", m.FormationID)) |> ignore
    marker.Add(Bson_NameValue("Description", m.Description)) |> ignore
    marker.Add(Bson_NameValue("LocationX", m.LocationX)) |> ignore
    marker.Add(Bson_NameValue("LocationY", m.LocationY)) |> ignore
    marker.Add(Bson_NameValue("UIMarker", m.UIMarker)) |> ignore
    marker.Add(Bson_NameValue("ToolTipName", m.ToolTipName)) |> ignore
    marker


let profilebson = convert_profile_to_BsonDocument p
let seqbson = convert_sequence_to_BsonDocument s
let formbson = convert_formation_to_BsonDocument f
let markerbson = convert_marker_to_BsonDocument m


(* Insert Bson Documents *)
profilescollection.Insert profilebson
sequencesCollection.Insert seqbson
formationsCollection.Insert formbson
markerCollection.Insert markerbson

(* Find seq collection for given user "me" *)
let qd = new QueryDocument(Bson_NameValue("UserName","cameron.frederick@gmail.com"))
let qdseq = new QueryDocument(Bson_NameValue("SequenceID", seqID))
let qdform = new QueryDocument(Bson_NameValue("FormationID", formationID))
let sequences = sequencesCollection.Find qd
let formations = formationsCollection.Find(qdseq)
let markers = markerCollection.Find(qdform)
Seq.iter(fun (s: BsonDocument) -> 
                  let user = s.GetValue("User").AsString
                  let name = s.GetValue("Name").AsString
                  let seqid = s.GetValue("SequenceID").AsString
                  printf "User: %s \n Name: %s \n ID: %s \n " user name seqid) sequences




let d = new BsonDocument()
let onearray = new BsonArray()
onearray.Add(BsonValue.Create "one") |> ignore
d.Add(Bson_NameValue("key",onearray))
d.GetValue("key").AsBsonArray.[0] <- BsonValue.Create("two")
