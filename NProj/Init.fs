namespace NProj

module Init =

    open System
    open NProj.Common
    open NProj.LanguageTypes
    open NProj.IO
    open NProj.Project
    open NProj.Language

    type InitCommand = { ProjectDirectory: Directory
                         Lang: Language
                         Type: AssemblyType }

    let defaultInit =
        disk { let! pfl = directory "."
               return { ProjectDirectory = pfl
                        Lang = fsharp
                        Type = Library } }

    let parseProjectFile (raw: Command) (cmd: FreeDisk<InitCommand>) =
        disk { let! cmd' = cmd
               match raw.Arguments with
               | [] -> return cmd'
               | [x] ->
                    let! pfl = directory x
                    return { cmd' with ProjectDirectory = pfl }
               | _ -> return failwith "Too many arguments specified; expected one." }

    let parseLang (raw: Command) (cmd: FreeDisk<InitCommand>) =
        let parseLang' cmd' =
            match Map.tryFind "--lang" raw.Options with
            | None -> cmd'
            | Some None -> failwith "The flag \"--lang\" requires an argument"
            | Some (Some x) ->
                match Language.parse x with
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

    let createAssemblyInfo (cmd: InitCommand): FreeDisk<unit> =
        disk { let lang = cmd.Lang
               let! template = lang.AssemblyInfoTemplate |> readFile
               let (Directory dir) = cmd.ProjectDirectory
               let name = System.IO.Path.GetFileName dir
               let path = lang.SourceExtension |> sprintf "%s/AssemblyInfo.%s" dir
               let content = String.Format(template, name)
               printfn "Creating file at %s" path
               return! writeFile path content }

    let execute (cmd: InitCommand): FreeDisk<unit> =
        let (Directory dir) = cmd.ProjectDirectory
        let name = System.IO.Path.GetFileName dir

        let defaultProject =
            match cmd.Lang.Name with
            | CSharp -> CSharp.defaultProject
            | FSharp -> FSharp.defaultProject

        let project = defaultProject cmd.Type dir name

        disk { do! createAssemblyInfo cmd
               printfn "Creating project: %A" project
               do! Project.create project }
