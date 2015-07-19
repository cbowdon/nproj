namespace NProj

module Init =

    open Common

    type InitCommand = { ProjectFile: ProjectFileLocation
                         Lang: Language
                         Type: AssemblyType }

    let defaultInit = { ProjectFile = projectFileLocation "."
                        Lang = FSharp
                        Type = Library }

    let parseProjectFile (raw: Command) (cmd: InitCommand): InitCommand =
        match raw.Arguments with
        | [] -> cmd
        | [x] -> { cmd with ProjectFile = projectFileLocation x }
        | _ -> failwith "Too many arguments specified; expected one."

    let parseLang (raw: Command) (cmd: InitCommand): InitCommand =
        match Map.tryFind "--lang" raw.Options with
        | None -> cmd
        | Some None -> failwith "The flag \"--lang\" requires an argument"
        | Some (Some x) ->
            match Language.Parse x with
            | None -> failwith "Language not recognised: %s" x
            | Some lang -> { cmd with Lang = lang }

    let parseType (raw: Command) (cmd: InitCommand): InitCommand =
        match Map.tryFind "--type" raw.Options with
        | None -> cmd
        | Some None -> failwith "The flag \"--type\" requires an argument"
        | Some (Some x) ->
            match AssemblyType.Parse x with
            | None -> failwith "AssemblyType not recognised: %s" x
            | Some at -> { cmd with Type = at }

    let parse (args: string seq): InitCommand =
        let raw = collectArgs args
        [ parseProjectFile; parseLang; parseType ]
        |> Seq.fold (fun acc p -> p raw acc) defaultInit

    let execute (cmd: InitCommand): unit = failwith "undefined"
