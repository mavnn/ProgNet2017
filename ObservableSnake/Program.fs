module ObservableSnake
open System
open System.Reactive.Linq
open FSharp.Control

let maxY = min 50 (Console.BufferHeight - 2)
let maxX = min 50 (Console.BufferWidth - 1)

type Coord =
    { X : int
      Y : int }

type GameData =
    { Turns : int
      Board : Map<Coord, char>
      PlayerPos : Coord }

type Directions =
    | Up
    | Down
    | Left
    | Right
    | Stay

let initialisePosition x y =
    let coord = { X = x; Y = y }
    match coord with
    | { X = 0; Y = _ }
    | { X = _; Y = 0 } ->
        coord, '#'
    | { X = x; Y = _ } when x = maxX ->
        coord, '#'
    | { X = _; Y = y } when y = maxY ->
        coord, '#'
    | _ ->
        coord, ' '

let initialBoard =
    [0..maxX]
    |> List.collect (fun x -> List.init (maxY + 1) (fun y -> initialisePosition x y))
    |> Map

let makeChangeSeq gameData =
    let boardUpdates =
        gameData.Board
        |> Map.toSeq
        |> Seq.map (fun (coord, item) -> if coord = gameData.PlayerPos then coord, '*' else coord, item)
    let turnCounter =
        sprintf "Turns: %d" gameData.Turns
        |> Seq.mapi (fun i c -> { X = i; Y = maxY + 1 }, c)
    Seq.concat [boardUpdates;turnCounter]

let movePlayer gameData direction =
    let oldPos = gameData.PlayerPos
    let newPos =
        match direction with
        | Up -> { oldPos with Y = max (oldPos.Y - 1) 0 }
        | Down -> { oldPos with Y = min (oldPos.Y + 1) maxY }
        | Left -> { oldPos with X = max (oldPos.X - 1) 0 }
        | Right -> { oldPos with X = min (oldPos.X + 1) maxX }
        | Stay -> oldPos
    let updatedBoard =
        if gameData.Board.[newPos] = '#' then
            gameData
        else
            { gameData with PlayerPos = newPos }
    { updatedBoard with Turns = gameData.Turns + 1 }

let start =
    { Board = initialBoard; Turns = 0; PlayerPos = { X = 2; Y = 10 } }

let gameTurns directions =
    directions
    |> Seq.scan movePlayer start

// Impure code starts here!
let rec inputUnfolder prev =
    if Console.KeyAvailable then
        let read = Console.ReadKey(true)
        match read.Key with
        | ConsoleKey.UpArrow ->
            Some (Up, Up)
        | ConsoleKey.DownArrow ->
            Some (Down, Down)
        | ConsoleKey.LeftArrow ->
            Some (Left, Left)
        | ConsoleKey.RightArrow ->
            Some (Right, Right)
        | _ ->
            Some (prev, prev)
    else
        Async.Sleep 1 |> Async.RunSynchronously
        inputUnfolder prev

let drawTurn gameData =
    makeChangeSeq gameData
    |> Seq.iter (fun (coord, item : char) ->
        Console.CursorLeft <- coord.X
        Console.CursorTop <- coord.Y
        Console.Write item)

let run () =
    // Draw the initial board state
    drawTurn start

    let directions = Seq.unfold inputUnfolder Stay

    gameTurns directions
    |> Seq.iter drawTurn

try
    Console.Clear()
    Console.CursorVisible <- false
    run ()
finally
    Console.CursorVisible <- true
