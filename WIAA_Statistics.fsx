open System
open System.IO


type Category = | Dance | HipHop | POM | DRILL

type Teamsize = | Small | Medium | Large | Unknown

let select_category (line: string) c = 
    let items = line.Split('|')
    let select = fun singleton -> Some(Seq.head(singleton))
    match Seq.length(items) > 2 with
    | false -> None
    | true -> 
        match c with
        | Dance ->
                items
                |> Seq.take(1)
                |> select
        | HipHop ->
                items
                |> Seq.skip(1) 
                |> Seq.take(1)
                |> select
        | POM ->
                items
                |> Seq.skip(2)
                |> Seq.take(1)
                |> select
        | DRILL ->
                items
                |> Seq.skip(3)
                |> Seq.take(1)
                |> select


let raw_data = File.ReadAllLines(@"C:\WIAA Data Analytics\1A_2A_3A data.txt")


let parse_teamSize = fun (team_size: string) -> team_size.Split('_')

let remove_blanks = fun (items: seq<string array>) -> Seq.filter(fun (x: string array) -> x.[0].Replace(" ", String.Empty) <> String.Empty) items

let pick_teamAndSize = fun (x: seq<string array>) -> Seq.map(fun (z: string array) -> z.[0], (z.[1] |> int), (z.[2] |> int)) x

let parse = fun items -> items |> Seq.skip(1) |> Seq.map parse_teamSize |> remove_blanks |> pick_teamAndSize
let pick_category = fun category -> raw_data |> Seq.map(fun x -> select_category x category) |> Seq.choose(fun x -> x) |> parse

let dance_data =  pick_category Dance
let hiphop_data = pick_category HipHop
let pom_data = pick_category POM
let drill_data = pick_category DRILL

let team_sizes = fun data -> Seq.groupBy(fun (item : string * int * int) -> 
                                    let name,size,score = item
                                    match size with
                                    | v when v > 3 && v <= 12 -> Small
                                    | v when v > 12 && v <= 20 -> Medium
                                    | v when v > 20 -> Large 
                                    | _ -> Unknown) data


(* Visualize Team Sizes *)

let printf_TeamSizes label data = 
    printf "****** START %s \n" label
    Seq.iter(fun (size,teams) -> 
                printf "******TeamSize: %A \n" size
                Seq.iter(fun (team,count,score) -> printf "%s %i %i \n" team count score)teams) data
    printf "***** END: %s \n" label



//team_sizes dance_data
//|> Seq.iter(fun (size,teams) -> 
//                printf "******TeamSize: %A \n" size
//                Seq.iter(fun (team,count) -> printf "%s %i \n" team count) teams)

printf_TeamSizes "pom" (team_sizes pom_data)


(* Grouping Categories - Rule of 4 and category dependent grouping *)
let final_groups = fun category_data ->
                    let L = category_data |> Seq.find(fun (teamsize,teams) -> teamsize = Large) |> fun (s,t) -> Seq.length(t) - 1                         
                    let M = category_data |> Seq.find(fun (teamsize,teams) -> teamsize = Medium) |> fun (s,t) -> Seq.length(t) - 1 
                    let S = category_data |> Seq.find(fun (teamsize,teams) -> teamsize = Small) |> fun (s,t) -> Seq.length(t) - 1 

                    Seq.map(fun (item: Teamsize * seq<string*int*int>) ->
                                    let teams = snd(item)
                                    match fst(item) with
                                    | Large -> 
                                            if L < 4 then "Large/Medium",teams 
                                            elif L >= 4 then "Large",teams
                                            else "Large",teams
                                    | Medium -> 
                                            if M < 4 && L < S then "Large/Medium", teams
                                            elif M < 4 && S < L then "Medium/Small", teams
                                            elif M >= 4 then "Medium",teams
                                            else "Medium", teams
                                    | Small -> 
                                            if S < 4 then "Medium/Small",teams 
                                            elif S >= 4 then "Small", teams
                                            else "Small",teams
                                    | Unknown -> "Unknown",teams ) category_data


team_sizes drill_data
|> final_groups
|> printf_TeamSizes "drill"



   