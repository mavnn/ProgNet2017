module FunctionalSender.Sender

open System
open System.IO

type CsvRow (name : string, emailAddress : string, message : string) =
    member val Name = name with get
    member val EmailAddress = emailAddress with get
    member val Message = message with get

    override this.Equals (other : obj) =
        match other with
        | :? CsvRow as row ->
            (this :> IEquatable<_>).Equals row
        | _ ->
            false

    override this.GetHashCode() =
        this.Name.GetHashCode() * 3
          + this.EmailAddress.GetHashCode() * 5
          + this.Message.GetHashCode() * 7

    static member op_Equality (lhs : CsvRow, rhs : CsvRow) =
        if Object.ReferenceEquals(lhs, null) || Object.ReferenceEquals(rhs, null) then
            false
        else
            (lhs :> IEquatable<_>).Equals rhs

    static member op_BangEqual (lhs : CsvRow, rhs : CsvRow) =
        not (CsvRow.op_Equality(lhs, rhs))

    interface IEquatable<CsvRow> with
        member this.Equals (other : CsvRow) =
            if Object.ReferenceEquals (other, null) then
                false
            elif Object.ReferenceEquals (this, other) then
                true
            elif this.GetType() <> other.GetType() then
                false
            else
                this.Name = other.Name
                  && this.EmailAddress = other.EmailAddress
                  && this.Message = other.Message

type ICsvReader =
    abstract member Read : csvData : #Stream -> seq<CsvRow>

type CsvReader() =
    // Don't do this at home, use a CSV reader library
    member this.Read (csvData : #Stream) =
        let sr = new StreamReader(csvData)
        seq {
            while sr.Peek() >= 0 do
                let line = sr.ReadLine()
                let splits = line.Split(',') |> Array.map (fun s -> s.Trim())
                yield CsvRow(splits.[0], splits.[1], splits.[2])
            sr.Dispose()
        }
        // We'll skip the title row
        |> Seq.skip 1
    interface ICsvReader with
        member this.Read csvData = this.Read csvData

type IEmailSender =
    abstract member Send : emailAddress : string -> subject : string -> message : string -> bool

type EmailSender() =
    member this.Send emailAddress subject message =
        if String.IsNullOrWhiteSpace emailAddress then
            printfn "Email send failed!"
            false
        else
            printfn "Email sent!\nTo: %s\nSubject: %s\nMessage: %s"
                emailAddress subject message
            true
    interface IEmailSender with
        member this.Send emailAddress subject message =
            this.Send emailAddress subject message

type ICsvProcessor =
    abstract member Process : rows : #Stream -> int

type CsvProcessor(reader : ICsvReader, sender : IEmailSender) =
    member __.Process (inputStream : #Stream) =
        let rows = reader.Read inputStream
        let mutable count = 0
        for row in Seq.distinct rows do
            let subject = sprintf "Hi %s!" row.Name
            if sender.Send row.EmailAddress subject row.Message then
                count <- count + 1
        count
    interface ICsvProcessor with
        member this.Process inputStream =
            this.Process inputStream
