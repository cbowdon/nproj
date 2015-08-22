namespace NProj

module Language =

    open System
    open NProj.Common
    open NProj.Project

    type LanguageName =
    | CSharp
    | FSharp

    type Language = { Name: LanguageName
                      ProjectExtension: string
                      SourceExtension: string
                      AssemblyInfoTemplate: string
                      SourceTemplate: string }

    let csharp = { Name = CSharp
                   ProjectExtension = "csproj"
                   SourceExtension = "cs"
                   AssemblyInfoTemplate = "Templates/CSharp/AssemblyInfo.cs"
                   SourceTemplate = "Templates/CSharp/Class.cs" }

    let fsharp = { Name = FSharp
                   ProjectExtension = "fsproj"
                   SourceExtension = "fs"
                   AssemblyInfoTemplate = "Templates/FSharp/AssemblyInfo.fs"
                   SourceTemplate = "Templates/FSharp/Module.fs" }

    let parse (x: string): Language option =
      match x.ToLowerInvariant() with
      | "csharp" -> Some csharp
      | "fsharp" -> Some fsharp
      | _ -> None

    let minimalProject (lang: Language) (outputType: AssemblyType) (directory: string) (name: string): NProject =

        { ProjectFilePath = System.IO.Path.Combine(directory, lang.ProjectExtension |> sprintf "%s.%s" name)

          PropertyGroups = [ { Condition = None
                               Properties = Map.ofSeq [ ("Language", sprintf "%A" lang.Name)
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

    let fromExtension (ext: string): Language option =
        let exts = seq { yield csharp.ProjectExtension, csharp
                         yield csharp.SourceExtension, csharp
                         yield fsharp.ProjectExtension, fsharp
                         yield fsharp.SourceExtension, fsharp }
        let res = exts |> Seq.tryFind (fun (e,_) -> ext = e)
        match res with
        | Some (_,l) -> Some l
        | None -> None

    module CSharp =
        let defaultProject outputType directory name =
            let proj = minimalProject csharp outputType directory name
            { proj with Items = seq { yield! proj.Items;
                                      yield Compile "AssemblyInfo.cs" } }

    module FSharp =
        let defaultProject outputType directory name =
            let project = minimalProject fsharp outputType directory name
            let fsharpTargetsPath = "FSharpTargetsPath"
            let fsharpTargets = @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\FSharp\Microsoft.FSharp.Targets"
            let fsharpTargetsPg = { Condition = fsharpTargets |> sprintf "Exists('%s')" |> Some
                                    Properties = Map.ofSeq [ (fsharpTargetsPath, fsharpTargets) ] }

            let origPg::otherPgs = List.ofSeq project.PropertyGroups
            let origPg' = { origPg with Properties = Map.add "TargetFSharpCoreVersion" "4.3.0.0" origPg.Properties }

            let fsharpCore = "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"

            { project with
                    PropertyGroups = seq { yield origPg'
                                           yield! otherPgs
                                           yield fsharpTargetsPg }
                    Items = seq { yield! project.Items
                                  yield Reference fsharpCore
                                  yield fsharpTargetsPath |> sprintf "$(%s)" |> Import
                                  yield sprintf "%s/AssemblyInfo.fs" directory |> Compile } }
