#I @"C:\Windows\Microsoft.NET\Framework\v4.0.30319"
#r "System.Xml.Linq.dll"
#r @"C:\Project Dragnet\svn\ITO\Libraries\FsCore.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\PresentationCore.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\Profile\Client\System.Xaml.dll"


open System
open System.Windows
open System.Windows.Controls
open System.Xml
open System.Xml.Linq
open FsCore

(* load the xml *)
(* Phase 1 *)
let phase1 = XDocument.Load(@"C:\Project Dragnet\data\gilead\phase1\D71466AE-4B43-46AD-87C7-281CBD18CDB4.content.xml")
(* Phase 2 *)
let phase2 = XDocument.Load(@"C:\Project Dragnet\data\gilead\phase2\447875BD-7E50-49CA-84FE-5C3280693731.content.xml")
(* Phase 3 *)
let phase3 = XDocument.Load(@"C:\Project Dragnet\data\gilead\phase3\55E3B16A-F832-4600-9E2B-BFEC4F489491.content.xml")


(* helper values *)
let ns: XNamespace = XNamespace.Get("DWF-Content:1.0")
let xname s = XName.Get(s)
let structured_revisions (rev: string list) = rev |> List.map(fun rev -> rev |> XDocument.Load)



(* Phases or Revisions *)
let phases = 
    structured_revisions [@"C:\Project Dragnet\data\gilead\phase1\D71466AE-4B43-46AD-87C7-281CBD18CDB4.content.xml";
                          @"C:\Project Dragnet\data\gilead\phase2\447875BD-7E50-49CA-84FE-5C3280693731.content.xml";
                          @"C:\Project Dragnet\data\gilead\phase3\55E3B16A-F832-4600-9E2B-BFEC4F489491.content.xml"]

(* all top level families in the data *)
let families (datasource: XDocument) = 
    datasource.Descendants(ns + "Objects").Elements(ns + "Object")
    |> Seq.map(fun xelem -> xelem.Attribute(xname "label").Value.Split('(').[0])


let get_and_clean_dimensionalvalues (objs: seq<XElement>) (dv: string) = 
        seq { for obj in objs do
                for p in obj.Descendants(ns + "Property") do
                    if p.Attribute(xname "name").Value = dv then
                        yield Double.Parse(p.Attribute(xname "value").Value.Split(' ').[0]) }

let get_length_dimensionalvalues (objs: seq<XElement>) =
    let input = [| for obj in objs do
                    for p in obj.Descendants(ns + "Property") do
                        if p.Attribute(xname "name").Value = "Length" then
                            yield p.Attribute(xname "value").Value |] 
    SummaryViewProvider.GetLenghtTotalForVarianceReporting input

let calc_objects (objs: seq<XElement>) =
    let flatobjects =  objs |> Seq.filter(fun xe -> xe.Attribute(xname "label").Value.Contains("[")) 
    let count = Seq.length flatobjects
    let surfaceArea = get_and_clean_dimensionalvalues flatobjects "Area" 
                      |> Seq.sum |> (fun d -> String.Format("{0:N} SF",d))
    let volume = get_and_clean_dimensionalvalues flatobjects "Volume" 
                 |> Seq.sum |> (fun d -> d * 0.037037037) |> (fun d -> String.Format("{0:N} CY",d))
    let length = get_length_dimensionalvalues flatobjects
    
    (count,surfaceArea,volume,length)

let get_child_familynames fn (datasource: XDocument) = 
    datasource.Descendants(ns + "Objects").Descendants(ns + "Object")
    |> Seq.filter( fun x -> x.Attribute(xname "label").Value.Contains(fn))
    |> fun x -> x.Descendants(ns + "Object")
    |> Seq.filter( fun x -> x.Attribute(xname "label").Value.Contains("("))
    |> Seq.map( fun xe -> (xe.Attribute(xname "label").Value.Split('(').[0], calc_objects(xe.Descendants(ns + "Object")) ))


let GenerateFamilyReportForRevision rev  =
    let datasource = XDocument.Parse(rev)
    let input = families datasource
    Seq.map( fun fam -> fam, get_child_familynames fam datasource ) input
    



//let GenerateReport seq = 
//    Seq.map(fun rev -> GenerateFamilyReportForRevision rev) seq

(* Async Functions *)
let GenerateReportFunc = 
    new Func<string, seq<string * seq<string * (int * string * string * string)>>>(GenerateFamilyReportForRevision)


let phases = 
    seq { yield phase1.ToString()
          yield phase2.ToString()
          yield phase3.ToString()  }

let KickOff = 
    let asyncfuncs = Seq.map(fun phase -> Async.FromBeginEnd(phase,GenerateReportFunc.BeginInvoke,GenerateReportFunc.EndInvoke)) phases
    Async.RunSynchronously(Async.Parallel asyncfuncs) |> Seq.concat


let printout s = 
    Seq.iter(fun f -> printf "%A" f ) s

                         



(* UI Functions *)

(* Family Types By Count *)
  
//let clean (familyname: string) = 
//    let cleanfamilyname = familyname.Remove(familyname.Length - 1,1)
//    cleanfamilyname


(* FamilyName By Property Function [Higher Order] *)
// args - familyname (e.g. "Walls"),
//  function (to remove white space at end of string),
//  function (parse out property from tuple) (fun familyname,(count,area,volume,length)) -> familyname,count) 
let family_types_byproperty familyname cleanfunc parseprop =
    Seq.filter(fun (rf,rv) -> cleanfunc rf = familyname) KickOff
    |> Seq.map(fun (tf,tv) -> tv) |> Seq.concat
    |> Seq.map(parseprop)
    |> Seq.groupBy(fun (ft,c) -> ft)    