#I @"C:\mercurial\repositories\FLinq\FLinq\bin\Debug"
#I @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\"
#r "System.Xml.Linq.dll"
#r "FLinq.dll"


open FLinq.LinqHelper
open System.Xml
open System.Xml.Linq
open System



let doc = "<bookstore>
              <book>
                <title>Harry Potter</title>
                <author>J K. Rowling</author>
                <price>29.99</price>
              </book>
              <book>
                <title>Expert F#</title>
                <author>Don Syme</author>
                <price>59.99</price>
              </book>
            </bookstore>"

let xml = makeXML doc
  
let exp = "^bookstore/book/(title,author)"

query exp xml
 


let tokenlist = query.Split('/') |> List.ofArray


(* root node/query *)
let root token = not <| String.IsNullOrEmpty(token) && not <| token.Contains("[") && token.StartsWith("^")
(* node/query *)
let node_descendants token = not <| String.IsNullOrEmpty(token) && not <| token.Contains("[")
(* node/predicate/query *)
let node_pred token = not <| String.IsNullOrEmpty(token) && token.Contains("[")


let token_to_func doc tokens = 
    let rec reducer = function 
    | [] -> []
    | h :: t ->
        match h with
        | token when root token -> 
            let name = token.Substring(1,token.Length - 1)
            (fun _ -> descendantsfromRoot name doc)  :: reducer t
        | token when node_descendants token -> (fun (y: seq<XElement>) -> descendants token y) :: reducer t
        | token when node_pred token -> 
            (* for now we are skipping this*)
            reducer t
        | _ -> failwith "invalid expression"

    reducer tokens
            
let reducerfunctions = token_to_func xml tokenlist
List.fold(fun acc f -> f acc) Seq.empty reducerfunctions |> Seq.map(fun (x: XElement) -> x.Value)

//token_to_func tokenlist

descendantsfromRoot "bookstore" xml
|> descendants "book"
|> Seq.length


let v = ((fun _ -> query.IndexOf('(')) >> (fun index -> query.Substring(index)) >> (fun (x: string) -> x.Split(','))) query

exp.IndexOf('(')
|> fun index -> exp.Substring(index).Replace("(",String.Empty).Replace(")",String.Empty)
|> fun c -> c.Split(',')
|> List.ofArray



let rec 

