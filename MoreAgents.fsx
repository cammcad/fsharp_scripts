open System

type MailboxProcessor<'a> with
    static member public Spawn(messageHandler: 'a -> 'b -> 'b , initialState: 'b) : MailboxProcessor<'a> =
        MailboxProcessor.Start(fun inbox -> 
                            let rec loop(state) = 
                                async {
                                            let! msg = inbox.TryReceive()
                                            try 
                                            match msg with 
                                            | None ->  return! loop(initialState)
                                            | Some(m) -> return! loop(messageHandler m state)
                                            with 
                                            | ex -> return ()

                                        }
                            loop(initialState))

(* Alias for Mailbox *)
type Agent<'T> = MailboxProcessor<'T>                                                                       

//let spawn f = Agent.Start(f)




(* simple calculator *)

//message calculator type
type CalcAction = | Add of (int * int * AsyncReplyChannel<int>) 
                  | Multiply of (int * int * AsyncReplyChannel<int>)
                  | Subtract 



let addAgent = Agent.Spawn((fun message n -> 
                                match message with
                                | Add(num1,num2,sender) ->
                                        sender.Reply(num1 + num2)
                                | _ -> None |> ignore), ())

let multiplyAgent = Agent.Spawn((fun message n -> 
                                    match message with
                                    | Multiply(num1,num2,sender) ->
                                            sender.Reply(num1 * num2)
                                    | _ -> None |> ignore), ())


let printAgent = Agent.Spawn((fun (str: string) (n: string list) -> 
                                match str with 
                                | "PrintState" ->
                                        List.iter(fun (item: string) -> Console.WriteLine(item)) n
                                        n
                                | _ -> str::n), [])
                                


//spawn (fun inbox -> 
//                         let rec loop message = 
//                             async {
//                                        let! message = inbox.Receive()
//                                        match message with
//                                        | Add(num1,num2,sender) ->
//                                             sender.Reply(num1 + num2)
//                                        | _ -> return ()
//                                    }
//                         loop())

//let multiplyAgent = spawn (fun inbox -> 
//                         let rec loop message = 
//                             async {
//                                        let! message = inbox.Receive()
//                                        match message with
//                                        | Multiply(num1,num2,sender) ->
//                                             sender.Reply(num1 * num2)
//                                        | _ -> return ()
//                                    }
//                         loop())

(* Print Agent or could be completed Agent *)
//let printAgent = spawn (fun inbox ->
//                            let rec loop n = 
//                                async {
//                                        let! str = inbox.Receive()
//                                        match str with 
//                                        | "PrintState" ->
//                                                List.iter(fun (item: string) -> Console.WriteLine(item)) n
//                                        | _ ->
//                                            do! loop(str::n)
//                                      } 
//                            loop([]))

(* request for computation message *)
type request = | Compute of int * int

let supervisor = Agent.Spawn((fun message n ->
                                let added = addAgent
                                let multi = multiplyAgent
                                let completed = printAgent                                
                                match message with
                                | Compute(num1,num2) ->
                                    let addResult = added.PostAndAsyncReply(fun sender -> Add(num1,num2,sender)) |> Async.RunSynchronously
                                    let multiResult = multi.PostAndAsyncReply(fun sender -> Multiply(num1,num2,sender)) |> Async.RunSynchronously
                                    completed.Post <| sprintf "add: %i  multiply: %i  \n" addResult multiResult), ())
                                                    

//let controllerAgent =  spawn (fun inbox ->
//                                    let added = addAgent
//                                    let multi = multiplyAgent
//                                    let completed = printAgent
//                                    let rec loop message = 
//                                        async {
//                                                let! message = inbox.Receive()
//                                                match message with
//                                                | Compute(num1,num2) ->
//                                                    let addResult = added.PostAndReply(fun sender -> Add(num1,num2,sender))
//                                                    let multiResult = multi.PostAndReply(fun sender -> Multiply(num1,num2,sender))
//                                                    completed.Post <| sprintf "add: %i  multiply: %i  \n" addResult multiResult
//                                                    return! loop()
//                                              } 
//                                    loop())
//controllerAgent.Post(Compute(r.Next(0,10),r.Next(0,10)))
let r = new Random()
supervisor.Post(Compute(r.Next(0,10),r.Next(0,10)))
printAgent.Post("PrintState")


















//                  let first,second = be
//                  let fk,fv = first
//                  target.Resources.Add(fk, fv)
//                  fv.Completed.Add( fun _ ->
//                                    target.Resources.Remove(fk)
//                                    let sk,sv = second
//                                    target.Resources.Add(sk, sv)
//                                    sv.Completed.Add(fun _ -> target.Resources.Remove(sk))
//                                    sv.Begin())
//                  fv.Begin()
//            let _,v = Story.getLastStory stories
//            let k = Guid.NewGuid().ToString()
//            target.Resources.Add(k,v)
//            v.Completed.Add(fun _ -> target.Resources.Remove(k)) 
//            v.Begin()