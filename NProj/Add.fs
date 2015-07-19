namespace NProj

module Add =
    open Common

    type AddCommand = { SourceFiles: SourceFile seq
                        ProjectFile: ProjectFileLocation }

    let defaultAdd = { SourceFiles = []
                       ProjectFile = projectFileLocation "." }

    let parseSourceFiles (raw: Command) (cmd: AddCommand) =
        { cmd with SourceFiles = raw.Arguments |> List.map sourceFile }

    let parseProjectFile (raw: Command) (cmd: AddCommand) =
        match Map.tryFind "--project" raw.Options with
        | None -> cmd
        | Some None -> failwith "The flag \"--project\" requires an argument"
        | Some (Some x) -> { cmd with ProjectFile = projectFileLocation x }

    let parse (args: string seq): AddCommand =
        let raw = collectArgs args
        [ parseSourceFiles; parseProjectFile ]
        |> Seq.fold (fun acc p -> p raw acc) defaultAdd

    let execute (cmd: AddCommand): unit = failwith "undefined"
