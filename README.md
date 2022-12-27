# Epicweb.Optimizely.QuickNavExtension

**version 6 (2022-09-01)**

[![Platform](https://img.shields.io/badge/Platform-.NET%206-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx) [![Platform](https://img.shields.io/badge/Optimizely-%2012.6-green.svg?style=flat)](https://world.optimizely.com/products/#contentcloud)

An Optimizely addon that helps and adds menu items to the QuickNavigationMenu when logged in on public site, 
All configurable links to Search & navigation, admin, admin content type, and logout. Even custom links!

![alt text](https://github.com/Epicweb-Optimizely/Epicweb.Optimizely.QuickNavExtension/blob/master/QuickNavExtension.png?raw=true "This is how the QuickNavExtension could look")

**This is the .net 6 version of : https://github.com/LucGosso/Gosso.EPiServerAddOn.QuickNavExtension ** <-- use this for CMS 11

# Installation and configuration 

Available on nuget.optimizely.com https://nuget.optimizely.com/package/?id=Epicweb.Optimizely.QuickNavExtension

This can be installed via the package manager console in Visual Studio.

Run ```install-package Epicweb.Optimizely.QuickNavExtension``` in package manager console.


Default menus are Admin and logout, to activate other menu items apply this appsettings.json: (they are sortable)

```"Epicweb.QuickNav": "find,admin,contenttype,logout"```

You can add custom menu items, Name and url with pipe in between. Name can be resource path eg /shell/admin/logout

```"Epicweb.QuickNav": "Custom link|https://devblog.gosso.se,Custom Javascript|javascript:alert('Hey you'),find,admin,contenttype,logout"```

To only show logout item, apply this appsettings: 

```"Epicweb.QuickNav": "logout"```


**Alternate way to register links:**

Startup.cs

 ```       

 using Epicweb.Optimizely.QuickNavExtension;

    //add links in a simple way in startup. 
    services
        .AddQuickNav("Custom link", "https://devblog.gosso.se/", role: "WebDevs")
        .AddQuickNav("Custom Javascript", "javascript:if(confirm(\'R U SURE?\')){document.location=\'/\';}", role: "WebDevs")
        .AddQuickNav(QuickNavigator.FIND)
        .AddQuickNav(QuickNavigator.Admin)
        .AddQuickNav(QuickNavigator.ContentType)
        .AddQuickNav(QuickNavigator.LogOut);
            
 ```

**Role Base Links**

You can add a third pipe with rolename from Epi.

eg: ```“Link title|/urlForDevsOnly/|WebDevs”``` <= only WebDevs will see the menu

or ```“logout||WebEditors”``` <= only WebEditors will see the logout menu

# Sandbox alloy app

**Get this solution runing**

1. Clone it

2. Unpack the /alloy/app_data/blobs-and-database.zip

3. dotnet run

4. log in to CMS with "admin" and "Test1234!"

# Package maintainer

https://github.com/lucgosso