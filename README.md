# i18nex.ZipBsonLoader

A simple loader that allows i18nex to load zipped bson files. Bson files are created with our included console tool. The file is zipped by the user afterwards.

This was made in an attempt to compress the gross amount of script files a translation of this game takes. It was simply unacceptable and the load times were way too long. This seeks to perfectly balance size and speed. The result is this loader for i18nex. Faster than loose files and smaller too. In trials, the following speeds were observed:

```
Testing with 292618 script files.

Loose files: 34 seconds
BSON file: 1 second
Zipped BSON file: 5 seconds

*Results may vary.
```

- Supports BSON packed and zipped UI and Script files. Also supports unzipped BSON files.
- Prioritizes loose (unpacked and unzipped) files over packed and zipped files, allowing you to add your own dumped translations or make quick changes.
- Textures are not supported, they must be left unpacked and unzipped.

# Priority Scheme
1. Loose files
2. Unzipped BSON
3. Zipped BSON

This means that loose files will always overwrite BSON files. Unzipped BSON will always overwrite Zipped BSON files.

# Requires

i18nEx: [https://github.com/ghorsington/COM3D2.i18nEx  ](https://github.com/Pain-Brioche/COM3D2.i18nEx)

# How to use the loader

Simply place the English and Loader folders in your i18nex folder in the root of your game. If you wish to load other languages, copy and paste the English folder, and rename it to your desired language (ex: Swahili).

Zipped BSON files are to be placed in their appropriate folders. So, ZipBsons containing scripts go in the `English/Scripts`, ZipBsons containing UI csvs go into the `English/UI` folder.

If you want to do this process manually, then create a new folder and name it your desired language (Ex: Marshallese), and then create a file named `config.ini` file with the following contents:
```
[Info]  
Loader=i18nex.ZipLoader  
```
# How to use the packer

The packer is a console utility, so you must run it via something like command prompt or powershell. Running it through there will allow you to see a list of commands. As the input, you'd pass in the contents of what would normally be read in your i18nex folder.

Say you had a Scripts folder that is incredibly large and you'd like to pack it using our tool. You would simply supply the Scripts folder you wish to target, and you'd recieve a BSON file you can zip and place in the Scripts folder, deleting your loose files afterwards.

## Zipping Guidelines:
- Use zip. Must be zip. No exceptions.
- Use deflate or deflate64
