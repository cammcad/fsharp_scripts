open System
open System.Reflection
open System.IO

#r @"C:\Syncronex prototypes\PluginContracts\PluginContracts\bin\Debug\PluginContracts.dll"
open Contracts


let plugins_location = new DirectoryInfo(@"c:\Extensions")
let assemblyname = 
    plugins_location.GetFiles("*.dll")
    |> Seq.filter(fun n -> n.Name.ToLower() = "pipelinedefinition.dll")
    |> Seq.head
    |> (fun n -> AssemblyName.GetAssemblyName(n.FullName) )



plugins_location.GetFiles("*.dll")
|> Seq.map(fun n -> AssemblyName.GetAssemblyName(n.FullName))
|> Seq.iter(fun an -> AppDomain.CurrentDomain.Load(an) |> ignore)

let definitionAssembly = AppDomain.CurrentDomain.Load(assemblyname)

let extract op = 
    match op with
    | Some v -> true
    | None -> false

let FetchType (defAsm: Assembly) = 
     let definitions = seq { for t in defAsm.GetTypes() do
                                printf "type: %s \n" t.FullName
                                if t.GetInterface("IDefinition") <> null then
                                         yield t  }
    
     if Seq.length(definitions) > 0 then
        Some(Seq.head(definitions))
     else
        None
     
let t_opt = FetchType(definitionAssembly) 
if extract t_opt then
  (* Discovered a type that implements the IDefinition<'a> Interface, so create an instance *)
    let bi = Activator.CreateInstance(t_opt.Value) :?> IDefinition

    let result = bi.FetchDefinition "" "";
    printf "%A" result



