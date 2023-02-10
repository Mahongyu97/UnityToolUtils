svn up D:\AquamanClient --accept postpone
svn status D:\AquamanClient

svn cleanup D:\AqumanClientClean
svn up D:\AqumanClientClean --accept  theirs-full
svn revert D:\AqumanClientClean -R
svn status D:\AqumanClientClean

svn cleanup D:\AquamanDesign\table_export
svn up D:\AquamanDesign\table_export --accept theirs-full
::svn revert D:\AquamanDesign\table_export -R
svn status D:\AquamanDesign\table_export