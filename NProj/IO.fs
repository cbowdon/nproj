namespace NProj

module IO =

    open System.IO
    open Microsoft.Build.Evaluation

    type Disk<'a> =
    | FullPath of string * (string -> 'a)
    | FileExists of string * (bool -> 'a)
    | DirectoryExists of string * (bool -> 'a)
    | Extension of string * (string -> 'a)
    | ListFiles of string * string option * (string seq -> 'a)
    | WriteProjectFile of Project * 'a
    | ReadFile of string * (string -> 'a)
    | WriteFile of string * string * 'a
    with
        static member fmap (f: 'a -> 'b) (x: Disk<'a>): Disk<'b> =
            match x with
            | FullPath (p, f') -> FullPath (p, f' >> f)
            | FileExists (p, f') -> FileExists (p, f' >> f)
            | DirectoryExists (p, f') -> DirectoryExists (p, f' >> f)
            | Extension (p, f') -> Extension (p, f' >> f)
            | ListFiles (p, pattern, f') -> ListFiles (p, pattern, f' >> f)
            | WriteProjectFile (proj, a) -> WriteProjectFile (proj, f a)
            | ReadFile (p, f') -> ReadFile (p, f' >> f)
            | WriteFile (p, content, a) -> WriteFile (p, content, f a)

    type FreeDisk<'a> =
    | Pure of 'a
    | Roll of Disk<FreeDisk<'a>>
    with
        static member bind (f: 'a -> FreeDisk<'b>) (x: FreeDisk<'a>): FreeDisk<'b> =
            match x with
            | Pure a -> f a
            | Roll a -> a |> Disk.fmap (FreeDisk.bind f) |> Roll

        static member lift (x: Disk<'a>): FreeDisk<'a> =
            x |> Disk.fmap Pure |> Roll

        static member liftM (f: 'a -> 'b) (m1: FreeDisk<'a>): FreeDisk<'b> =
            m1 |> FreeDisk.bind (f >> Pure)

        static member liftM2 (f: 'a -> 'b -> 'c) (m1: FreeDisk<'a>) (m2: FreeDisk<'b>): FreeDisk<'c> =
            let (>>=) m f' = FreeDisk.bind f' m
            m1 >>= (fun x1 ->
            m2 >>= (fun x2 ->
            f x1 x2 |> Pure))

        static member sequence (x: FreeDisk<'a> list): FreeDisk<'a list> =
            let cons y ys = y::ys
            match x with
            | [] -> Pure []
            | x::xs -> FreeDisk.sequence xs |> FreeDisk.liftM2 cons x

    type DiskBuilder() =
        member this.Bind(freeDisk, f) = FreeDisk.bind f freeDisk
        member this.Return(x) = Pure x
        member this.ReturnFrom(m) = m
        member this.Zero() = Pure ()
    let disk = DiskBuilder()

    let fullPath (path: string): FreeDisk<string> = FullPath (path, id) |> FreeDisk.lift
    let fileExists (path: string): FreeDisk<bool> = FileExists (path, id) |> FreeDisk.lift
    let directoryExists (path: string): FreeDisk<bool> = DirectoryExists (path, id) |> FreeDisk.lift
    let extension (path: string): FreeDisk<string> = Extension (path, id) |> FreeDisk.lift
    let listFiles (path: string) (pattern: string option): FreeDisk<string seq> = ListFiles (path, pattern, id) |> FreeDisk.lift
    let writeProjectFile (project: Project): FreeDisk<unit> = WriteProjectFile (project, ()) |> FreeDisk.lift
    let readFile (path: string): FreeDisk<string> = ReadFile (path, id) |> FreeDisk.lift
    let writeFile (path: string) (content: string): FreeDisk<unit> = WriteFile (path, content, ()) |> FreeDisk.lift

    let rec interpret (fd: FreeDisk<'a>): 'a =
        match fd with
        | Pure a -> a
        | Roll d ->
            match d with
            | FullPath (p, f) -> p |> Path.GetFullPath |> f |> interpret
            | FileExists (p, f) -> p |> File.Exists |> f |> interpret
            | DirectoryExists (p, f) -> p |> Directory.Exists |> f |> interpret
            | Extension (p, f) -> p |> Path.GetExtension |> f |> interpret
            | ListFiles (p, pattern, f) ->
                let pattern' =
                    match pattern with
                    | None -> "*"
                    | Some x -> x
                Directory.EnumerateFiles(p, pattern') |> f |> interpret
            | WriteProjectFile (proj, a) -> printfn "writing project file in dir: %A" proj.FullPath; proj.Save(); interpret a
            | ReadFile (p, f) -> p |> File.ReadAllText |> f |> interpret
            | WriteFile (p, content, a) -> File.WriteAllText(p, content); interpret a

