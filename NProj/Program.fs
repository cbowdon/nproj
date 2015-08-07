open NProj

[<EntryPoint>]
let main (argv: string[]): int =
  try
    printfn "Arguments given: %A" argv
    let actions =
        match List.ofArray argv with
        | "init"::rest ->
            IO.disk { let! cmd = Init.parse rest
                      return! Init.execute cmd }
        | "add"::rest ->
            IO.disk { let! cmd = Add.parse rest
                      return! Add.execute cmd }
        | x::_ -> sprintf "Unrecognized command: %s" x |> failwith |> IO.Pure
        | [] -> failwith "No commands given" |> IO.Pure
    IO.interpret actions
    0
  with
    | ex -> printfn "Exception: %A" ex; 1
