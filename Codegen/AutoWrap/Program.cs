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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(wrapper));
        }
    }
}