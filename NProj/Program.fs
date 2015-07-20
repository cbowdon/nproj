open NProj

[<EntryPoint>]
let main argv =
  try
    printfn "Arguments: %A" argv
    // TEST
    let cmd = Add.parse [ "test.fs" ]
    printfn "%A" cmd
    Add.execute cmd
    0
  with
    | ex -> printfn "Exception: %A" ex; 1
