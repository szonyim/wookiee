
# Exotic Wookiee Chat Support  

## Minimal requirements
  
- Windows Server 2012 R2 or Windows 8.1  
- .NET 4.7.2 runtime environment ([Web installer](https://dotnet.microsoft.com/download/thank-you/net472 "Web Installer") or [Offline installer](https://dotnet.microsoft.com/download/thank-you/net472-offline "Offline installer"))  
- IIS 8.5 or above with WebSocket Protocol  
- MSSQL Database server  
  
## Installation
1. Create database and an account for it.  
2. Create an empty directory on your web server under the wwwroot folder (e.g.: C:\inetpub\wwwroot\wookiee)  
3. Copy the **Release** folder content to the new directory  
4. Modify connectionStrings paramteres in **Web.config**. Replace **Database**, **User Id** and **Password** properties value with your database name, database user name and password.  
5. In IIS manager Add Website.  
1. Enter Site name  
2. Select physical path (e.g.: C:\inetpub\wwwroot\wookiee)  
3. Enter Host name  
4. Select Start Website Immediately  
5. Click OK button  
  
## First usage
First you can login with admin user. Admins user password is admin by default. Before you do anything, change it to your own password.