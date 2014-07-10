
open System
open System.Collections
open System.Collections.Generic
open System.IO

(* Parsing a random txt file for word count *)


/// map
let data = 
    File.ReadAllLines(@"C:\Users\cfrederick\Documents\forum question for Design Review.txt")
    |> Array.collect(fun line -> line.Split(' ')) 

/// reduce
let accum = new Dictionary<string,int>()
Array.fold(fun (acc: Dictionary<string,int>) (elem: string) ->
            if acc.ContainsKey(elem) then
                let v = acc.Item elem
                acc.Item elem <- v + 1
                acc
            else
                acc.Add(elem, 1)
                acc) accum data