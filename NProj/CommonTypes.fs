namespace NProj

module Common =

    open System
    open System.Linq

    type Language =
    | CSharp
    | FSharp

    type AssemblyType =
    | Exe
    | Library

    type ProjectFile = { Location: Uri }

    type SourceFile = { Location: Uri }

    type Command = { Arguments: string list
                     Options: Map<string,string option> }

    type ArgumentType = Flag of string | Parameter of string

    let collectArgs (args: string seq): Command =
        let typedArgs =
            args
            |> Seq.map (fun a -> if a.StartsWith("-") then Flag a else Parameter a)
            |> List.ofSeq
        let rec coll (xs: ArgumentType list) (result: Command): Command =
            match xs with
            | [] -> result
            | Parameter x::rest -> coll rest { result with Arguments = x::result.Arguments }
            | Flag x::Flag y::rest -> coll (Flag y::rest) { result with Options = Map.add x None result.Options }
            | Flag x::Parameter y::rest -> coll rest { result with Options = Map.add x (Some y) result.Options }
        coll typedArgs { Arguments = []; Options = Map.empty }

    let projectFile (path: string): ProjectFile = failwith "undefined"
