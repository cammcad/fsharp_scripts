open System
open System.Threading

let cancelableTask =
    async {
        printfn "Waiting 10 seconds..."
        for i = 1 to 10 do 
            printfn "%d..." i
            do! Async.Sleep(1000)
        printfn "Finished!"
    }
   
// Callback used when the operation is canceled
let cancelHandler (ex : OperationCanceledException) = 
    printfn "The task has been canceled."

Async.TryCancelled(cancelableTask, cancelHandler)
|> Async.Start

// ...

Async.CancelDefaultToken()