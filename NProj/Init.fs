namespace NProj

module Init =

    open System
    open Microsoft.Build.Evaluation
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

    let name (project: ProjectFileLocation): string =
        match project with
        | Directory x -> x.AbsolutePath |> System.IO.Path.GetDirectoryName
        | File x -> x.AbsolutePath |> System.IO.Path.GetFileNameWithoutExtension

    let execute (cmd: InitCommand): unit =
        let name = name cmd.ProjectFile
        let proj = Project()
        proj.Xml.DefaultTargets <- "Build"
        // Imports
        let commonPropImport = proj.Xml.AddImport("$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props")
        commonPropImport.Condition <- "Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"
        // Properties
        proj.SetProperty("Language", sprintf "%A" cmd.Lang) |> ignore
        proj.SetProperty("OutputType", sprintf "%A" cmd.Type) |> ignore
        proj.SetProperty("Name", name) |> ignore
        proj.SetProperty("RootNamespace", name) |> ignore
        proj.SetProperty("AssemblyName", name) |> ignore
        proj.SetProperty("SchemaVersion", "2.0") |> ignore
        proj.SetProperty("ProjectGuid", Guid.NewGuid() |> sprintf "{%A}") |> ignore
        // Default references
        proj.AddItem("Reference", "mscorlib") |> ignore
        proj.AddItem("Reference", "System") |> ignore
        proj.AddItem("Reference", "System.Core") |> ignore
        proj.AddItem("Reference", "System.Numerics") |> ignore
        if cmd.Lang = FSharp
        then
            // Properties
            proj.SetProperty("TargetFrameworkVersion", "v4.0") |> ignore
            proj.SetProperty("TargetFSharpCoreVersion", "4.3.0.0") |> ignore
            // References
            proj.AddItem("Reference", "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") |> ignore
            // Targets
            let pg = proj.Xml.AddPropertyGroup()
            pg.Condition <- @"Exists('$(MSBuildExtensionsPath32)\..\Microsoft SDKs\F#\3.0\Framework\v4.0\Microsoft.FSharp.Targets')"
            pg.AddProperty("FSharpTargetsPath", @"$(MSBuildExtensionsPath32)\..\Microsoft SDKs\F#\3.0\Framework\v4.0\Microsoft.FSharp.Targets") |> ignore
            let fsTargetsImport = proj.Xml.AddImport("$(FSharpTargetsPath)")
            fsTargetsImport.Condition <- "Exists('$(FSharpTargetsPath)')"
        else ()
        let saveLocation =
            match cmd.ProjectFile with
            | File x -> x
            | Directory x -> Uri(x, cmd.Lang.Extension |> sprintf "%s.%s" name)
        proj.Save(saveLocation.AbsolutePath)
