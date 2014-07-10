open System

type crossfilter() = 
    
    let dataset = ref [||]
        
    let filters = ref [||]
    member xf.add (records: 'a array) = 
           (* Grab the current records in the data set *)
           let current_records = !dataset
           let mutable index = current_records.Length - 1
           let d = 
             match current_records.Length with
             | n when n <= 0 -> Array.Resize(dataset,records.Length)
             | _ -> Array.Resize(dataset,current_records.Length + records.Length)
           let cr = !dataset    
             
           
           (* Add all the new records to the dataset *)
           for r in records do 
              index <- index + 1
              cr.SetValue(r,index)
           (* Add all the newly added records back to the dataset *)
           dataset := cr

    member xf.size() = (!dataset).Length



type FamilyMember = {Name: string; Age: int}
let parents = [| {Name = "Cameron"; Age = 33}; {Name = "Rachel"; Age = 28};|]
let kids = [| {Name = "Nicole"; Age = 11}; {Name = "Katheryn"; Age = 10}; {Name = "Charlotte"; Age = 2}|]
let other = [| {Name = "Kim"; Age = 39}; {Name = "Scott"; Age = 38}; {Name = "Jessica"; Age = 34};|]

let xf = new crossfilter()

xf.add other
xf.size()