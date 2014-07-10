(* Registry Module playground *)

#r @"C:\Syncronex prototypes\Pipeline Plugins\Definitions\PipeLineBuilder\PipeLineBuilder\bin\Debug\PipeLineBuilder.dll"
#r @"C:\Syncronex prototypes\PluginContracts\PluginContracts\bin\Debug\PluginContracts.dll"
open System
open System.IO
open System.Collections.Generic
open System.Reflection
open Contracts
open Syncronex.Pipeline


exception PipelineStepException of string
type PipelineResult = | PipelineStepSuccess of PipelineContext  | PipelineStepFailure of PipelineStepException | EmptyPipelineResult 

type PipelineSteps = {
    Definition: (unit -> PipelineResult option) option;
    Fetch: (PipelineContext -> PipelineResult option) option;
    Filter: (PipelineContext -> PipelineResult option) option;
    Format: (PipelineContext -> PipelineResult option) option;
    Archive: (PipelineContext -> PipelineResult option) option;
}


let getinstance (instance: Type) = Activator.CreateInstance(instance)

(* Creating all the standard or inital plugins *)
module PipelineStepBuilder = 
    let construct_definition() : (unit -> PipelineResult option) =
            fun _ ->
                  let assm = Builder.fetch_plugin_by_assembly @"C:\Extensions" "pipelinedefinition.dll"
                  let pipeline_step = Builder.fetch_types "IDefinition" assm
                  match pipeline_step with
                  | Some(t,_) ->
                        Some( ( 
                                try
                                   let instance = getinstance t :?> IDefinition
                                   let result = instance.FetchDefinition "Adjustment" "configPath"
                                   PipelineStepSuccess(result)
                                with ex -> 
                                        let msg = PipelineStepException(ex.Message) :?> PipelineStepException
                                        PipelineStepFailure(msg)
                              ) )

                  | None -> None
    
    let construct_fetchdata(): (PipelineContext -> PipelineResult option)  =
            fun (ctx: PipelineContext) ->
                  let assm = Builder.fetch_plugin_by_assembly @"C:\Extensions" "pipelinefetch.dll"
                  let pipeline_step = Builder.fetch_types "IFetchData" assm
                  match pipeline_step with
                  | Some(t,_) ->
                        Some( ( 
                                try
                                   let instance = getinstance t :?> IFetchData
                                   let result = instance.FetchData ctx
                                   ctx.accum.Add("FetchData",result)
                                   PipelineStepSuccess(ctx)
                                with ex -> 
                                        let msg = PipelineStepException(ex.Message) :?> PipelineStepException
                                        PipelineStepFailure(msg)
                              ) )

                  | None -> None 

(* Handlers for Registry Events *)
module RegistryHandlers = 

    let plugin_added (e: FileSystemEventArgs) (pipeline_steps: List<PipelineSteps>) = 
            match e.Name.ToLower() with
            | "pipelinedefinition.dll" ->
                let current = pipeline_steps.[0]
                let update = { current with Definition = Some(PipelineStepBuilder.construct_definition()) }
                pipeline_steps.[0] <- update 
            | "pipelinefetch.dll" ->
                 let current = pipeline_steps.[0]
                 let update = { current with Fetch = Some(PipelineStepBuilder.construct_fetchdata())  }
                 pipeline_steps.[0] <- update 
            | "pipelinefilter.dll" ->
                 let current = pipeline_steps.[0]
                 let update = { current with Filter = None  }
                 pipeline_steps.[0] <- update
            | "pipelineformat.dll" ->
                 let current = pipeline_steps.[0]
                 let update = { current with Format = None  }
                 pipeline_steps.[0] <- update
            | "pipelinearchive.dll" ->
                 let current = pipeline_steps.[0]
                 let update = { current with Archive = None  }
                 pipeline_steps.[0] <- update
            | _ -> failwith "Unknown Pipeline Step detected"


(* ==== Registry - manages the plugins ==== *)
type Registry(path,extension) =
    let pipelineSteps = new List<PipelineSteps>()
    //standard plugins as a record of functions
    do pipelineSteps.Add({Definition = Some(PipelineStepBuilder.construct_definition());
                         Fetch = Some(PipelineStepBuilder.construct_fetchdata());
                         Filter = None;
                         Format = None;
                         Archive = None;}) |> ignore
    let fsw = new FileSystemWatcher(path,extension)
    let log = []
    //plugin added
    do fsw.Created.Add( fun e ->
                            sprintf "File Created: %s" e.Name :: log |> ignore
                            RegistryHandlers.plugin_added e pipelineSteps )

    //implement me: plugin removed function so that a standard plugin can be replaced
    //when an extension plugin is removed
    //do fsw.Deleted.Add( fun e -> ... )

    //set so that we can start watching...
    do fsw.EnableRaisingEvents <- true

    (* Fetch a pipeline step for the pipeline to run *)
    member self.FetchPipelineStep = pipelineSteps




(* Registry Usage Example *) 
let r = new Registry(@"c:\Extensions", "*.dll")

let definition = (r.FetchPipelineStep.[0]).Definition
match definition with
| Some(f) -> 
    let result = f()
    match result with
    | Some(v) ->
        match v with
        | PipelineStepSuccess(ctx) -> 
            let fetch = (r.FetchPipelineStep.[0]).Fetch
            match fetch with
            | Some(v) -> Some(v(ctx))
            | None ->  None
        | PipelineStepFailure(pse) -> None
        | EmptyPipelineResult -> None
    
    | None -> None              
| None -> None

