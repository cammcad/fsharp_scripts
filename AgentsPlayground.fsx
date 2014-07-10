// Agent Based Programming Playground

open System



type TraceRoute<'a>() =
    let mailbox = new MailboxProcessor<'a>()
    member self.Post msg = mailbox.Post(msg)
    member self.PostAndAsyncReply : (AsyncReplyChannel<'Reply> -> 'Msg) * int option -> Async<'Reply>
    member this.PostAndReply : (AsyncReplyChannel<'Reply> -> 'Msg) * int option -> 'Reply
    member this.PostAndTryAsyncReply : (AsyncReplyChannel<'Reply> -> 'Msg) * ?int -> Async<'Reply option>
    member this.Receive : ?int -> Async<'Msg>
    member this.Scan : ('Msg -> Async<'T> option) * ?int -> Async<'T>
    member this.Start : unit -> unit
    static member Start : (MailboxProcessor<'Msg> -> Async<unit>) * ?CancellationToken -> MailboxProcessor<'Msg>
    member this.TryPostAndReply : (AsyncReplyChannel<'Reply> -> 'Msg) * ?int -> 'Reply option
    member this.TryReceive : ?int -> Async<'Msg option>
    member this.TryScan : ('Msg -> Async<'T> option) * ?int -> Async<'T option>
    member this.add_Error : Handler<Exception> -> unit
    member this.CurrentQueueLength :  int
    member this.DefaultTimeout :  int with get, set
    member this.Error :  IEvent<Exception>
    member this.remove_Error : Handler<Exception> -> unit













type AgentMessage = GetContent of AsyncReplyChannel<string> 

type Agent<'T> = MailboxProcessor<'T>


let simpleMessagePrinterAgent = 
        Agent.Start( fun inbox ->  
                                let rec loop message = 
                                       async {
                                               let! message = inbox.Receive()
                                               match message with
                                               | GetContent(replyChannel) -> replyChannel.Reply("Got it!")
                                               return! loop() } 
                                loop())
                                        


simpleMessagePrinterAgent.PostAndReply(fun from -> GetContent(from))