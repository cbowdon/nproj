namespace NProj.Domain

module Types =

    open System

    type Language =
      | CSharp
      | FSharp

    type AssemblyType =
      | Library
      | Exe

    type SourceFile = { Location: Uri
                        Language: Language }

    type ProjectFile = { Location: Uri
                         Language: Language }

    type Add = { Files: SourceFile seq
                 Project: ProjectFile }

    type Init = { Project: ProjectFile
                  Language: Language
                  Type: AssemblyType }
