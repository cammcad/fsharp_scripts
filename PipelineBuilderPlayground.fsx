#r @"C:\Syncronex prototypes\Pipeline Plugins\Definitions\PipeLineBuilder\PipeLineBuilder\bin\Debug\PipeLineBuilder.dll"
#r @"C:\Syncronex prototypes\PluginContracts\PluginContracts\bin\Debug\PluginContracts.dll"
open System
open System.Reflection
open Contracts
open Syncronex.Pipeline


exception PipelineStepException of string

type PipelineResult = | PipelineStepSuccess of PipelineContext  | PipelineStepFailure of PipelineStepException | EmptyPipelineResult 

let getinstance (instance: Type) = Activator.CreateInstance(instance)
let construct_definition() : (unit -> PipelineResult option) =
    fun _ ->
          let assm = Builder.fetch_plugin_by_assembly @"C:\Extensions" ".pipelinedefinition"
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




let construct_fetchdata (ctx: PipelineContext) =
    fun _ ->
          let assm = Builder.fetch_plugin_by_assembly @"C:\Extensions" ".pipelinefetchdata"
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



let extract_pipelineval (t_opt: PipelineResult option) = 
    match t_opt with
    | Some(pr) -> pr
    | None -> PipelineResult.EmptyPipelineResult

let extact_pipeline_result (plv: PipelineResult) = 
    match plv with 
    | PipelineStepSuccess(v) -> v
    | _ -> failwith "bad pipeline value"

let f = construct_definition()
let ctx =  f() |> extract_pipelineval |> extact_pipeline_result

let f_fetch = construct_fetchdata

let fetch_result: (unit -> PipelineResult option) = f_fetch(ctx) 
let r = fetch_result()
