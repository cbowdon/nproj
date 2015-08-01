open NProj

[<EntryPoint>]
let main argv =
  try
    printfn "Arguments: %A" argv
    let x = IO.disk { let! fp = IO.fullPath "."
                      return fp }
    printfn "%A" x
    let x' = IO.interpret x
    printfn "%s" x'
    0
  with
    | ex -> printfn "Exception: %A" ex; 1
