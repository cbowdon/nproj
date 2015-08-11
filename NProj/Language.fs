namespace NProj

module Language =

    open System
    open NProj.Common
    open NProj.Project

    type ILanguage =
        abstract member Extension: string
        abstract member AssemblyInfoTemplate: Uri
        abstract member SourceFileTemplate: Uri
        abstract member DefaultProject: AssemblyType -> string -> string -> NProject

    type CSharp() =
        interface ILanguage with
            member this.Extension = "csproj"
            member this.AssemblyInfoTemplate = Uri("Templates/CSharp/AssemblyInfo.cs")
            member this.SourceFileTemplate = Uri("Templates/CSharp/Class.cs")
            member this.DefaultProject outputType directory name =
                minimalProject Language.CSharp outputType directory name

    type FSharp() =
        interface ILanguage with
            member this.Extension = "fsproj"
            member this.AssemblyInfoTemplate = Uri("Templates/FSharp/AssemblyInfo.fs")
            member this.SourceFileTemplate = Uri("Templates/FSharp/Module.fs")
            member this.DefaultProject outputType directory name =

                let project = minimalProject Language.FSharp outputType directory name
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

     let instance (lang: Language): ILanguage =
         match lang with
         | CSharp -> CSharp() :> ILanguage
         | FSharp -> FSharp() :> ILanguage
