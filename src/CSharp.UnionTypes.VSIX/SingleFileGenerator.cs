using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    /// <summary>
    ///     This is the generator class.
    ///     When setting the 'Custom Tool' property of a C#, VB, or J# project item to "XmlClassGenerator",
    ///     the GenerateCode function will get called and will return the contents of the generated file
    ///     to the project system
    /// </summary>
    [ComVisible(true)]
    [Guid("E15F1915-CDBD-46B1-9B35-72E3C04D9D0E")]
    [CodeGeneratorRegistration(typeof (CSharpUnionGenerator), "C# Discriminated Union Class Generator",
        vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof (CSharpUnionGenerator))]
    public class CSharpUnionGenerator : IVsSingleFileGenerator,
        IObjectWithSite
    {
#pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        // ReSharper disable once InconsistentNaming
        internal static string name = "csunion_tool";
#pragma warning restore 0414
        private readonly TextWriter _log;

        private object _site;

        public CSharpUnionGenerator()
        {
            _log = new StreamWriter(@"C:\csunion_tool_log", true) {AutoFlush = true};
        }

        protected string FileNameSpace { get; private set; } = string.Empty;

        protected string InputFilePath { get; private set; } = string.Empty;

        internal IVsGeneratorProgress CodeGeneratorProgress { get; private set; }

        void IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (_site == null)
            {
                throw new COMException("object is not sited", VSConstants.E_FAIL);
            }

            var pUnknownPointer = Marshal.GetIUnknownForObject(_site);

            IntPtr intPointer;
            Marshal.QueryInterface(pUnknownPointer, ref riid, out intPointer);

            if (intPointer == IntPtr.Zero)
            {
                throw new COMException("site does not support requested interface", VSConstants.E_NOINTERFACE);
            }

            ppvSite = intPointer;
        }

        void IObjectWithSite.SetSite(object pUnkSite)
        {
            _site = pUnkSite;
        }

        /// <summary>
        ///     Implements the IVsSingleFileGenerator.DefaultExtension method.
        ///     Returns the extension of the generated file
        /// </summary>
        /// <param name="pbstrDefaultExtension">
        ///     Out parameter, will hold the extension that is to be given to the output file name.
        ///     The returned extension must include a leading period
        /// </param>
        /// <returns>S_OK if successful, E_FAIL if not</returns>
        int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".g.cs";
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Implements the IVsSingleFileGenerator.Generate method.
        ///     Executes the transformation and returns the newly generated output file, whenever a custom tool is loaded, or the
        ///     input file is saved
        /// </summary>
        /// <param name="wszInputFilePath">
        ///     The full path of the input file. May be a null reference (Nothing in Visual Basic) in
        ///     future releases of Visual Studio, so generators should not rely on this value
        /// </param>
        /// <param name="bstrInputFileContents">
        ///     The contents of the input file. This is either a UNICODE BSTR (if the input file is
        ///     text) or a binary BSTR (if the input file is binary). If the input file is a text file, the project system
        ///     automatically converts the BSTR to UNICODE
        /// </param>
        /// <param name="wszDefaultNamespace">
        ///     This parameter is meaningful only for custom tools that generate code. It represents
        ///     the namespace into which the generated code will be placed. If the parameter is not a null reference (Nothing in
        ///     Visual Basic) and not empty, the custom tool can use the following syntax to enclose the generated code
        /// </param>
        /// <param name="rgbOutputFileContents">
        ///     [out] Returns an array of bytes to be written to the generated file. You must
        ///     include UNICODE or UTF-8 signature bytes in the returned byte array, as this is a raw stream. The memory for
        ///     rgbOutputFileContents must be allocated using the .NET Framework call,
        ///     System.Runtime.InteropServices.AllocCoTaskMem, or the equivalent Win32 system call, CoTaskMemAlloc. The project
        ///     system is responsible for freeing this memory
        /// </param>
        /// <param name="pcbOutput">[out] Returns the count of bytes in the rgbOutputFileContent array</param>
        /// <param name="pGenerateProgress">
        ///     A reference to the IVsGeneratorProgress interface through which the generator can
        ///     report its progress to the project system
        /// </param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns E_FAIL</returns>
        int IVsSingleFileGenerator.Generate(string wszInputFilePath,
            string bstrInputFileContents,
            string wszDefaultNamespace,
            IntPtr[] rgbOutputFileContents,
            out uint pcbOutput,
            IVsGeneratorProgress pGenerateProgress)
        {
            Log("Started generating");
            if (bstrInputFileContents == null)
            {
                throw new ArgumentNullException(nameof(bstrInputFileContents));
            }

            InputFilePath = wszInputFilePath;
            FileNameSpace = wszDefaultNamespace;
            CodeGeneratorProgress = pGenerateProgress;

            Log($"Input: {bstrInputFileContents}");
            var code = CodeGenerator.generate_code_for_text(bstrInputFileContents);
            Log($"Output: {code}");

            var bytes = Encoding.UTF8.GetBytes(code);

            var outputLength = bytes.Length;
            Log($"Output Length: {outputLength}");

            rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
            Marshal.Copy(bytes, 0, rgbOutputFileContents[0], outputLength);
            pcbOutput = (uint) outputLength;

            Log("Completed generating");
            return VSConstants.S_OK;
        }

        private void Log(string format, params object[] args)
            => _log.WriteLine($"{DateTime.UtcNow}: {string.Format(format, args)}");

        /// <summary>
        ///     Method that will communicate an error via the shell callback mechanism
        /// </summary>
        /// <param name="level">Level or severity</param>
        /// <param name="message">Text displayed to the user</param>
        /// <param name="line">Line number of error</param>
        /// <param name="column">Column number of error</param>
        protected virtual void GeneratorError(uint level, string message, uint line, uint column)
        {
            CodeGeneratorProgress?.GeneratorError(0, level, message, line, column);
        }

        /// <summary>
        ///     Method that will communicate a warning via the shell callback mechanism
        /// </summary>
        /// <param name="level">Level or severity</param>
        /// <param name="message">Text displayed to the user</param>
        /// <param name="line">Line number of warning</param>
        /// <param name="column">Column number of warning</param>
        protected virtual void GeneratorWarning(uint level, string message, uint line, uint column)
        {
            CodeGeneratorProgress?.GeneratorError(1, level, message, line, column);
        }
    }

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class CSharpUnionTypesPackage : Package
    {
        public const string PackageGuidString = "aeb9145c-bb3f-4ab1-8d8b-7b0b95c73f39";
    }
}