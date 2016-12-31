# openmcdf
**Structured Storage .net component - pure C#**

OpenMCDF is a 100% .net / C# component that allows developers to manipulate [Microsoft Compound Document File Format](https://msdn.microsoft.com/en-us/library/dd942138.aspx) (also known as OLE structured storage). 

Compound file includes multiple streams of information (document summary, user data) in a single container. 

This file format is used under the hood by a lot of applications: the files created by Microsoft Office until the 2007 product release are all structured storage files. Also the omnipresent Thumbs.db, used by Windows as thumbnails cache, is a structured storage file. Visual Studio .suo files (solution options) are compound files and a lot of audio/video editing tools saves project file in a compound container.

OpenMcdf supports read/write operations on streams and storages and traversal of structures tree. It supports version 3 and 4 of the specifications, uses lazy loading wherever possible to reduce memory usage and offer an intuitive API to work with structured files.


It's very easy to **create a new compound file**

```C#
byte[] b = new byte[10000];

CompoundFile cf = new CompoundFile();
CFStream myStream = cf.RootStorage.AddStream("MyStream");

myStream.SetData(b);
cf.Save("MyCompoundFile.cfs");
cf.Close();
```

You can **open an existing one**, an excel workbook (.xls) and use its main data stream

```C#
//A xls file should have a Workbook stream
String filename = "report.xls";
CompoundFile cf = new CompoundFile(filename);
CFStream foundStream = cf.RootStorage.GetStream("Workbook");
byte[] temp = foundStream.GetData();
//do something with temp
cf.Close();
```

Adding **storage and stream items** is just as easy...

```C#
CompoundFile cf = new CompoundFile();
CFStorage st = cf.RootStorage.AddStorage("MyStorage");
CFStream sm = st.AddStream("MyStream");
```
...as removing them

```C#
cf.RootStorage.Delete("AStream"); // AStream item is assumed to exist.
```

Call *commit()* method when you need to persist changes to the underlying stream

```C#
cf.RootStorage.AddStream("MyStream").SetData(buffer);
cf.Commit();
```

If you need to compress a compound file, you can purge its unused space

```C#
CompoundFile.ShrinkCompoundFile("MultipleStorage_Deleted_Compress.cfs"); 
```

OpenMcdf runs as well on the [Mono] (http://www.mono-project.com/) platform .
