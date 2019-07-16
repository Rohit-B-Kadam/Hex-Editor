using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace Hex_Editor
{
    // DataItem store each row data of DataGrid
    public class DataItem
    {
        public long offset;
        public byte[] value;    
        public DataItem()
        {
            value = new byte[16];
        }   

        public string Ascii
        {
            //I have not use 'for' loop beacuse I think this will work little fast
            get
            {
                string temp =   $"{(char)value[0]}{(char)value[1]}{(char)value[2]}" +
                                $"{(char)value[3]}{(char)value[4]}{(char)value[5]}" +
                                $"{(char)value[6]}{(char)value[7]}{(char)value[8]}" +
                                $"{(char)value[9]}{(char)value[10]}{(char)value[11]}" +
                                $"{(char)value[12]}{(char)value[13]}{(char)value[14]}" +
                                $"{(char)value[15]}" ;
                return temp;
            }
        }

        public string Offset
        {
            get
            {
                return offset.ToString("X2");
            }

        }
        
        public string Zero
        {
            get { return value[0].ToString("X2"); }
        }

        public string One
        {
            get { return value[1].ToString("X2"); }
        }

        public string Two
        {
            get { return value[2].ToString("X2"); }
        }

        public string Three
        {
            get { return value[3].ToString("X2"); }
        }

        public string Four
        {
            get { return value[4].ToString("X2"); }
        }

        public string Five
        {
            get { return value[5].ToString("X2"); }
        }

        public string Six
        {
            get { return value[6].ToString("X2"); }
        }

        public string Seven
        {
            get { return value[7].ToString("X2"); }
        }

        public string Eight
        {
            get { return value[8].ToString("X2"); }
        }

        public string Nine
        {
            get { return value[9].ToString("X2"); }
        }

        public string A
        {
            get { return value[10].ToString("X2"); }
        }

        public string B
        {
            get { return value[11].ToString("X2"); }
        }

        public string C
        {
            get { return value[12].ToString("X2"); }
        }

        public string D
        {
            get { return value[13].ToString("X2"); }
        }

        public string E
        {
            get { return value[14].ToString("X2"); }
        }

        public string F
        {
            get { return value[15].ToString("X2"); }
        }
    }

    // ExeReader: store the byte data of exe and do some operation on bytearray(like searching)
    public class ExeReader
    {
        public byte[] byteArr;      // To store all data of exeFile
        public long offset = 0;

        // Open the exefile and read the data
        public  ExeReader(string name)
        {
            offset = 0;
            string exeName = $@"{name}";
            try
            {
                FileInfo fi = new FileInfo(exeName);
                if (!fi.Extension.Equals(".exe") || !fi.Exists)
                {
                    Console.WriteLine("not exe");
                }

                byteArr = File.ReadAllBytes(exeName);
            }
            catch (IOException)
            {
                //MessageBox.Show(e.ToString(), "Exception");
            }
        }

        /* GetData return List of DataItem(set of 16 bytes).
         * list size depend the given argument
         * After reading Offset also change
         */
        public List<DataItem> GetData( long pagesize )
        {
            List<DataItem> diList = new List<DataItem>();
            DataItem di;
            // loop till pagesize or bytearray complete
            for (long i = offset; i < byteArr.Length && pagesize != 0; i++)
            {
                di = new DataItem();
                di.offset = i;
                for (int j = 0; j < 16; j++)
                {
                    if ((i + j) < byteArr.Length)
                    {
                        di.value[j] = byteArr[i + j];
                    }
                    else
                    {
                        di.value[j] = 0;
                    }

                    offset++;    
                }
                i += 15;
                pagesize--;

                diList.Add(di);
                //yield return di;
            }
            return diList;
        }


        // FindString accept the byte[] and find same sequence in ByteArr
        // Return the starting position
        public long FindString(byte[] bsearch)
        {
            long startIndex = -1;
            int k = 0;
            for(int i = 0; i < byteArr.Length; i++)
            {
                
                if( byteArr[i] == bsearch[k])
                {

                    if( k == 0)
                    { // Containing first char of string
                        startIndex = i;
                    }

                    if( k == bsearch.Length -1)
                    {   
                        return startIndex;
                    }
                    k++;
                }
                else
                {
                    k = 0;
                    startIndex = -1;
                }
            }

            return -1;
        }


    }

}
