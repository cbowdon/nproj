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
