mkdir build 2>nul
mkdir build\doxyxml 2>nul

bin\doxygen.exe ogre.doxygen

copy /y fixedDoxygen\*.xml build\doxyxml

bin\transform.exe -s:build\doxyxml\index.xml -xsl:build\doxyxml\combine.xslt -o:build\all.xml
bin\transform.exe -s:build\all.xml -xsl:input.xslt -o:build\meta.xml

