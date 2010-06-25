mkdir build 2>nul
bin\doxygen.exe ogre4j.doxygen

copy /y fixedDoxygen\*.xml build\doxyxml
copy /y xslt\mycombine.xslt build\doxyxml\ >nul

bin\transform.exe -s:build\doxyxml\index.xml -xsl:build\doxyxml\mycombine.xslt -o:build\all.xml
bin\transform.exe -s:build\all.xml -xsl:xslt\input.xslt -o:build\meta.xml

