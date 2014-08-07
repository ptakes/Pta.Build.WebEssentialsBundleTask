Pta.Build.WebEssentialsBundleTask
=================================

**MSBUILD task for expanding Web Essentials CSS and JavaScript bundles in HTML files**

Using Web Extenssials' CSS and JavaScript bundle files?
Referencing those bundles in plain HTML files?
Want the same behavior as the ASP.NET Optimization framework?

## Quick start
1. Install the NuGet package to your project:

	_Install-Package Pta.Build.WebEssentialsBundleTask_

2. Create one or more Web Essentials' CSS or JavaScript bundle files.
3. Reference the bundles in your HTML files, for example:

	_`!!styles: /css/app!!`_

	_`!!scripts: /js/vendor!!`_

	_`!!scripts: /js/app!!`_

4. Build your project.

## Documentation

### Bundle files

You can create CSS and JavaScript bundle files in you _Visual Studio_ web projects.  Those files with extension
_.css.bundle_ or _.js.bundle_ are XML files referencing the CSS and JavaScript files that are be part of the bundle.

_Web Essentials_ will automatically generate and update the plain and minified version of the bundle resource files.

In HTML, you reference the bundle resource files as normal CSS and JavaScript resources like:

`<link rel='stylesheet' href='/css/app.css'>`
`<link rel='stylesheet' href='/css/app.min.css'>`
`<script src='/js/vendor.js'></script>`
`<script src='/js/vendor.min.js'></script>`

There are 3 problems with this approach:

1. The bundle will not expand to the individual resource files in debug builds.
2. The bundle will not switch between the minified version in release builds and the plain, non-minified, version in debug builds.
3. The bundle will not automatically invalidate the browsers cache if the resource file changes.

### Using the NuGet package

The NuGet package adds a build task to the project. This task will scan all HTML files in the project for bundle references.
When a bundle reference is found, the bundle reference is replaced by a reference to the resource file.

For example:

`!!scripts: /js/app!!`

will be replaced with:

`<!--begin-scripts: /libs/app-->`
`<script src='/js/app.min.js?_v=140ff438eaaede'></script>`
`<!--end-scripts: /libs/app-->`

or:

`<!--begin-scripts: /libs/app-->`
`<script src='/js/app/main.js?_v=c3506a667e0dc5'></script>`
`<script src='/js/app/dataService.js?_v=79c103a8992298'></script>`
`<script src='/js/app/homeController.js?_v=f57f92c89eb3d8'></script>`
`<!--end-scripts: /libs/app-->`

depending on the build configuration. The former one is a release build, the later one a debug build.

Things to note:

* The release build references the minified version of the resource file generated by Web Essentials.
  You can disable minification by setting the minify flag in the bundle file to false.
* The debug build references all files in the bundle separately.
* The debug build references the plain, non-minified, files.
* A version query is always added to the resource reference.

### The build task

The build task will runs as early as possible in the build so they can be processed further by other build tasks.
For instance they can added as embedded resources in assemblies.

The task is define as follows:

> `<target name="WebEssentialsBundle">	`
> > `<itemgroup>`
> > > `<bundles include="@(Content)" condition="'%(Extension)' == '.bundle'" /> `
> > > `<htmlfiles include="@(Content)" condition="'%(Extension)' == '.html'" /> `
`</itemgroup>`
`<WebEssentialsBundleTask Configuration="$(Configuration)" ProjectDir="$(ProjectDir)" Bundles="@(Bundles)" HtmlFiles="@(HtmlFiles)" />`
`</Target>`


	### Not supported

	* Web Essentials HTML and sprite bundles.
	* The outputDirectory setting in the bundle file (also not implemented by Web Essentials 2013).

	### TODOs					s

	* Make the generation of the version query string optional.
