# Raspberry

A repo for my next dumb game idea.

The "Nez" project is a third party dependency cloned from here https://github.com/prime31/Nez.
If Nez is failing to build, make sure you have .NETFramework version 4.7.1 Developer pack downloaded from here https://dotnet.microsoft.com/en-us/download/dotnet-framework/net471

6/29/2022
The "Nez" project is now the "NezStandard" project. I created a .netstandard project and copied all the source files.
I also had to add the System.Drawing.Common project to get it to build

8/1/2022
In an effort to get my first shader (effect) working with Nez, I added a post build step that compiles the .fx file to the .mgfxo format
To do this, make sure to install the mgfxc command line tool using the following command:
dotnet tool install --global dotnet-mgfxc --version 3.8.0.1641