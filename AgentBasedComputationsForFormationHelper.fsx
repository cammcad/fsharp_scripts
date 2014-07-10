open System
open System.Net
open System.Net.Mail
open System.Web.Security

(* Computation for sending email *)
let sendMail (from: string) (To: string) (subject: string) (body: string) = 
        let client = new SmtpClient(Host = "localhost", DeliveryMethod = SmtpDeliveryMethod.Network, Port = 25)
        let mm = new MailMessage()
        mm.From <- new MailAddress(from)
        mm.To.Add(To)
        mm.Subject <- subject
        mm.Body <- body
        client.Send mm

(* E-Mail Agent, responsible for sending email *)
let MailerAgent = MailboxProcessor<_>.Start(fun s -> async {
        while true do
            let! msg = s.Receive()
            match msg with
            | "Cameron" -> 
                   sendMail "noreply@formationhelper.com" "cameron.frederick@gmail.com" "testing mailer agent" "Hello Cameron"
            | "Rachel" ->
                   sendMail "noreply@formationhelper.com" "rachel.frederick@gmail.com" "testing mailer agent" "Hello Rachel"
            | "Katie" ->
                    sendMail "noreply@formationhelper.com" "katiebug0826@live.com" "testing mailer agent" "Hello Katheryn"
            | _ -> printfn "Can't send email to %s" msg
        })



(* Paypal processor Agent *)
// steps
// Step 1 - Get query string variables which represent the customer purchase (store the variables in a record type)
// Step 2 - append &cmd=_notify-validate variable/value to the query string and post query string back to paypal
// Step 3 - Paypal will send back in the response a single word in the body of the message VERIFIED or INVALID
          //pattern match on this status - for verified case we want to check the IPN data 
          // a) make sure the payment_status = Completed
          // b) check the txn_id against the txn_id in the record type from the first post from paypal (should be the same)
          // c) check that the receiver_email is my registered email in my paypal account
          // d) check that mc_gross & mc_currency are correct for the item carried in variables item_name & item_number
          // e) notification validation is complete (IPN done!)
          // f) take the record info and start our process - generate a password, create a user profile and store 
          //   it in mongoDB, then post info to mailer agent to send email to new customer