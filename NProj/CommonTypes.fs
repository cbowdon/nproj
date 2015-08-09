namespace NProj

module Common =

    open System
    open System.Linq
    open NProj.IO

    type ProjectFileLocation = Directory of string

    type SourceFile =
    | Compile of string
    | Content of string
    | Reference of string
    | ProjectReference of string
    | Import of string

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
            | Flag x::Flag y::rest -> coll (Flag y::rest) { result with Options = Map.add x None result.Options }
            | Flag x::Parameter y::rest -> coll rest { result with Options = Map.add x (Some y) result.Options }
            | Flag x::rest -> coll rest { result with Options = Map.add x None result.Options }
            | Parameter x::rest -> coll rest { result with Arguments = x::result.Arguments }
        coll typedArgs { Arguments = []; Options = Map.empty }

    type Parser<'a> = Command -> FreeDisk<'a> -> FreeDisk<'a>

    let foldParsers (parsers: seq<Parser<'a>>) (defolt: FreeDisk<'a>) (args: string seq): FreeDisk<'a> =
        let raw = collectArgs args
        parsers |> Seq.fold (fun acc p -> p raw acc) defolt

    // TODO doesn't support naming a new file
    let projectFileLocation (path: string): FreeDisk<ProjectFileLocation> =
        path
        |> fullPath
        |> FreeDisk.liftM Directory

    let sourceFile (path: string): FreeDisk<SourceFile> =
        disk { let! path' = fullPath path
               let! ext = extension path'
               return
                   match ext with
                   | ".dll" -> Reference path'
                   | ".targets" -> Import path'
                   | ".props" -> Import path'
                   | ".cs" -> Compile path'
                   | ".fs" -> Compile path'
                   | ".csproj" -> ProjectReference path'
                   | ".fsproj" -> ProjectReference path'
                   | _ -> Content path' }
