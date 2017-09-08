module FunctionalSender.Sender

open System
open System.IO

type CsvRow =
    { name : string
      emailAddress : string
      message : string }

type EmailToSend =
    { address : string
      subject : string
      message : string }

let readCsv (csvData : #Stream) =
    let sr = new StreamReader(csvData)
    seq {
        while sr.Peek() >= 0 do
            let line = sr.ReadLine()
            let splits = line.Split(',') |> Array.map (fun s -> s.Trim())
            yield { name = splits.[0]; emailAddress = splits.[1]; message = splits.[2] }
        sr.Dispose()
    }
    // We'll skip the title row
    |> Seq.skip 1

let sendEmail emailToSend =
    if String.IsNullOrWhiteSpace emailToSend.address then
        printfn "Email send failed!"
        false
    else
        printfn "Email sent!\nTo: %s\nSubject: %s\nMessage: %s"
            emailToSend.address emailToSend.subject emailToSend.message
        true

let rowToEmail row =
    { address = row.emailAddress
      subject = sprintf "Hi %s!" row.name
      message = row.message }

let processCsv read send mapData input =
    read input
    |> Seq.distinct
    |> Seq.map mapData
    |> Seq.fold (fun count data -> if send data then count + 1 else count) 0
