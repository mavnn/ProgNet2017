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
            let reader = CsvReader()
            let testInput = File.Open(testDataFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            Expect.equal (reader.Read testInput |> Seq.length) 4 "There are 4 rows in the test file"

        testCase "Load csv data accurately" <| fun () ->
            let reader = CsvReader()
            let testInput = File.Open(testDataFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            let first = reader.Read testInput |> Seq.head
            Expect.equal first.Name "bob" "first row has bob for name"

        testCase "Count sends" <| fun () ->
            let sut = CsvProcessor(CsvReader(), EmailSender())
            let testInput = File.Open(testDataFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            let count = sut.Process testInput
            Expect.equal count 2 "There are 2 valid distinct rows in the test data"
    ]

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
