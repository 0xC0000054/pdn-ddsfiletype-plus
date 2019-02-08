# pdn-ddsfiletype-plus

A [Paint.NET](http://www.getpaint.net) filetype plugin that adds support for some of the DDS formats introduced in DirectX 10 and later.

### Note that a DDS file must be renamed to use the DDS2 file extension before the plugin will load it.
For example, if your DDS file is named `File.dds` you would rename it to `File.dds2`. 

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