namespace NProj.Test

module Add =
    open System
    open Xunit
    open NProj
    open NProj.Common

    let objectArrayOf (x: string list, y: Add.AddCommand): Object[] = [| x; y |]

    let parseAdd =
      Seq.map objectArrayOf [
          ( [ "one.fs"], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile ];
                           ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory });
          ( [ "one.fs"; "two.fs" ], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile; "/home/vagrant/Working/NProj/two.fs" |> uri |> Compile ];
                                      ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory });
          ( [ "one.fs"; "two.fs"; "--project"; "." ], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile; "/home/vagrant/Working/NProj/two.fs" |> uri |> Compile ];
                                                        ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory });
          ( [ "one.fs"; "two.fs"; "--project"; "../NProj.fsproj" ], { SourceFiles = [ "/home/vagrant/Working/NProj/one.fs" |> uri |> Compile; "/home/vagrant/Working/NProj/two.fs" |> uri |> Compile ];
                                                                      ProjectFile = "/home/vagrant/Working/NProj" |> uri |> Directory }); ]

    [<Theory>]
    [<MemberData("parseAdd")>]
    let ``Parse add arguments expect success`` (args: string seq) (expected: Add.AddCommand) =
        // Fixture setup
        // Exercise system
        let actual = Add.parse args
        // Verify outcome
        Assert.Equal(expected, actual)
