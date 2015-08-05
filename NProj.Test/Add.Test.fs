namespace NProj.Test

module Add =
    open System
    open Xunit
    open NProj
    open NProj.IO
    open NProj.Common
    open NProj.Add

    let parseAdd: Object[] seq =
      [   ( [ "one.fs"], { SourceFiles = [ Compile "/home/vagrant/Working/NProj/one.fs" ];
                           ProjectFile = Directory "/home/vagrant/Working/NProj" });
          ( [ "one.fs"; "two.fs" ], { SourceFiles = [ Compile "/home/vagrant/Working/NProj/one.fs"; Compile "/home/vagrant/Working/NProj/two.fs" ];
                                      ProjectFile = Directory "/home/vagrant/Working/NProj" });
          ( [ "one.fs"; "two.fs"; "--project"; "." ], { SourceFiles = [ Compile "/home/vagrant/Working/NProj/one.fs"; Compile "/home/vagrant/Working/NProj/two.fs" ];
                                                        ProjectFile = Directory "/home/vagrant/Working/NProj" });
          ( [ "one.fs"; "two.fs"; "--project"; "../NProj.fsproj" ], { SourceFiles = [ Compile "/home/vagrant/Working/NProj/one.fs"; Compile "/home/vagrant/Working/NProj/two.fs" ];
                                                                      ProjectFile = Directory "/home/vagrant/Working/NProj" }); ]
      |> Seq.map (fun (x,y) -> [|x;y|]: Object[])

    [<Theory>]
    [<MemberData("parseAdd")>]
    let ``parse - expect success`` (args: string seq) (expected: AddCommand) =
        // Fixture setup
        // Exercise system
        let actual = parse args
        // Verify outcome
        Assert.Equal(Pure expected, actual)
