namespace NProj

module Project =

    open System
    open Microsoft.Build.Evaluation
    open NProj.Common
    open NProj.IO

    type PropertyGroup = { Properties: Map<string, string>
                           Condition: string option }

    type NProject = { ProjectFilePath: string
                      PropertyGroups: PropertyGroup seq
                      Items: SourceFile seq }

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

    module MSProj =

        open Microsoft.Build.Construction

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

        let isCompileGroup (itemGroup: ProjectItemGroupElement): bool =
            itemGroup.Items |> Seq.exists (fun i -> i.ItemType = "Compile")

        let addCompileItem (msProj: Project) (path: string): unit =
            let rp = relativePath msProj
            let compileGroups = msProj.Xml.ItemGroups |> Seq.filter isCompileGroup
            match List.ofSeq compileGroups with
            | [] -> msProj.AddItem("Compile", rp path) |> ignore
            | x::_ ->
                 let items = x.Items
                 // TODO lang
                 // let lang = msProj.FullPath |> System.IO.Path.GetExtension |> Language.fromExtension
                 x.RemoveAllChildren()
                 seq { yield x.AddItem("Compile", rp path)
                       for i in items do
                           yield x.AddItem("Compile", i.Include) } |> Seq.toList |> ignore

        let addItem (msProj: Project) (item: SourceFile): unit =
            let rp = relativePath msProj
            match item with
            | ProjectReference x -> msProj.AddItem("ProjectReference", rp x) |> ignore
            | Reference x -> msProj.AddItem("Reference", x) |> ignore
            | Content x -> msProj.AddItem("Content", rp x) |> ignore
            | Import x ->
                let import = msProj.Xml.AddImport(x)
                import.Condition <- sprintf "Exists('%s')" (x)
            | Compile x -> addCompileItem msProj x

    let create (nProj: NProject): FreeDisk<unit> =
        let msProj = Project()
        msProj.FullPath <- nProj.ProjectFilePath
        msProj.Xml.DefaultTargets <- "Build"
        msProj.Xml.ToolsVersion <- "4.0"
        nProj.PropertyGroups |> Seq.iter (MSProj.addPropertyGroup msProj)
        nProj.Items |> Seq.iter (MSProj.addItem msProj)
        writeProjectFile msProj

    let add (nProj: NProject): FreeDisk<unit> =
        let msProj = Project(nProj.ProjectFilePath)
        nProj.Items |> Seq.iter (MSProj.addItem msProj)
        nProj.PropertyGroups |> Seq.iter (MSProj.addPropertyGroup msProj)
        writeProjectFile msProj
