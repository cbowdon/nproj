namespace NProj

module Init =

    open Common
    open IO

    type InitCommand = { ProjectFile: ProjectFileLocation
                         Lang: Language
                         Type: AssemblyType }

    let defaultInit =
        disk { let! pfl = projectFileLocation "."
               return { ProjectFile = pfl
                        Lang = FSharp
                        Type = Library } }

    let parseProjectFile (raw: Command) (cmd: FreeDisk<InitCommand>) =
        disk { let! cmd' = cmd
               match raw.Arguments with
               | [] -> return cmd'
               | [x] ->
                    let! pfl = projectFileLocation "."
                    return { cmd' with ProjectFile = pfl }
               | _ -> return failwith "Too many arguments specified; expected one." }

    let parseLang (raw: Command) (cmd: FreeDisk<InitCommand>) =
        let parseLang' cmd' =
            match Map.tryFind "--lang" raw.Options with
            | None -> cmd'
            | Some None -> failwith "The flag \"--lang\" requires an argument"
            | Some (Some x) ->
                match Language.Parse x with
                | None -> failwith "Language not recognised: %s" x
                | Some lang -> { cmd' with Lang = lang }
        FreeDisk.liftM parseLang' cmd

    let parseType (raw: Command) (cmd: FreeDisk<InitCommand>) =
        let parseType' cmd' =
            match Map.tryFind "--type" raw.Options with
            | None -> cmd'
            | Some None -> failwith "The flag \"--type\" requires an argument"
            | Some (Some x) ->
                match AssemblyType.Parse x with
                | None -> failwith "AssemblyType not recognised: %s" x
                | Some at -> { cmd' with Type = at }
        FreeDisk.liftM parseType' cmd

    let parse (args: string seq): FreeDisk<InitCommand> =
        foldParsers [ parseProjectFile; parseLang; parseType ] defaultInit args

    let execute (cmd: InitCommand): unit = failwith "undefined"
