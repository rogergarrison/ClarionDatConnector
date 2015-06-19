using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClarionDatConnector
{
    public class ClarionFileHeader  // changed to class because byte[]/string isn't primitive
    {
        public ushort filesig;        // file signature
        public ushort sfatr;          // file attribute and status
        public byte numkeys;          // number of keys in file
        public uint numrecs;          // number of records in file
        public uint numdels;          // number of deleted recoreds
        public ushort numflds;        // number of fields
        public ushort numpics;        // number of pictures
        public ushort numarrs;        // number of arrays
        public ushort reclen;         // record length (including record header)
        public uint offset;           // start of data area
        public uint logeof;           // logical end of file
        public uint logbof;           // logical start of file
        public uint freerec;          // first usuable deleted record
        //public byte [] recname;       // record name without prefix
        //public byte [] memname;       // memo name without prefix
        //public byte [] fileprefix;    // file name prefix
        //public byte [] recprefix;     // record name prefix
        public string recname;        // record name without prefix
        public string memname;        // memo name without prefix
        public string fileprefix;     // file name prefix
        public string recprefix;      // record name prefix
        public ushort memolen;        // size of memo
        public ushort memowid;        // column width of memo
        public uint reserved;         // reserved
        public uint chgtime;          // time of last change
        public uint chgdate;          // date of last change
        public ushort reserved2;      //  reserved
    }

    public class ClarionFileFields // changed to class because byte[]/string isn't primitive
    {
        //public byte fldtype;          // type of field
        public ClarionFileData.ClarionDataTypes fldtype;          // type of field
        //public byte[] fldname;        // name of field
        public string fldname;        // name of field
        public ushort foffset;        // offset into record
        public ushort length;         // length of field
        public byte decsig;           // significance for decimals
        public byte decdec;           // number of decimal places
        public ushort arrnum;         // array number
        public ushort picnum;         // picture number

    }

    public struct ClarionFileKeyPart // is a struct because it meets the rules
    {
        public byte fldtype;          // type of field
        public ushort fldnum;         //filed number
        public ushort elmoff;         // record offset of this element
        public byte elmlen;           // length of element
    }
    public class ClarionFileKeySect // changed to class because byte[]/string isn't primitive
    {
        public byte numcomps;         // number of components for key
        //public byte[] keyname;        // name of this key
        public string keyname;        // name of this key
        public byte comptype;         // type of composite
        public byte complen;          // length of composite
        public List<ClarionFileKeyPart> keyparts;
    }
}
