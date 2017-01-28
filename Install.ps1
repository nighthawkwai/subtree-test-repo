param($installPath, $toolsPath, $package, $project) 
$dte2 = Get-Interface $dte ([EnvDTE80.DTE2])
#open the link 
$dte2.ItemOperations.Navigate("http://hdimicroservices.azurewebsites.net/nuget/microsoft.services.core/1.2.0.0") | out-null