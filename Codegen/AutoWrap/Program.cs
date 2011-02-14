using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using AutoWrap.Meta;

namespace AutoWrap
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Globals.NativeNamespace = "Ogre";
            Globals.ManagedNamespace = "Mogre";

            MetaDefinition meta = new MetaDefinition(@"..\..\..\cpp2java\build\meta.xml", "Mogre");
            meta.AddAttributes(@"..\..\Attributes.xml");

            //check if auto directories exists, and create it if needed
            if (!Directory.Exists(@"..\..\..\..\Main\include\auto"))
                Directory.CreateDirectory(@"..\..\..\..\Main\include\auto");

            if (!Directory.Exists(@"..\..\..\..\Main\src\auto"))
                Directory.CreateDirectory(@"..\..\..\..\Main\src\auto");


            Wrapper wrapper = new Wrapper(meta, @"..\..\..\..\Main\include\auto", @"..\..\..\..\Main\src\auto", "Mogre", "Ogre");

            if (args.Length > 0)
            {
                // Command line mode - parse arguments.
                if (args.Length == 1 && args[0] == "produce")
                {
                    wrapper.IncludeFileWrapped += delegate(object sender, IncludeFileWrapEventArgs e)
                    {
                        Console.WriteLine(e.IncludeFile);
                    };
                    wrapper.ProduceCodeFiles();
                }
                else
                {
                    Console.Write(
                        "Invalid command\n\n" +
                        "Usage: AutoWrap.exe <command>\n\n" +
                        "Supported Commands\n" +
                        "==================\n\n" +
                        "    produce    Produces Mogre auto-generated files (equivalent to pressing the \"Produce\" button in the GUI).\n" +
                        "\n"
                    );
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(wrapper));
            }
        }
    }
}