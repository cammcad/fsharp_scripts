open System
open System.Text
open System.Net
open Microsoft.FSharp.Control.WebExtensions
open System.Collections.Generic
(* Alias for Mailbox *)
type Agent<'T> = MailboxProcessor<'T>

// The following snippet shows how to wrap BeginGetContext/EndGetContext
// method pair and expose the operation using asynchronous workflow

[<AutoOpen>]
module HttpExtensions = 

  type System.Net.HttpListener with
    member x.AsyncGetContext() = 
      Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)
    static member Start(url, handler) =
            let serverHandler = async {
                use listener = new HttpListener()
                listener.Prefixes.Add(url)
                listener.Start()
                while true do
                    let! context = listener.AsyncGetContext()
                    Async.Start
                     ( handler(context.Request, context.Response)) }
            Async.Start(serverHandler)
            
  type System.Net.HttpListenerResponse with
    member x.Reply(s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      x.ContentLength64 <- int64 buffer.Length
      x.OutputStream.Write(buffer,0,buffer.Length)
      x.OutputStream.Close()
    member x.Reply(typ, buffer:byte[]) = 
      x.ContentLength64 <- int64 buffer.Length
      x.ContentType <- typ
      x.OutputStream.Write(buffer,0,buffer.Length)
      x.OutputStream.Close()  



(* Note - this does not return a handle to the listener
   so you will not be able to call stop, you must reset FSI *)
(* Listener implemented with just Async workflow no Agent *)
HttpListener.Start("http://localhost:8082/",
                (fun (request,response) -> async {
                    match request.Url.LocalPath with
                    | "/syncronex/fedx/cammcad" ->
                        response.Reply("<html><body><p>Welcome Cameron Frederick</p></body></html>")
                    | "/syncronex/dhl/rachel" ->
                        response.Reply("<html><body><p>Welcome Rachel Frederick</p></body></html>")    
                    | "/syncronex/ups/katie" ->
                        response.Reply("<html><body><p>Welcome katie</p></body></html>")
                    | _ -> response.Reply("Not Authorized")
                 }))


(* HttpAgent that listens for HTTP requests and handles *)
(* them using the function provided to the Start method *)
type HttpAgent private (url, f) as this =
  let agent = Agent.Start((fun _ -> f this))
  let server = async { 
    use listener = new HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()
    while true do 
      let! context = listener.AsyncGetContext()
      agent.Post(context) }
  do Async.Start(server)

  /// Asynchronously waits for the next incomming HTTP request
  /// The method should only be used from the body of the agent
  member x.Receive(?timeout) = agent.Receive(?timeout = timeout)

  /// Stops the HTTP server and releases the TCP connection
  //member x.Stop() = agent.

  /// Starts new HTTP server on the specified URL. The specified
  /// function represents computation running inside the agent.
  static member Start(url, f) = 
    new HttpAgent(url, f)

type SystemMsg = 
    | GetContent of AsyncReplyChannel<string list>
    | SendMessage of string




(* Test a single agent's capabilities *)
let simpleAgent = Agent.Start(fun inbox -> async {
                                                    
                            while true do 
                               let! msg = inbox.Receive()
                               let db = async { Async.Sleep(1000) 
                                                |> Async.RunSynchronously
                                                return "db" }
                               let profile = async { 
                                            Async.Sleep(300) |> Async.RunSynchronously
                                            return "profile"
                                           }
                               let summation = async {
                                                        
                                                        Async.Sleep (500) 
                                                        |> Async.RunSynchronously
                                                        failwith("whoa!")
                                                       }

                               Async.Parallel( seq { yield db
                                                     yield profile
                                                     yield summation } ) 
                               |> Async.RunSynchronously
                               |> Array.iter(fun s -> printf "m: %s" s)
                               printf "msg: %s" msg
                        })



type agentTestMsg =  | Print of string | Stop

let agentTest = Agent.Start(fun inbox -> async { 
                                   while true do 
                                    let! msg = inbox.Receive()
                                    match msg with 
                                    | Print value -> printf "%s" value
                                    | Stop -> failwith "attempting to stop agent"
                            })


agentTest.Post(Print("Syncronex!"))
agentTest.Post(Print("What up Agent!"))
agentTest.Post(Stop)












Async.Parallel( seq { for i = 0 to 10 do simpleAgent.Post(sprintf "msg # %i \n" i) } )
|> Async.RunSynchronously
|> ignore




(* completed agent which handles completed computations and builds a completed store *)
let completedAgent = 
        Agent.Start(fun inbox -> async {
              let store = new Dictionary<string,string list>()
              while true do
                let! msg = inbox.Receive()
                if not(List.isEmpty msg) then
                    let id = Guid.NewGuid().ToString()
                    store.Add(id,msg)
                    printf "key: %s - value: %A \n" id msg
                })


(* Mock out DB I/O *)
let dbAgent = 
    Agent.Start(fun inbox ->  async {
                    let store = new Dictionary<string,seq<string>>()
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | SendMessage text -> 
                            //simulate latency I/O
                            Async.Sleep(2000) |> Async.RunSynchronously
                            //return some arbitrary result
                            completedAgent.Post(["local1";"local2";"local3"])
                            
                        | GetContent reply -> 
                            reply.Reply(["local1";"local2";"local3"])
                })

(* Mock out service I/O *)
let servicedata = 
    Agent.Start(fun inbox -> async {
                while true do
                    let! msg = inbox.Receive()
                    match msg with
                    | SendMessage text -> 
                        //simulate latency I/O
                        Async.Sleep(4000) |> Async.RunSynchronously
                        //return some arbitrary result
                        completedAgent.Post(["serviceResul1";"serviceResult2";"ServiceResult3"])
                            
                    | GetContent reply -> 
                        reply.Reply(["local1";"local2";"local3"])   })




(* Simulate supervising Agent *)
let driver1 = SendMessage("joe")
let driver2 = SendMessage("jim")
let driver3 = SendMessage("Jake")

dbAgent.Post(driver1)
servicedata.Post(driver1)

dbAgent.Post(driver2)
servicedata.Post(driver2)

dbAgent.Post(driver3)
servicedata.Post(driver3)


(* Simulate 1000 clients *)

let getHtml (url : string) =
    async {

        let req = WebRequest.Create(url)
        let! rsp = req.AsyncGetResponse()
        use stream = rsp.GetResponseStream()
        use reader = new System.IO.StreamReader(stream)
        return reader.ReadToEnd()
    }


let execute_clients =
    Async.Parallel( seq { for i = 1 to 1000 do yield getHtml "http://localhost:8082/" })
    |> Async.RunSynchronously
    |> Array.iter(fun r -> printf "msg: %s \n" r)


let results = 
        async {
            Async.Parallel( seq { for i = 1 to 1000 do yield getHtml "http://localhost:8082/" })
            |> Async.RunSynchronously
            |> Array.iter(fun r -> printf "msg: %s \n" r)        
            
            Async.Sleep(1000) |> ignore
            
            Async.Parallel( seq { for i = 1 to 1000 do yield getHtml "http://localhost:8082/" })
            |> Async.RunSynchronously
            |> Array.iter(fun r -> printf "msg: %s \n" r)

        } |> Async.Start

    
    
    