#I "packages/FSharp.Compiler.Service.1.4.0.1/lib/net45/"
#r "FSharp.Compiler.Service.dll"

open System.IO
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.SourceCodeServices

let checker = FSharpChecker.Create()

let file = "NProj/IO.fs"
let source = File.ReadAllText file
let projFile = "NProj/NProj.fsproj"
let ast = async { let opts = checker.GetProjectOptionsFromProjectFile(projFile)
                  let! results = checker.ParseFileInProject(file, source, opts)
                  match results.ParseTree with
                  | None -> return failwith "error"
                  | Some x -> return x } |> Async.RunSynchronously

type CodeUnitType =
| File
| Module
| Namespace

type CodeUnit = { Type: CodeUnitType
                  Name: string
                  Dependencies: CodeUnit seq
                  SubModules: CodeUnit seq }

let rec visitDecl (decl: SynModuleDecl) =
    match decl with
    | SynModuleDecl.Open (LongIdentWithDots(s,_), _) -> printfn "Open %A" s
    | SynModuleDecl.NestedModule (c, decls, _, _) ->
        let (ComponentInfo(_,_,_,li,_,_,_,_)) = c
        printfn "NestedModule %A" li
        Seq.iter visitDecl decls
    | _ -> ()

let rec visitModules (mns: SynModuleOrNamespace list) =
    for mn in mns do
        let (SynModuleOrNamespace(li, isMod, decls, xml, attrs, _, m)) = mn
        if isMod
        then
            printfn "Module %A" li
        else
            printfn "Namespace %A" li
        for decl in decls do
            visitDecl decl
    printfn "Done"

let buildModuleTree (ast: ParsedInput): CodeUnit =
    match ast with
    | ParsedInput.ImplFile(implFile) ->
        let (ParsedImplFileInput (filename, script, name, _, _, modules , _)) = implFile
        visitModules modules
        let deps = []
        let subs = []
        { Type = File
          Name = filename
          Dependencies = deps
          SubModules = subs }
    | _ -> failwith "TODO"

let res = buildModuleTree ast
