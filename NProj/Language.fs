namespace NProj

module Language =

    open System
    open NProj.Common
    open NProj.Project

    type ILanguage =
        abstract member Name: string
        abstract member ProjectFileExtension: string
        abstract member SourceExtension: string
        abstract member AssemblyInfoTemplate: Uri
        abstract member SourceFileTemplate: Uri
        abstract member DefaultProject: AssemblyType -> string -> string -> NProject

    let minimalProject (lang: ILanguage) (outputType: AssemblyType) (directory: string) (name: string): NProject =

        { ProjectFilePath = System.IO.Path.Combine(directory, lang.ProjectFileExtension |> sprintf "%s.%s" name)

          PropertyGroups = [ { Condition = None
                               Properties = Map.ofSeq [ ("Language", lang.Name)
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

    type CSharpSpec() =
        interface ILanguage with
            member this.Name = "CSharp"
            member this.ProjectFileExtension = "csproj"
            member this.SourceExtension = "cs"
            member this.AssemblyInfoTemplate = Uri("Templates/CSharp/AssemblyInfo.cs")
            member this.SourceFileTemplate = Uri("Templates/CSharp/Class.cs")
            member this.DefaultProject outputType directory name =
                minimalProject this outputType directory name

    type FSharpSpec() =
        interface ILanguage with
            member this.Name = "FSharp"
            member this.ProjectFileExtension = "fsproj"
            member this.SourceExtension = "fs"
            member this.AssemblyInfoTemplate = Uri("Templates/FSharp/AssemblyInfo.fs")
            member this.SourceFileTemplate = Uri("Templates/FSharp/Module.fs")
            member this.DefaultProject outputType directory name =
                let project = minimalProject this outputType directory name
                let fsharpTargetsPath = "FSharpTargetsPath"
                let fsharpTargets = @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\FSharp\Microsoft.FSharp.Targets"
                let fsharpTargetsPg = { Condition = fsharpTargets |> sprintf "Exists('%s')" |> Some
                                        Properties = Map.ofSeq [ (fsharpTargetsPath, fsharpTargets) ] }

                let origPg = Seq.head project.PropertyGroups
                let origPg' = { origPg with Properties = Map.add "TargetFSharpCoreVersion" "4.3.0.0" origPg.Properties }

                let fsharpCore = "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"

                { project with
                      PropertyGroups = [ origPg'; fsharpTargetsPg ]
                      Items = Seq.append project.Items [ Reference fsharpCore;
                                                         fsharpTargetsPath |> sprintf "$(%s)" |> Import ] }

    type Language =
    | CSharp
    | FSharp with
        member this.Spec: ILanguage =
            match this with
            | CSharp -> CSharpSpec() :> ILanguage
            | FSharp -> FSharpSpec() :> ILanguage
        static member Parse (x: string): Language option =
            match x.ToLowerInvariant() with
            | "csharp" -> Some CSharp
            | "fsharp" -> Some FSharp
            | _ -> None
