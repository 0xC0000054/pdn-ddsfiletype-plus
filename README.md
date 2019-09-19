# pdn-ddsfiletype-plus

A [Paint.NET](http://www.getpaint.net) filetype plugin that adds support for some of the DDS formats introduced in DirectX 10 and later.

### This plugin is bundled with Paint.NET 4.2.2 and later.

If you need the features from a newer version you can still install the plugin.   
The plugin will override the bundled version if it has higher version number.

## Installing the plugin

1. Close Paint.NET.
2. Place DdsFileTypePlus.dll, DdsFileTypePlusIO_x86.dll and DdsFileTypePlusIO_x64.dll in the Paint.NET FileTypes folder which is usually located in one the following locations depending on the Paint.NET version you have installed.

  Paint.NET Version |  FileTypes Folder Location
  --------|----------
  Classic | C:\Program Files\Paint.NET\FileTypes    
  Microsoft Store | Documents\paint.net App Files\FileTypes

3. Restart Paint.NET.

## License

This project is licensed under the terms of the MIT License.   
See [License.txt](License.txt) for more information.

***

# Source code

## Prerequsites

* Visual Studio 2017
* Paint.NET 4.0.17 or later

## Building the plugin

* Open the solution
* Change the PaintDotNet references in the DDSFileTypePlus project to match your Paint.NET install location
* Update the post build events to copy the build output to the Paint.NET FileTypes folder
* Build the solution

## 3rd Party Code

This project utilizes the following code (located under 3rdParty folder)

* [DirectXTex](https://github.com/Microsoft/DirectXTex) (February 7 2019 release)