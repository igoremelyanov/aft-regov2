@echo Make sure you are running this script with Administrator's rights
%WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy RemoteSigned -command "& {Invoke-Command -ScriptBlock { &'C:\Program Files\Service Bus\1.1\Scripts\ImportServiceBusModule.ps1' ; New-SBNamespace -Name regodebug -ManageUsers %username% }} "
