module FunctionalTester.Test
open System.IO
open System.Reflection
open FunctionalSender.Sender
open Expecto

let here =
    Assembly.GetEntryAssembly().Location
    |> Path.GetDirectoryName

let testDataFile =
    Path.Combine(here, "testData.csv")

[<Tests>]
let tests =
    testList "Tests" [
        testCase "Load all csv rows" <| fun () ->
            use testInput = File.Open(testDataFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            Expect.equal (readCsv testInput |> Seq.length) 4 "There are 4 rows in the test file"

        testCase "Load csv data accurately" <| fun () ->
            use testInput = File.Open(testDataFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            let first = readCsv testInput |> Seq.head
            Expect.equal first.name "bob" "first row has bob for name"

        testCase "Test mapping" <| fun () ->
            let inputRow =
                { name = "Bob"
                  emailAddress = "bob@example.com"
                  message = "Hello Bob!" }
            let result =
                rowToEmail inputRow
            Expect.equal result.address inputRow.emailAddress "Email address is transferred unchanged"
            Expect.equal result.subject "Hi Bob!" "The name is used to create the subject"
            Expect.equal result.message inputRow.message "Use the message as is"

        testCase "Count sends" <| fun () ->
            let sut = processCsv readCsv sendEmail rowToEmail
            use testInput = File.Open(testDataFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            let count = sut testInput
            Expect.equal count 2 "There are 2 valid distinct rows in the test data"
    ]

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
