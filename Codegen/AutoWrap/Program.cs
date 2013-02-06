using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using AutoWrap.Meta;

namespace AutoWrap
{
    static class Program
    {
        private const string BASE_DIR = @"..\..\..\..\";
        
        private const string META_XML_FILE = BASE_DIR + @"Codegen\cpp2java\build\meta.xml";
        private const string ATTRIBUTES_FILE = BASE_DIR + @"Codegen\Attributes.xml";
        
        private const string INCLUDES_DEST_DIR = BASE_DIR + @"Main\include\auto";
        private const string SRC_DEST_DIR = BASE_DIR + @"Main\src\auto";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Globals.NativeNamespace = "Ogre";
            Globals.ManagedNamespace = "Mogre";

            MetaDefinition meta = new MetaDefinition(META_XML_FILE, "Mogre");
            meta.AddAttributes(ATTRIBUTES_FILE);

            //check if auto directories exists, and create it if needed
            if (!Directory.Exists(INCLUDES_DEST_DIR))
                Directory.CreateDirectory(INCLUDES_DEST_DIR);

            if (!Directory.Exists(SRC_DEST_DIR))
                Directory.CreateDirectory(SRC_DEST_DIR);


            Wrapper wrapper = new Wrapper(meta, INCLUDES_DEST_DIR, SRC_DEST_DIR, "Mogre", "Ogre");

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