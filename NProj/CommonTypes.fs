namespace NProj

open System

type Language =
  | CSharp
  | FSharp

type AssemblyType =
  | Exe
  | Library

type ProjectFile = { Location: Uri }

type SourceFile = { Location: Uri }
