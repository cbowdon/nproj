namespace NProj

module Common =

    open System
    open System.Linq
    open NProj.IO

    type Language =
    | CSharp
    | FSharp with
        member x.Extension: string =
            match x with
            | CSharp -> "csproj"
            | FSharp -> "fsproj"

        static member Parse (x: string): Language option =
            match x.ToLowerInvariant() with
            | "csharp" -> Some CSharp
            | "fsharp" -> Some FSharp
            | _ -> None

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

    let uri path = Uri(path)

    type ProjectFileLocation =
    | Directory of Uri
    | File of Uri

    type SourceFile =
    | Compile of Uri
    | Content of Uri
    | Reference of Uri
    | ProjectReference of Uri
    | Import of Uri

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

    let projectFileLocation (path: string): FreeDisk<ProjectFileLocation> =
        disk { let! path' = fullPath path
               let! isFile = fileExists path'
               let! isDirectory = directoryExists path'
               if isFile
               then return path' |> uri |> File
               else
                   if isDirectory
                   then return path' |> uri |> Directory
                   else
                       return failwith "No such file or directory: %s" path' }

    let sourceFile (path: string): FreeDisk<SourceFile> =
        disk { let! path' = fullPath path
               let u = uri path'
               let! ext = extension path'
               return
                   match ext with
                   | ".dll" -> Reference u
                   | ".targets" -> Import u
                   | ".props" -> Import u
                   | ".cs" -> Compile u
                   | ".fs" -> Compile u
                   | ".csproj" -> ProjectReference u
                   | ".fsproj" -> ProjectReference u
                   | _ -> Content u }
