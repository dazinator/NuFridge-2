# NuFridge
NuFridge is a package management server for NuGet which supports multiple feeds.

The feeds are powered by https://github.com/themotleyfool/NuGet.Lucene.

## Chat Room
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/lukeskinner/NuFridge?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Prerequisites
1. Install .NET Framework 4.5.1 (or higher)

## Installation
1. Download the installer from the latest release.
2. Run the .msi installer and follow the instructions

## Building The Code

To build the code, follow these steps:

1. Ensure that [NodeJS](http://nodejs.org/) is installed. This provides the platform on which the build tooling runs.

2. Ensure that [Grunt](http://gruntjs.com/) is installed. If you need to install it, use the following command:

  ```shell
  npm install -g grunt-cli
  ``` 

3. From the 'src\NuFridge.Service\Website\Content' folder, execute the following command:

  ```shell
  npm install
  ```

4. Build the solution in Visual Studio 2013 in the Debug configuration

5. (Optional) From the 'src\NuFridge.Service\Website\Content' folder, execute the following command:

  ```shell
  grunt watch
  ```
This step allows the use of the [LiveReload](https://chrome.google.com/webstore/detail/livereload/jnihajbhpnppcggbcgedagnkighmdlei?hl=en) Chrome extension which will refresh the page when either a .html or .js file changes.

6. Run the NuFridge.Service project in the Debug configuration

Before submitting a PR you should build the solution in Visual Studio under the Release configuration. 
When using the Release configuration it will execute the following command in the NuFridge.Service projects pre-build event:
  ```shell
  grunt "build release"
  ```
The build release task performs the following:
1. Runs jshint against all .js files - to improve code consistency
2. Clean the build folder
3. Copy files over to the build folder
4. Run the durandal:release task to output a single main.js javascript file
5. Minify the main.js file


## Screenshots
### Website - List of feeds
![NuFridge Control Panel](https://www.nufridge.com/images/List.png)
### Website - Create feed
![NuFridge Control Panel](https://www.nufridge.com/images/Create.png)