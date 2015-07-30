namespace NProj

module IO =

    type Disk<'a> =
    | FullPath of string * (string -> 'a)
    | FileExists of string * (bool -> 'a)
    | DirectoryExists of string * (bool -> 'a)
    with
        static member fmap (f: 'a -> 'b) (x: Disk<'a>): Disk<'b> =
            match x with
            | FullPath (p, f') -> FullPath (p, f' >> f)
            | FileExists (p, f') -> FileExists (p, f' >> f)
            | DirectoryExists (p, f') -> FileExists (p, f' >> f)

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

    type DiskBuilder() =
        member this.Bind(freeDisk, f) = FreeDisk.bind f freeDisk
        member this.Return(x) = Pure x
    let disk = DiskBuilder()

    let fullPath (path: string): FreeDisk<string> = FullPath (path, id) |> FreeDisk.lift
    let fileExists (path: string): FreeDisk<bool> = FileExists (path, id) |> FreeDisk.lift
    let directoryExists (path: string): FreeDisk<bool> = DirectoryExists (path, id) |> FreeDisk.lift

    (* example
    let x = disk { let! fp = fullPath "."
                   return fp }
                   *)
