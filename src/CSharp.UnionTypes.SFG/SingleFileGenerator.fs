namespace CSharp.UnionTypes.SFG

open System
open System.Runtime.InteropServices

open Microsoft.VisualStudio
open Microsoft.VisualStudio.OLE.Interop
open Microsoft.VisualStudio.Shell
open Microsoft.VisualStudio.Shell.Interop

open VSLangProj80

open BrightSword.CSharpExtensions.DiscriminatedUnion

module VisualStudioIntegration =
    [<ComVisible(true)>]
    [<Guid("F15F1915-CDBD-46B1-9B35-72E3C04D9D0E")>]
    [<CodeGeneratorRegistration(typeof<SingleFileGenerator>, "C# Discriminated Union Class Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)>]
    [<ProvideObject(typeof<SingleFileGenerator>)>]
    type SingleFileGenerator () =
        let mutable site = Unchecked.defaultof<_>

        static member internal name = "csunion"
         
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

        interface Microsoft.VisualStudio.OLE.Interop.IObjectWithSite with
            member this.SetSite (pUnkSite: obj) =
                site <- pUnkSite;

            member this.GetSite (riid: Guid byref, ppvSite: nativeint byref) =
                if (isNull site) then raise (COMException("object is not sited", VSConstants.E_FAIL))
                
                let pUnknownPointer = Marshal.GetIUnknownForObject(site);
                Marshal.QueryInterface (pUnknownPointer, ref riid, ref ppvSite) |> ignore

                if (ppvSite = IntPtr.Zero) then raise (COMException("site does not support requested interface", VSConstants.E_NOINTERFACE));
