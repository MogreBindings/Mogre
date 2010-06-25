@echo off

if "%1"=="clean" goto clean
if "%1"=="build" goto build
if "%1"=="meta" goto meta

goto usage

:clean

echo.clean project files...
rmdir /s /q build
mkdir build
echo.clean successful

goto end

:build

mkdir build 2>nul

@echo on

bin\doxygen.exe ogre4j.doxygen

copy /y fixedDoxygen\*.xml build\doxyxml
copy /y xslt\mycombine.xslt build\doxyxml\ >nul

bin\transform.exe -s:build\doxyxml\index.xml -xsl:build\doxyxml\mycombine.xslt -o:build\all.xml
bin\transform.exe -s:build\all.xml -xsl:xslt\input.xslt -o:build\meta.xml

@echo off

goto end

:meta

@echo on

bin\transform.exe -s:build\all.xml -xsl:xslt\input.xslt -o:build\meta.xml

@echo off

:usage

echo.autogenerates jni bindings
echo.
echo. %0 [clean^|build^|meta]
echo.

:end

