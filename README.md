# pdn-ddsfiletype-plus

A [Paint.NET](http://www.getpaint.net) filetype plugin that adds support for some of the DDS formats introduced in DirectX 10 and later.

### This plugin is bundled with Paint.NET 4.2.2 and later.

If you need the features from a newer version you can still install the plugin into the FileTypes folder.   
The installed plugin will override the bundled version if it has higher version number.

The plugin supports loading and saving DDS files that contain single images and cube maps, texture arrays and volume maps are not supported.   
If the file contains mipmaps only the main image will be loaded, the plugin has the option to generate mipmaps when saving.

Cube maps are loaded and saved using a 'crossed image' layout, see [Creating and Editing Cube Maps](https://github.com/0xC0000054/pdn-ddsfiletype-plus/wiki/Cube-Maps) for more information.   
An overview of the save UI options is provided on [this](https://github.com/0xC0000054/pdn-ddsfiletype-plus/wiki/Save-UI) page.

## Supported DDS Formats

The plugin can read most DDS formats, but only the following formats are supported when saving:

### Compressed

* BC1 (Linear and sRGB)
* BC2 (Linear and sRGB)
* BC3 (Linear, sRGB and RXGB)
* BC4 (Unsigned)
* BC5 (Signed and Unsigned)
* BC6H (Unsigned 16-bit Float)
* BC7 (Linear and sRGB)

### Uncompressed

* R8G8B8A8 (Linear and sRGB)
* R8 (Unsigned)
* R8G8 (Signed and Unsigned)
* R32 (Float)
* B8G8R8A8 (Linear and sRGB)
* B8G8R8X8 (Linear and sRGB)
* B4G4R4A4
* B5G5R5A1
* B5G6R5

### Legacy DirectX 9

* R8G8B8X8
* B8G8R8
* ATI1 (BC4 Unsigned)
* ATI2 (BC5 Unsigned)

## Installing the plugin

1. Close Paint.NET.
2. Place DdsFileTypePlus.dll, DdsFileTypePlusIO_ARM64.dll and DdsFileTypePlusIO_x64.dll in the Paint.NET FileTypes folder which is usually located in one the following locations depending on the Paint.NET version you have installed.

  Paint.NET Version |  FileTypes Folder Location
  --------|----------
  Classic | C:\Program Files\Paint.NET\FileTypes    
  Microsoft Store | Documents\paint.net App Files\FileTypes
  Portable | <Paint.NET folder>\FileTypes

3. Restart Paint.NET.

## License

This project is licensed under the terms of the MIT License.   
See [License.txt](License.txt) for more information.

***

# Source code

## Prerequsites

* Visual Studio 2022
* Paint.NET 5.0 or later

## Building the plugin

* Open the solution
* Change the PaintDotNet references in the DDSFileTypePlus project to match your Paint.NET install location
* Update the post build events to copy the build output to the Paint.NET FileTypes folder
* Build the solution

## 3rd Party Code

This project utilizes the following code (located under 3rdParty folder)

* [DirectXTex](https://github.com/Microsoft/DirectXTex) (September 1, 2023 release)