namespace NProj

module Init =

    open NProj.Common
    open NProj.IO
    open NProj.Project

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
                    let! pfl = projectFileLocation x
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

    let projectName (project: ProjectFileLocation): string =
        match project with
        | Directory x -> System.IO.Path.GetDirectoryName x
        | File x -> System.IO.Path.GetFileNameWithoutExtension x

    let execute (cmd: InitCommand): FreeDisk<unit> =
        let name = projectName cmd.ProjectFile

        let project = { ProjectFilePath = match cmd.ProjectFile with
                                          | File x -> x
                                          | Directory x -> System.IO.Path.Combine(x, cmd.Lang.Extension |> sprintf "%s.%s" name)

                        Properties = Map.ofSeq [ ("Language", sprintf "%A" cmd.Lang)
                                                 ("OutputType", sprintf "%A" cmd.Type)
                                                 ("Name", name)
                                                 ("RootNamespace", name)
                                                 ("AssemblyName", name)
                                                 ("SchemaVersion", "2.0")
                                                 ("ProjectGuid", System.Guid.NewGuid().ToString())
                                                 ("TargetFrameworkVersion", "v4.0") ]

                        Items = [ Import @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"
                                  Reference "mscorlib"
                                  Reference "System"
                                  Reference "System.Core"
                                  Reference "System.Numerics" ] }

        // Add language specific items and properties
        let project' =
            match cmd.Lang with
            | CSharp -> project
            | FSharp ->
                { project with
                      Properties = project.Properties
                                   |> Map.add "TargetFSharpCoreVersion" "4.3.0.0"
                      Items = Seq.append project.Items [ Reference "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" ] }

        Project.create project' |> writeProjectFile
