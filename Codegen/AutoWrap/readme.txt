﻿AutoWrapper
===========
This tool generates C++/CLI wrapper code from C++ header file (.h). It needs
the following input files to work:

 * "meta.xml" (in Mogre/Codegen/cpp2java/build) : generated by "cpp2java"; 
     contains the structure of all .h files in an XML format
 * "Attributes.xml" (in Mogre/Codegen/Attributes.xml) : contains information 
     about how to tweak the information in "meta.xml" (e.g. ignore classes, add 
     custom code, ...). See "readme-attributes.txt" for more information.

This tool is used to generate most of MOGRE's source code.

Usage: Simply start the program and click on the "Generate" button. This will
generate the C++/CLI sources.
