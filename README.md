Pta.Build.WebEssentialsBundleTask
=================================

**MSBUILD task for expanding Web Essentials CSS and JavaScript bundles in HTML files**

Using Web Extenssials' CSS and JavaScript bundle files? Referencing the bundles in plain HTML files? Want the same behavior as the ASP.NET Optimization framework? 

## Quick start
1. Install the NuGet Package:

   _Install-Package Pta.Build.WebEssentialsBundleTask_
   
2. Create Web Essentials' CSS or JavaScript bundle files.
3. Reference the bundles in your HTML files:

   _!!styles: /css/app!!_
   
   _!!scripts: /js/vendor!!_
   
   _!!scripts: /js/app!!_
   
4. Build your project.  


TODO: make adding hash version query strings to URLs optional