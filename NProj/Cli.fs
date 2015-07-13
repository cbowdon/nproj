namespace NProj.Cli

module Types =

    type Init = { Directory: string option
                  Language: string option
                  Type: string option }

    type Add = { Files: string seq
                 Project: string option }

module Parse =

    open Types

    let init (args: string seq): Init = failwith "undefined"

    let add (args: string seq): Add = failwith "undefined"
