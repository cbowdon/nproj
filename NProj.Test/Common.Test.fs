namespace NProj.Test

module Common =
    open System
    open Xunit
    open NProj
    open NProj.IO
    open NProj.Common

    let projectFileLocationData: Object[] seq =
        [ (".", "/home/vagrant/Working/NProj.Test/bin/Debug" |> uri |> Directory);
          ("/home/vagrant/Working/NProj", "/home/vagrant/Working/NProj" |> uri |> Directory);
          ("/home/vagrant/Working/NProj/NProj.fsproj", "/home/vagrant/Working/NProj/NProj.fsproj" |> uri |> File) ]
        |> Seq.map (fun (x,y) -> [| x; y |]: Object[])

    [<Theory>]
    [<MemberData("projectFileLocationData")>]
    let ``projectFileLocation - expect correct datatype`` (path: string) (expected: ProjectFileLocation) =
        // Fixture setup
        // Exercise system
        let result = projectFileLocation path
        // Verify outcome
        Assert.Equal(Pure expected, result)

    let sourceFileData: Object[] seq =
        [ ("one.cs", "one.cs" |> uri |> Compile);
          ("one.fs", "one.fs" |> uri |> Compile);
          ("one.csproj", "one.csproj" |> uri |> ProjectReference);
          ("one.fsproj", "one.fsproj" |> uri |> ProjectReference);
          ("one.dll", "one.dll" |> uri |> Reference);
          ("one.css", "one.css" |> uri |> Content);
          ("one.props", "one.props" |> uri |> Import);
          ("one.targets", "one.targets" |> uri |> Import); ]
        |> Seq.map (fun (x,y) -> [| x; y |]: Object[])

    // TODO this test is going to fail, because depends on System.IO.Path.GetFullPath
    [<Theory>]
    [<MemberData("sourceFileData")>]
    let ``sourceFile - expect correct datatype`` (path: string) (expected: SourceFile) =
        // Fixture setup
        // Exercise system
        let result = sourceFile path
        // Verify outcome
        Assert.Equal(Pure expected, result)

    let collectArgsData: Object[] seq =
        [   ([ "." ], { Arguments = ["."];
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
      |> Seq.map (fun (x,y) -> [|x;y|]: Object[])

    [<Theory>]
    [<MemberData("collectArgsData")>]
    let ``collectArgs - expect success`` (raw: string seq) (expected: Command) =
        // Fixture setup
        // Exercise system
        let result = collectArgs raw
        // Verify outcome
        Assert.Equal(expected, result)
