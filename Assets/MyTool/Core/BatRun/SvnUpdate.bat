svn up D:\AquamanClient --accept postpone
svn status D:\AquamanClient

svn cleanup D:\AqumanClientClean
svn up D:\AqumanClientClean --accept  theirs-full
svn revert D:\AqumanClientClean -R
svn status D:\AqumanClientClean

svn cleanup D:\AquamanClientMilestone02
svn up D:\AquamanClientMilestone02 --accept  theirs-full
svn revert D:\AquamanClientMilestone02 -R
svn status D:\AquamanClientMilestone02

svn cleanup D:\AquamanDesigMilestone02
svn up D:\AquamanDesigMilestone02 --accept  theirs-full
svn revert D:\AquamanDesigMilestone02 -R
svn status D:\AquamanDesigMilestone02

svn cleanup D:\AquamanDesign\table_export
svn up D:\AquamanDesign\table_export --accept  theirs-full
::svn revert D:\AquamanDesign\table_export -R
svn status D:\AquamanDesign\table_export