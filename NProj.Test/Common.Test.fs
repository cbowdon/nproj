namespace NProj.Test

module Common =
    open System
    open Xunit
    open NProj
    open NProj.Common

    let objectArrayOf (args: string list, cmd: Command): Object[] = [| args; cmd |]

    let rawCommandParsing: Object[] seq =
        Seq.map objectArrayOf [
            ([ "." ], { Arguments = ["."];
                        Options = Map.empty });
            ([ "."; "--type"; "lib" ], { Arguments = ["."];
                                         Options = Map.ofSeq [("--type", Some "lib")] });
            ([ "."; "--lang"; "fsharp"; "--type"; "lib" ], { Arguments = ["."];
                                                             Options = Map.ofSeq [("--lang", Some "fsharp"); ("--type", Some "lib")] });
            ([ "one.fs" ], { Arguments = ["one.fs"];
                             Options = Map.empty });
            ([ "one.fs"; "two.fs" ], { Arguments = ["two.fs"; "one.fs"];
                                       Options = Map.empty });
            ([ "one.fs"; "two.fs"; "--project"; "numbers.fsproj" ], { Arguments = ["two.fs"; "one.fs"];
                                                                      Options = Map.ofSeq [("--project", Some "numbers.fsproj")] });
            ([ "one.fs"; "two.fs"; "--link" ], { Arguments = ["two.fs"; "one.fs"];
                                                 Options = Map.ofSeq [("--link", None)] });
            ([ "one.fs"; "two.fs"; "--link"; "--project"; "numbers.fsproj" ], { Arguments = ["two.fs"; "one.fs"];
                                                                                Options = Map.ofSeq [("--link", None); ("--project", Some "numbers.fsproj")] }) ]

    [<Theory>]
    [<MemberData("rawCommandParsing")>]
    let ``collectArgs - expect success`` (raw: string seq) (expected: Command) =
        // Fixture setup
        // Exercise system
        let result = collectArgs raw
        // Verify outcome
        Assert.Equal(expected, result)
