using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClarionDatConnector;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;

namespace InventorySelector
{
    class Program
    {
        /// <summary>
        /// Sample usage of the ClarionDatConnector Library. It outputs all the data that  has a value On_Hand > 0
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string fileName = "INV.DAT";
            ClarionFileData clarionFile = new ClarionFileData(fileName);
            var dataTables = clarionFile.GetData();
            var lessTPR = (from dataTable in dataTables
                           where dataTable.Field<Decimal>("ON_HAND") > 0
                          select new 
                          { 
                              ItemName = dataTable.Field<String>("DESCR"),
                              OnHand = dataTable.Field<Decimal>("ON_HAND"),
                          }).ToList() ;
            foreach(var k in lessTPR)
            {
                Console.WriteLine(k);
            }
            Console.Read();
        }
    }
}
