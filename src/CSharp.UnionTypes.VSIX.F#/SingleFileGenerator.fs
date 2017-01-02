namespace CSharp.UnionTypes.VSIX
open Microsoft.VisualStudio.Shell.Interop
open Microsoft.VisualStudio
open System.Runtime.InteropServices
open BrightSword.CSharpExtensions.DiscriminatedUnion
open System
open Microsoft.VisualStudio.Shell
open VSLangProj80


module VisualStudioIntegration =
    [<ComVisible(true)>]
    [<Guid("F15F1915-CDBD-46B1-9B35-72E3C04D9D0E")>]
    [<CodeGeneratorRegistration(typeof<SingleFileGenerator>, "C# Discriminated Union Class Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)>]
    [<ProvideObject(typeof<SingleFileGenerator>)>]
    type SingleFileGenerator () =
        interface IVsSingleFileGenerator with
            member this.DefaultExtension (pbstrDefaultExtension: string byref) : int =
                pbstrDefaultExtension <- ".g.cs";
                VSConstants.S_OK

            member this.Generate (_, bstrInputFileContents: string, _, rgbOutputFileContents: nativeint[], pcbOutput: uint32 byref, _) : int =
                let code = CodeGenerator.generate_code_for_text bstrInputFileContents
                let buf = System.Text.Encoding.UTF8.GetBytes code
                rgbOutputFileContents.[0] <- Marshal.AllocCoTaskMem buf.Length
                Marshal.Copy (buf, 0, rgbOutputFileContents.[0], buf.Length)
                pcbOutput <- (uint32) buf.Length;
                VSConstants.S_OK
