using System;
using System.Runtime.InteropServices;
using System.Text;
using BrightSword.CSharpExtensions.DiscriminatedUnion;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace CSharp.UnionTypes.VSIX
{
    [ComVisible(true)]
    [Guid("F15F1915-CDBD-46B1-9B35-72E3C04D9D0E")]
    [CodeGeneratorRegistration(typeof(SingleFileGenerator), "SingleFileGenerator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(SingleFileGenerator))]
    internal class SingleFileGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        // ReSharper disable InconsistentNaming
        public static string name = "csunion";
        // ReSharper restore InconsistentNaming

        private object _site;

        void IObjectWithSite.SetSite(object pUnkSite)
        {
            _site = pUnkSite;
        }

        void IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (_site == null)
            {
                Marshal.ThrowExceptionForHR(VSConstants.E_NOINTERFACE);
            }

            var punk = Marshal.GetIUnknownForObject(_site);
            var hr = Marshal.QueryInterface(punk, ref riid, out ppvSite);
            Marshal.Release(punk);
            ErrorHandler.ThrowOnFailure(hr);
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
            if (bstrInputFileContents == null)
            {
                throw new ArgumentException(nameof(bstrInputFileContents));
            }

            var code = CodeGenerator.generate_code_for_text(bstrInputFileContents);
            var bytes = Encoding.UTF8.GetBytes(code);

            rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, rgbOutputFileContents[0], bytes.Length);
            pcbOutput = (uint)bytes.Length;
            return VSConstants.S_OK;
        }
    }
}