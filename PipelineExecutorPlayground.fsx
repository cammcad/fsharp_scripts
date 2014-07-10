(* Abstract notion of a Pipeline Stage *)
type IPipelineStage =
     abstract execute: 'a -> 'a
     abstract execute: 'a

type PipelineStages = 
| WhatToFetchData of IPipelineStage
| FetchData of IPipelineStage 
| ApplyFilters of IPipelineStage
| FormatIO of IPipelineStage
| Archive of IPipelineStage



let pipeline = [FetchData,2; ApplyFilters,3; FormatIO,4; Archive,5]

let executePipline pipline =
   let rec execute pipline = 
       match pipline with 
       | [] -> printf "pipline complete!"
       | h::t ->
            match h with 
            | WhatToFetchData(ps), c -> 
                    printf "how to fetch data processing... num: %i" c
                    execute t
            | FetchData(ps), c -> 
                    printf "fetching data.... num %i" c
                    execute t
            | ApplyFilters(ps), c ->
                    printf "applying filters... num %i" c
                    execute t
            | FormatIO(ps), c ->
                    printf "formating and creating file... num %i" c
                    execute t
            | Archive(ps), c -> 
                    printf "archiving file... num %i" c
                    execute t

   execute pipline
