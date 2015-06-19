using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace ClarionDatConnector
{
    public class ClarionFileData
    {
        public enum ClarionDataTypes
        {
            LONG = 1,
            REAL = 2,
            STRING = 3,
            STRING_WITH_PICTURE_TOKEN = 4,
            BYTE = 5,
            SHORT = 6,
            GROUP = 7,
            DECIMAL = 8,
            OTHER 
        }
        private readonly string filename;

        private DataTable dataTable = new DataTable();
        public DataTable ClarionData { get { return dataTable; } }

        private List<ClarionFileFields> fldsList = new List<ClarionFileFields>();
        private List<ClarionFileKeySect> keys = new List<ClarionFileKeySect>();
        private ClarionFileHeader fh;

        public ClarionFileData(string filename)
        {
            this.filename = filename;
            //var startTime = DateTime.Now;
            //setupDatatable();
            //Console.WriteLine("Took: {0}", DateTime.Now - startTime);
        }

        private void setupDatatable()
        {
            ReadDataFile((br) => //begin read the file for the header
                {
                    #region SetClarionFileHeader
                    fh = new ClarionFileHeader // begin - set file header info
                    {
                        filesig = br.ReadUInt16(),
                        sfatr = br.ReadUInt16(),
                        numkeys = br.ReadByte(),
                        numrecs = br.ReadUInt32(),
                        numdels = br.ReadUInt32(),
                        numflds = br.ReadUInt16(),
                        numpics = br.ReadUInt16(),
                        numarrs = br.ReadUInt16(),
                        reclen = br.ReadUInt16(),
                        offset = br.ReadUInt32(),
                        logeof = br.ReadUInt32(),
                        logbof = br.ReadUInt32(),
                        freerec = br.ReadUInt32(),
                        recname = Encoding.ASCII.GetString(br.ReadBytes(12)),
                        memname = Encoding.ASCII.GetString(br.ReadBytes(12)),
                        fileprefix = Encoding.ASCII.GetString(br.ReadBytes(3)),
                        recprefix = Encoding.ASCII.GetString(br.ReadBytes(3)),
                        memolen = br.ReadUInt16(),
                        memowid = br.ReadUInt16(),
                        reserved = br.ReadUInt32(),
                        chgtime = br.ReadUInt32(),
                        chgdate = br.ReadUInt32(),
                        reserved2 = br.ReadUInt16()
                    }; // end - set file header info
                    #endregion SetClarionFileHeader

                    #region SetClarionFieldList
                    for (int i = 0; i < fh.numflds; i++) // begin - set fields info
                    {
                        var fld = new ClarionFileFields
                        {
                            fldtype = (ClarionFileData.ClarionDataTypes)br.ReadByte(),
                            fldname = Encoding.ASCII.GetString(br.ReadBytes(16)).Remove(0, fh.fileprefix.Length + 1).Trim(),
                            foffset = br.ReadUInt16(),
                            length = br.ReadUInt16(),
                            decsig = br.ReadByte(),
                            decdec = br.ReadByte(),
                            arrnum = br.ReadUInt16(),
                            picnum = br.ReadUInt16(),
                        };
                        fldsList.Add(fld);
                    } // end - set fields info
                    #endregion SetClarionFieldList

                    #region SetClarionTableKeys
                    for (int i = 0; i < fh.numkeys; i++) // begin - set table keys
                    {
                        var keysect = new ClarionFileKeySect
                        {
                            numcomps = br.ReadByte(),
                            keyname = Encoding.ASCII.GetString(br.ReadBytes(16)),
                            comptype = br.ReadByte(),
                            complen = br.ReadByte(),
                            keyparts = new List<ClarionFileKeyPart>()
                        };
                        for (int j = 0; j < keysect.numcomps; j++)
                        {
                            var keypart = new ClarionFileKeyPart
                            {
                                fldtype = br.ReadByte(),
                                fldnum = br.ReadUInt16(),
                                elmoff = br.ReadUInt16(),
                                elmlen = br.ReadByte()
                            };
                            keysect.keyparts.Add(keypart);
                        }
                        keys.Add(keysect);
                    } // end - set table keys
                    #endregion SetClarionTableKeys

                    #region SetDataTableColumns
                    //begin - set in-memory table columns
                    foreach (var fld in fldsList)
                    {
                        switch (fld.fldtype)
                        {
                            case ClarionFileData.ClarionDataTypes.LONG: // type is 'LONG' uses 4 bytes 
                                // i'm guessing "long" in clarion means date/time, but because i'm planning on using timespan for time
                                // save as string?
                                //dataTable.Columns.Add(fld.fldname, typeof(DateTime));
                                dataTable.Columns.Add(fld.fldname, typeof(String));
                                break;
                            case ClarionFileData.ClarionDataTypes.REAL: // type is 'REAL'
                                throw new NotImplementedException(String.Format("Type {0} is not implemented yet", fld.fldtype));
                                break;
                            case ClarionFileData.ClarionDataTypes.STRING: // type is 'STRING'
                                dataTable.Columns.Add(fld.fldname, typeof(String));
                                break;
                            case ClarionFileData.ClarionDataTypes.STRING_WITH_PICTURE_TOKEN: // type is 'STRING With PICTURE TOKEN'
                                throw new NotImplementedException(String.Format("Type {0} is not implemented yet", fld.fldtype));
                                break;
                            case ClarionFileData.ClarionDataTypes.BYTE: // type is 'BYTE'
                                dataTable.Columns.Add(fld.fldname, typeof(Byte));
                                break;
                            case ClarionFileData.ClarionDataTypes.SHORT: // type is 'SHORT'
                                //dataTable.Columns.Add(fld.fldname, typeof(ushort));
                                dataTable.Columns.Add(fld.fldname, typeof(UInt16));
                                break;
                            case ClarionFileData.ClarionDataTypes.GROUP: // type is 'GROUP'
                                throw new NotImplementedException(String.Format("Type {0} is not implemented yet", fld.fldtype));
                                break;
                            case ClarionFileData.ClarionDataTypes.DECIMAL: // type is 'DECIMAL'
                                // decimals are packed BCD-like format with 2 digits per byte
                                dataTable.Columns.Add(fld.fldname, typeof(Decimal));
                                break;
                            default: // No Ideal What this is. Throw error
                                throw new ArgumentException("Field Type is unknown");
                                break;
                        }
                    }
                    #endregion SetDataTableColumns

                    #region SetTableRows
                    // begin Set table rows
                    // Read the Whole file and add it to the Table
                    byte rec_status;
                    uint rec_ptr;
                    //for (uint rec_no = 0; rec_no < fh.numrecs; rec_no++)
                    for (uint rec_no = 0; rec_no < 100; rec_no++)
                    {
                        br.BaseStream.Seek(fh.offset + (fh.reclen * rec_no), SeekOrigin.Begin);
                        //Read a head by 5 bytes, as Clarion adds 5 bytes to the header of each record
                        rec_status = br.ReadByte();
                        rec_ptr = br.ReadUInt32();
                        DataRow newRow = dataTable.NewRow();
                        foreach (var fld in fldsList)
                        {
                            switch (fld.fldtype)
                            {
                                case ClarionFileData.ClarionDataTypes.LONG: // type is 'LONG' uses 4 bytes -- i'm guessing "long" in clarion means date/time
                                    uint fld_long = br.ReadUInt32();

                                    string long_type = String.Empty;
                                    // date and time information is from
                                    // Clarion TB 117
                                    if (fld_long > 109211) // The Max value for Date is 109211, if it's greater assume it's a time
                                    {                      // though techinally a time can be below 109211 -- but that is in range of 00:00:00 (midnight) to 00:18:12.11
                                        long_type = "time";// if you assume you multiply by 10 --- so an 18 minute range to assume it's a date is good by me
                                        var abstime = fld_long - 1;
                                        //Console.WriteLine(TimeSpan.FromMilliseconds(abstime * 10)); // it is saved as centiseconds, multiply by 10 to get milleseconds
                                        DateTime theTime = new DateTime(1700, 1, 1); // if date is 1700, 1, 1 then it's a time
                                        theTime += TimeSpan.FromMilliseconds(abstime * 10);
                                        //newRow[fld.fldname] = TimeSpan.FromMilliseconds(abstime * 10); //theTime;
                                        //var k = theTime.ToLongTimeString();
                                        newRow[fld.fldname] = theTime.ToLongTimeString();
                                    }
                                    else if (fld_long > 3) // dates can't be below 3
                                    {
                                        long_type = "date";
                                        //uint absday = fld_long > 36527 ? fld_long - 3 : fld_long - 4;
                                        //var year = (1801 + 4 * (absday / 1461));
                                        //absday = absday % 1461;
                                        //year += absday != 1460 ? (absday / 365) : 3;
                                        //var day = absday != 1460 ? (absday % 365) : 365;
                                        //DateTime theDate = new DateTime((int)year, 1, 1).AddDays(day); // if date is not 1700 then it it's a date
                                        DateTime theDate = new DateTime(1800, 12, 28).AddDays(fld_long); // fld_long is number of days since 28-Dec-1800
                                        //Console.WriteLine(theDate.ToString("M/d/yyyy"));
                                        newRow[fld.fldname] = theDate.ToString("M/d/yyyy");
                                    }
                                    else
                                    {
                                        long_type = "Bad Date/Time?";
                                        //Console.WriteLine();
                                        //newRow[fld.fldname] = new DateTime(1, 1, 1); // 1/1/1 will represent an invalid date
                                        newRow[fld.fldname] = String.Empty;
                                    }
                                    break;
                                case ClarionFileData.ClarionDataTypes.REAL: // type is 'REAL'
                                    //br.BaseStream.Seek(br.BaseStream.Position + fld.length, SeekOrigin.Begin);
                                    throw new NotImplementedException(String.Format("Type {0} is not implemented yet", fld.fldtype));
                                    break;
                                case ClarionFileData.ClarionDataTypes.STRING: // type is 'STRING'
                                    // strings are padded with spaces to their maximum length
                                    byte[] fld_bytes = br.ReadBytes(fld.length);
                                    newRow[fld.fldname] = Encoding.ASCII.GetString(fld_bytes);
                                    break;
                                case ClarionFileData.ClarionDataTypes.STRING_WITH_PICTURE_TOKEN: // type is 'STRING With PICTURE TOKEN'
                                    //br.BaseStream.Seek(br.BaseStream.Position + fld.length, SeekOrigin.Begin);
                                    throw new NotImplementedException(String.Format("Type {0} is not implemented yet", fld.fldtype));
                                    break;
                                case ClarionFileData.ClarionDataTypes.BYTE: // type is 'BYTE'
                                    byte fld_byte = br.ReadByte();
                                    newRow[fld.fldname] = fld_byte;
                                    break;
                                case ClarionFileData.ClarionDataTypes.SHORT: // type is 'SHORT'
                                    //var count = 0;
                                    //if (fld.arrnum != 0)
                                    //{
                                    //    for (int j = 0; j < (fld.length / sizeof(ushort)) - 1; j++)
                                    //    {
                                    //        //Console.Write("{0},", br.ReadUInt16());
                                    //        br.ReadUInt16();
                                    //        count++;
                                    //    }
                                    //}
                                    ushort fld_short = br.ReadUInt16();
                                    newRow[fld.fldname] = fld_short;
                                    break;
                                case ClarionFileData.ClarionDataTypes.GROUP: // type is 'GROUP'
                                    //br.BaseStream.Seek(br.BaseStream.Position + fld.length, SeekOrigin.Begin);
                                    throw new NotImplementedException(String.Format("Type {0} is not implemented yet", fld.fldtype));
                                    break;
                                case ClarionFileData.ClarionDataTypes.DECIMAL: // type is 'DECIMAL'
                                    // decimals are packed BCD-like format with 2 digits per byte
                                    var bcdByteArr = br.ReadBytes(fld.length);
                                    decimal clairionDecimalVal = 0;
                                    Stack<int> bcdByteStack = new Stack<int>();
                                    foreach (byte b in bcdByteArr) // unpack each byte in byte arr
                                    {
                                        bcdByteStack.Push((b & 0xf0) >> 4);  // get the first number
                                        bcdByteStack.Push(b & 0x0f); // get the second digit
                                    }
                                    for (int i = -1 * fld.decdec; i < fld.decsig; i++)
                                    {
                                        clairionDecimalVal += (decimal)(Math.Pow(10, i) * bcdByteStack.Pop());
                                    }
                                    int bcdByteStackLeft = bcdByteStack.Pop();
                                    if (bcdByteStackLeft > 0) // this appears to be what makes the value negative
                                    {
                                        clairionDecimalVal *= -1;
                                    }
                                    //if (clairionDecimalVal != 0)
                                    //    Console.WriteLine("{0}\t:{1}", fld.fldname, clairionDecimalVal);
                                    newRow[fld.fldname] = clairionDecimalVal;
                                    break;
                                default: // No Ideal What this is. Throw error
                                    throw new ArgumentException("Field Type is unknown");
                                    break;
                            }
                        }
                        dataTable.Rows.Add(newRow);
                    }
                    // end Set table rows
                    #endregion SetTableRows
                }); // end read the file for the header
        } // end method

        private void ReadDataFile(Action<BinaryReader> fileAction) // begin - open the file and perform action
        {
            #region OpenFileStream
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (BinaryReader br = new BinaryReader(fs, Encoding.ASCII)) // begin -- open file stream
            {
                fileAction(br);
            } // end using of file stream
            #endregion OpenFileStream
        } // end - open the file with action
         
        public IEnumerable<DataRow> GetData()
        {
            setupDatatable();
            return dataTable.AsEnumerable();
        }
    }
}
