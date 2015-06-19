# Welcome to ClarionDatConnector
**ClarionDatConnector** is a C# library to connect and read from a Clarion "dat" file database based on Clarion TB117

###Information###
Clarion "DAT" files are binary files with a nonSQL-like syntax making it difficult to process and extract information.

The binary file is broken down into 4 parts:
- File Header: this contains the file signature, attributes, status and the number of fields, pictures, records, etc
- Field Header: information of the field names, size and information for processing the decimal values
- Field Keys: information on the type, number and offest of the field
- Field Values: the actual data of the field

The outline for the Clarion "DAT" was supposedly in "Technical Bullitin 117" (TB117), but finding this bullitin is difficult. I found an old forum that had some of the information and used that as the basis for the ClarionDatconnector. A copy of the forums is in the project.

###History###
ClarionDatConnector was created in response to the lack of information that exists regarding the "DAT" format of Clarion databases. After finding almost no information about the Clarion "DAT" format files, I decided to create my own application to read the data so I can process it easily.


###How to use###
Pass the "DAT" file to the constructor "ClarionFileData". Call class method "GetData()" it creates an in-memory database of the "DAT" file. *This may change later to use paging, etc. The file I was working on was quite small, so reading the whole file into memory was not a problem* 

