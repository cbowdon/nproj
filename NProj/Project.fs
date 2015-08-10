namespace NProj

module Project =

    open System
    open Microsoft.Build.Evaluation
    open NProj.Common

    type PropertyGroup = { Properties: Map<string, string>
                           Condition: string option }

    type NProject = { ProjectFilePath: string
                      PropertyGroups: PropertyGroup seq
                      Items: SourceFile seq }

    type Language =
    | CSharp
    | FSharp with
      member x.Extension: string =
        match x with
        | CSharp -> "csproj"
        | FSharp -> "fsproj"
      static member Parse (x: string): Language option =
        match x.ToLowerInvariant() with
        | "csharp" -> Some CSharp
        | "fsharp" -> Some FSharp
        | _ -> None

    type AssemblyType =
    | Exe
    | Library with
      static member Parse (x: string): AssemblyType option =
        match x.ToLowerInvariant() with
        | "exe" -> Some Exe
        | "console" -> Some Exe
        | "lib" -> Some Library
        | "library" -> Some Library
        | _ -> None

    let relativePath (project: Project) (path: string): string =
        let pathUri = Uri(path)
        let projUri = Uri(project.FullPath)
        let relUri = projUri.MakeRelativeUri(pathUri)
        relUri.ToString()

    let addPropertyGroup (msProj: Project) (propertyGroup: PropertyGroup): unit =
        let pg = msProj.Xml.AddPropertyGroup()
        match propertyGroup.Condition with
        | None -> ()
        | Some x -> pg.Condition <- x
        propertyGroup.Properties
        |> Map.iter (fun k v -> pg.AddProperty(k, v) |> ignore)

    let addItem (msProj: Project) (item: SourceFile): unit =
        let rp = relativePath msProj
        match item with
        | ProjectReference x -> msProj.AddItem("ProjectReference", rp x) |> ignore
        | Reference x -> msProj.AddItem("Reference", x) |> ignore
        | Compile x -> msProj.AddItem("Compile", rp x) |> ignore
        | Content x -> msProj.AddItem("Content", rp x) |> ignore
        | Import x ->
            let import = msProj.Xml.AddImport(x)
            import.Condition <- sprintf "Exists('%s')" (x)
        msProj.Save()

    let create (nProj: NProject): Project =
        let msProj = Project()
        msProj.FullPath <- nProj.ProjectFilePath
        msProj.Xml.DefaultTargets <- "Build"
        nProj.PropertyGroups |> Seq.iter (addPropertyGroup msProj)
        nProj.Items |> Seq.iter (addItem msProj)
        msProj

    let minimalProject (lang: Language) (outputType: AssemblyType) (directory: string) (name: string): NProject =

        { ProjectFilePath = System.IO.Path.Combine(directory, lang.Extension |> sprintf "%s.%s" name)

          PropertyGroups = [ { Condition = None
                               Properties = Map.ofSeq [ ("Language", sprintf "%A" lang)
                                                        ("SchemaVersion", "2.0")
                                                        ("ProjectGuid", System.Guid.NewGuid().ToString())
                                                        ("OutputType", sprintf "%A" outputType)
                                                        ("Name", name)
                                                        ("RootNamespace", name)
                                                        ("AssemblyName", name)
                                                        ("TargetFrameworkVersion", "v4.5") ] } ]

          Items = [ Import @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"
                    Reference "mscorlib"
                    Reference "System"
                    Reference "System.Core"
                    Reference "System.Numerics" ] }
