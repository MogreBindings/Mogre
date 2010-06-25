
Step by step instructions to build Mogre/Ogre 1.7.1 with Visual Studio 2010:
============================================================================

- Clone Mogre: "hg clone http://bitbucket.org/mogre/mogre -u Mogre17 Mogre".
- Clone Ogre 1.7: "hg clone http://bitbucket.org/sinbad/ogre -u 58266f25ccd2 Mogre\Main\OgreSrc\ogre".
- Apply "Mogre\Main\Ogre Patches\58266f25ccd2.patch" to "Mogre\Main\OgreSrc\ogre".
- Download "http://surfnet.dl.sourceforge.net/project/ogre/ogre-dependencies-vc%2B%2B/1.7/OgreDependencies_MSVC_20100501.zip".
- Unpack "OgreDependencies_MSVC_20100501.zip" into "Mogre\Main\OgreSrc\ogre", it will create a folder "Dependencies".
- Open the dependencies solution file "Mogre\Main\OgreSrc\ogre\Dependencies\src\OgreDependencies.VS2010.sln" in Visual Studio.
- Use batch build to rebuild all 32bit projects in the solution. Do NOT compile the x64 configurations!
- Start CMake to generate Ogre build files. Make sure you have OGRE_CONFIG_ENABLE_PVRTC switched ON and 
  OGRE_CONFIG_CONTAINERS_USE_CUSTOM_ALLOCATOR switched OFF. Target Main/OgreSrc/build as output directory.
- Go to folder "Mogre\Codegen\cpp2java" and execute "build.bat" in this folder.
- Open solution "Mogre\Codegen\AutoWrap\AutoWrap_vs2010.sln" in Visual Studio and compile the Debug version.
- Execute "AutoWrap.exe" in folder "Mogre\Codegen\AutoWrap\bin\Debug" and press button "Produce".
- Go to folder "Mogre\Main\Ogre" and execute "copy_to_ogre.bat" in this folder.
- Copy "Mogre\Main\include\auto\CLRObjects.inc" to folder "Mogre\Main\OgreSrc\build\include".
- Open solution "Mogre\Main\OgreSrc\build\OGRE.sln" in Visual Studio.
- In the solution explorer window of Visual Studio, find project "OgreMain" and right click it.
- Select "Add->Existing Item..." and navigate to "Mogre\Main\OgreSrc\ogre\OgreMain\src", add "CLRHandle.cpp" and "CLRObject.cpp".
- In the solution explorer window of Visual Studio, find project "OgreMain" and right click it.
- Select "Add->Existing Item..." and navigate to "Mogre\Main\OgreSrc\ogre\OgreMain\include", add "CLRConfig.h", "CLRHandle.h" and "CLRObject.h".
- Open "CLRConfig.h" in Visual Studio and change "#define LINK_TO_MOGRE 1" to "#define LINK_TO_MOGRE 0".
- Open the batch build window in Visual Studio.
- Select "Debug|Win32" and "Release|Win32" for the following projects:
  + OgreMain
  + OgrePaging
  + OgreRTShaderSystem
  + OgreTerrain
  + Plugin_BSPSceneManager
  + Plugin_CgProgramManager
  + Plugin_OctreeSceneManager
  + Plugin_OctreeZone
  + Plugin_ParticleFX
  + Plugin_PCZSceneManager
  + RenderSystem_Direct3D9
  + RenderSystem_GL
- Rebuild all selected projects.
- If there are linker errors for "_ITERATOR_DEBUG_LEVEL" mismatch, check "http://www.ogre3d.org/forums/viewtopic.php?f=1&t=54533&start=100#p388654",
  I could fix the errors by compiling debug version of dependencies first, then compiling release versions of dependencies separately.
- Open soultion "Mogre\Main\Mogre_vs2010.sln" in Visual Studio.
- Use batch build to rebuild all projects.
- Open solution "Mogre\Main\OgreSrc\build\OGRE.sln" again.
- Open "CLRConfig.h" in Visual Studio and change "#define LINK_TO_MOGRE 0" back to "#define LINK_TO_MOGRE 1".
- Open batch build and use a normal build instead of rebuild all this time, this will save a lot of time.
- Debug binaries are in "Mogre\Main\lib\Debug" and "Mogre\Main\OgreSrc\build\lib\Debug".
- Release binaries are in "Mogre\Main\lib\Release" and "Mogre\Main\OgreSrc\build\lib\Release".


This is what cmake printed out for me as project configuration summary:
=======================================================================

----------------------------------------------------------------------------
  FEATURE SUMMARY
----------------------------------------------------------------------------

Building components:
  + Paging
  + Terrain
  + RTShader System
  + RTShader System Core Shaders
  + RTShader System Extensions Shaders
Building plugins:
  + BSP scene manager
  + Cg program manager
  + Octree scene manager
  + Portal connected zone scene manager
  + Particle FX
Building rendersystems:
  + Direct3D 9
  + OpenGL
Building executables:
  + Samples
  + Tools
Building core features:
  + DDS image codec
  + PVRTC image codec
  + FreeImage codec
  + ZIP archives

Build type:                      dynamic
Threading support:               none
Use double precision:            disabled
Allocator type:                  nedmalloc (pooling)
STL containers use allocator:    disabled
Strings use allocator:           disabled
Memory tracker (debug):          disabled
Memory tracker (release):        disabled
Use new script compilers:        enabled
Use Boost:                       disabled

----------------------------------------------------------------------------


OLD INSTRUCTIONS, KEPT HERE FOR REFERENCE, WILL BE REMOVED LATER:
=================================================================

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

