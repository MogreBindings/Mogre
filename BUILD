Please verify this!

- Download boost (www.boost.org). Compile and install it.
- Set the environment variables BOOST_INCLUDE_DIRECTORY 
  (e.g. C:\boost\include\boost-1_42) and BOOST_LIBRARY_DIRECTORY
  (e.g. C:\boost\lib).
- Download Ogre Dependencies and extract them, e.g. 
  http://sf.net/projects/ogre/files/ogre-dependencies-vc%2B%2B/1.7
  into Main/OgreSrc/Dependencies. Compile the Dependencies.
- Clone ogre sources, branch 1.7 into Main/OgreSrc. 
  See Main/OgreSrc/readme.txt for more.
- Patch Ogre files by applying "Main/Ogre Patches/Mogre.patch"
- Start CMake to generate Ogre build files. Make sure you have 
  OGRE_CONFIG_ENABLE_PVRTC switched ON and 
  OGRE_CONFIG_CONTAINERS_USE_CUSTOM_ALLOCATOR switched OFF. Target
  Main/OgreSrc/build as output directory.
- execute Codegen/cpp2java/build.bat (from within Codegen/cpp2java)
- Compile AutoWrap (Codegen/AutoWrap/AutoWrap.sln or ...AutoWrap_vs2010.sln)
- Run AutoWrap and click "Generate".
- Compile Ogre. If you get the error "missing mogre.lib",
  make sure that in CLRConfig.h it is "#define LINK_TO_MOGRE 0"
- Compile Mogre (either Main/Mogre_vc9.sln or Main/Mogre_vs2010.sln)
- Change in Main/OgreSrc/ogre/OgreMain/include/CLRConfig.h the define
  to "#define LINK_TO_MOGRE 1"
- Compile Ogre again (no need for a full rebuild)
