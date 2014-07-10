#I @"C:\Users\cfrederick\Downloads\SignalR-SignalR-v0.3.5-88-g94aaf97\SignalR-SignalR-94aaf97\SelfHostingAssemblies"
#r @"SignalR.dll"
#r @"SignalR.Hosting.Common.dll"
#r @"SignalR.Hosting.Self.dll"
#r @"ImpromptuInterface.dll"
#r @"ImpromptuInterface.FSharp.dll"

open System
open System.Net
open System.Net.NetworkInformation
open System.Threading
open SignalR
open SignalR.Hubs
open SignalR.Hosting.Self
open SignalR.Hosting.Common
open ImpromptuInterface
open ImpromptuInterface.Dynamic

let generate_next_available_tcpPort = 
    let properties = IPGlobalProperties.GetIPGlobalProperties() 
    let active_tcp_connections = properties.GetActiveTcpConnections()
    Seq.map(fun (x: TcpConnectionInformation) -> x.LocalEndPoint.Address, x.LocalEndPoint.Port) active_tcp_connections
    |> Seq.sortBy(fun x -> snd(x))
    |> Seq.map(fun x -> snd(x))
    |> fun x -> 
           let index = Seq.length(x) - 1
           Seq.nth index x
    |> fun x -> x + 1




type Agent<'T> = MailboxProcessor<'T>
// The following snippet shows how to wrap BeginGetContext/EndGetContext
// method pair and expose the operation using asynchronous workflow

[<AutoOpen>]
module HttpExtensions = 

  type System.Net.HttpListener with
    member x.AsyncGetContext() = 
      Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)

// ----------------------------------------------------------------------------

/// HttpAgent that listens for HTTP requests and handles
/// them using the function provided to the Start method
type HttpAgent private (url, f) as this =
  let tokenSource = new CancellationTokenSource()
  let agent = Agent.Start((fun _ -> f this), tokenSource.Token)
  let server = async { 
    use listener = new HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()
    while true do 
      let! context = listener.AsyncGetContext()
      agent.Post(context) }
  do Async.Start(server, cancellationToken = tokenSource.Token)

  /// Asynchronously waits for the next incomming HTTP request
  /// The method should only be used from the body of the agent
  member x.Receive(?timeout) = agent.Receive(?timeout = timeout)

  /// Stops the HTTP server and releases the TCP connection
  member x.Stop() = tokenSource.Cancel()

  /// Starts new HTTP server on the specified URL. The specified
  /// function represents computation running inside the agent.
  static member Start(url, f) = 
    new HttpAgent(url, f)

// ----------------------------------------------------------------------------

/// SignalR Hub
type TimeHub() =
    class 
    inherit Hub()
        member x.GetTime() = base.Clients.shoTime(DateTime.Now.ToString()) 
    end
// ----------------------------------------------------------------------------

/// SignalRAgent that listens for HTTP requests and handles
/// them using the function provided to the Start method
type SignalRAgent (url, f) as this =
  let server = ref(SignalR.Hosting.Self.Server(url))
  let tokenSource = new CancellationTokenSource()
  let agent = Agent.Start((fun _ -> f this), tokenSource.Token)
//  let server = async { 
//    use listener = new HttpListener()
//    listener.Prefixes.Add(url)
//    listener.Start()
//    while true do 
//      let! context = listener.AsyncGetContext()
//      agent.Post(context) }
  let srvr = async { 
                        !server |> fun x -> x.Start()
                        !server |> fun y -> y.ConnectionManager.GetHubContext<TimeHub>()
                        !server |> fun z -> z.MapHubs() |> ignore
                     }

  do Async.Start(srvr, cancellationToken = tokenSource.Token)

  /// Asynchronously waits for the next incomming HTTP request
  /// The method should only be used from the body of the agent
  member x.Receive(?timeout) = agent.Receive(?timeout = timeout)

  /// Stops the HTTP server and releases the TCP connection
  member x.Stop() = 
        tokenSource.Cancel()
        let s = !server
        if s <> null then s.Stop() 

  /// Starts new HTTP server on the specified URL. The specified
  /// function represents computation running inside the agent.
  static member Start(url, f) = 
    new SignalRAgent(url, f)


// ----------------------------------------------------------------------------

// The HttpListenerContext type has two properties that represent HTTP request 
// and HTTP response respectively. The following listing extends the class 
// representing HTTP request with a member InputString that returns the data 
// sent as part of the request as text. It also extends HTTP response class 
// with an overloaded member Reply that can be used for sending strings or 
// files to the client:

open System.IO
open System.Text

[<AutoOpen>]
module HttpExtensions2 = 
  type System.Net.HttpListenerRequest with
    member request.InputString =
      use sr = new StreamReader(request.InputStream)
      sr.ReadToEnd()

  type System.Net.HttpListenerResponse with
    member response.Reply(s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentLength64 <- int64 buffer.Length
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()
    member response.Reply(typ, buffer:byte[]) = 
      response.ContentLength64 <- int64 buffer.Length
      response.ContentType <- typ
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()


open System.Data.SqlClient
open System.Runtime.Serialization.Formatters.Binary
open System.Net.Mail

// ----------------------------------------------------------------------------

// The following listing shows how to create an HTTP server that responds to 
// any incoming request with just "Hello world!" string.

let url = sprintf "http://localhost:%i/" generate_next_available_tcpPort
let convert_stream_toBytes (stream: Stream) = 
    let memoryStream = new MemoryStream();
    stream.CopyTo(memoryStream);
    memoryStream.ToArray()
(* Computation for sending email *)
let sendMail (from: string) (To: string) (subject: string) (body: string) = 
        let client = new SmtpClient(Host = "localhost", DeliveryMethod = SmtpDeliveryMethod.Network, Port = 25)
        let mm = new MailMessage()
        mm.From <- new MailAddress(from)
        mm.To.Add(To)
        mm.Subject <- subject
        mm.Body <- body
        client.SendAsync(mm,null)

//let server = HttpAgent.Start(url, fun server -> async {
//    while true do 
//        let! ctx = server.Receive()
//        let stream = ctx.Request.InputStream
//        let memStream = new MemoryStream()
//        let binForm = new BinaryFormatter()
//        let arrBytes = convert_stream_toBytes stream
//        memStream.Write(arrBytes, 0, arrBytes.Length)
//        memStream.Seek((0 |> int64), SeekOrigin.Begin) |> ignore
//        let obj: string = binForm.Deserialize(memStream) |> fun x -> x :?> string
//        sendMail "noreply@F#Agent.com" "cameron.frederick@gmail.com" "BackEnd SQL Message" obj
//        ctx.Response.Reply("Message Forwarded")    })

// Stop the HTTP server and release the port 8082
//server.Stop()

let signalR = SignalRAgent.Start(url, fun s -> async {
                                        while true do
                                            s.
                                            Clients.
                                        })



