using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace CSharp.UnionTypes.CodeGenerator
{
    [ComVisible(true)]
    [Guid("F15F1915-CDBD-46B1-9B35-72E3C04D9D0E")]
    [CodeGeneratorRegistration(typeof (SingleFileGenerator), "C# Discriminated Union Class Generator",
        vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof (SingleFileGenerator))]
    internal class SingleFileGenerator : IVsSingleFileGenerator,
        IObjectWithSite
    {
        public static string name = "csunion";
        private object site;

        void IObjectWithSite.SetSite(object pUnkSite)
        {
            site = pUnkSite;
        }

        void IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (null == site)
            {
                throw new COMException("object is not sited", VSConstants.E_FAIL);
            }
            var pUnknownPointer = Marshal.GetIUnknownForObject(site);
            Marshal.QueryInterface(pUnknownPointer, ref riid, out ppvSite);

            if (ppvSite == IntPtr.Zero)
            {
                throw new COMException("site does not support requested interface", VSConstants.E_NOINTERFACE);
            }
        }

        int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".g.cs";
            return VSConstants.S_OK;
        }

        int IVsSingleFileGenerator.Generate(string wszInputFilePath,
            string bstrInputFileContents,
            string wszDefaultNamespace,
            IntPtr[] rgbOutputFileContents,
            out uint pcbOutput,
            IVsGeneratorProgress pGenerateProgress)
        {
            var code = BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator.generate_code_for_text(bstrInputFileContents);
            var buf = Encoding.UTF8.GetBytes(code);
            rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(buf.Length);
            Marshal.Copy(buf, 0, rgbOutputFileContents[0], buf.Length);
            pcbOutput = (uint) buf.Length;
            return VSConstants.S_OK;
        }
    }
}