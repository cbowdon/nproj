open NProj

[<EntryPoint>]
let main argv =
  try
    printfn "Arguments: %A" argv
    // TEST - by ruining this program's own build XD
    let cmd = Add.parse [ "test.fs" ]
    printfn "%A" cmd
    Add.execute cmd
    0
  with
    | ex -> printfn "Exception: %A" ex; 1
