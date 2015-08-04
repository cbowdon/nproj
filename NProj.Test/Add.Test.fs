namespace NProj.Test

module Add =
    open System
    open Xunit
    open NProj
    open NProj.IO
    open NProj.Common
    open NProj.Add

    let parseAdd: Object[] seq =
      [   ( [ "one.fs"], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile ];
                           ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory });
          ( [ "one.fs"; "two.fs" ], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile; "/home/vagrant/Working/NProj/two.fs" |> uri |> Compile ];
                                      ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory });
          ( [ "one.fs"; "two.fs"; "--project"; "." ], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile; "/home/vagrant/Working/NProj/two.fs" |> uri |> Compile ];
                                                        ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory });
          ( [ "one.fs"; "two.fs"; "--project"; "../NProj.fsproj" ], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile; "/home/vagrant/Working/NProj/two.fs" |> uri |> Compile ];
                                                                      ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory }); ]
      |> Seq.map (fun (x,y) -> [|x;y|]: Object[])

    [<Theory>]
    [<MemberData("parseAdd")>]
    let ``parse - expect success`` (args: string seq) (expected: AddCommand) =
        // Fixture setup
        // Exercise system
        let actual = parse args
        // Verify outcome
        Assert.Equal(Pure expected, actual)
