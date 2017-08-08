namespace CSharp.AlgebraicTypes

open System.Text.RegularExpressions
open System.IO

open Utility.Monads.MaybeMonad

[<AutoOpen>]
module CodeGenerator =
    let generate_code_for_text text =
        text |> (parseTextToNamespace >> to_namespace_declaration >> namespace_to_code)

    let mapTuple2 f (a, b) = (f a, f b)
    let trimWS (s: System.String) = s.Trim()
    let fixupNL t = Regex.Replace(t, "(?<!\r)\n", "\r\n")

    let ensure_input_file input_file =
        input_file  |> Option.filter (fun f -> File.Exists f) |> Option.map FileInfo

    let ensure_output_directory output_file =
        output_file |> Option.map (FileInfo >> (fun fi -> fi.Directory.Create (); fi))

    let generate_code_for_csunion_file (input_file, output_file) =
        maybe {
            let! inputFileInfo = ensure_input_file input_file
            let! outputFileInfo = ensure_output_directory output_file
            let text = File.ReadAllText inputFileInfo.FullName
            let! ns = parse_namespace_from_text text
            let code = ns |> (to_namespace_declaration >> namespace_to_code >> trimWS >> fixupNL)
            do File.WriteAllText (outputFileInfo.FullName, code)
            return ()
        }
