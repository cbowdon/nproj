namespace NProj

module Project =

    open System
    open Microsoft.Build.Evaluation
    open NProj.Common

    type NProject = { ProjectFilePath: string
                      Properties: Map<string, string>
                      Items: SourceFile seq }

    let relativePath (project: Project) (path: string): string =
        let pathUri = Uri(path)
        let projUri = Uri(project.FullPath)
        let relUri = projUri.MakeRelativeUri(pathUri)
        relUri.ToString()

    let addProperty (msProj: Project) (key: string) (value: string): unit =
        msProj.SetProperty(key, value) |> ignore

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
        nProj.Properties |> Map.iter (addProperty msProj)
        nProj.Items |> Seq.iter (addItem msProj)
        msProj
