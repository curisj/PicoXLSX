﻿/*
 * PicoXLSX is a small .NET library to generate XLSX (Microsoft Excel 2007 or newer) files in an easy and native way
 * Copyright Raphael Stoeckli © 2015
 * This library is licensed under the MIT License.
 * You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PicoXLSX
{
    /// <summary>
    /// Class representing a cell of a worksheet
    /// </summary>
    public class Cell : IComparable<Cell>
    {
        /// <summary>
        /// Enum defines the basic data types of a cell
        /// </summary>
        public enum CellType
        {
            /// <summary>Type for single characters and strings</summary>
            STRING,
            /// <summary>Type for all numeric types (integers and floats, respectively doubles)</summary>
            NUMBER,
            /// <summary>Type for dates and times</summary>
            DATE,
            /// <summary>Type for boolean</summary>
            BOOL,
            /// <summary>Type for Formulas (The cell will be handled differently)</summary>
            FORMULA,
            /// <summary>Default Type, not specified</summary>
            DEFAULT
        }

        private Style cellStyle;

        /// <summary>Number of the row (zero-based)</summary>
        public int RowAddress { get; set; }
        /// <summary>Number of the column (zero-based)</summary>
        public int ColumnAddress { get; set; }
        /// <summary>Value of the cell (generic object type)</summary>
        public object Value { get; set; }
        /// <summary>Type of the cell</summary>
        public CellType Fieldtype { get; set; }
        /// <summary>
        /// Assigned style of the cell
        /// </summary>
        public Style CellStyle
        {
            get { return cellStyle; }
        }

        /// <summary>Combined cell address as struct (read-only)</summary>
        public Address CellAddress
        {
            get { return new Address(this.ColumnAddress, this.RowAddress); }
        }


        /// <summary>Default constructor</summary>
        public Cell()
        {
        }

        /// <summary>
        /// Constructor with value and cell type
        /// </summary>
        /// <param name="value">Value of the cell</param>
        /// <param name="type">Type of the cell</param>
        public Cell(object value, CellType type)
        {
            this.Value = value;
            this.Fieldtype = type;
        }

        /// <summary>
        /// Constructor with value, cell type, row address and column address
        /// </summary>
        /// <param name="value">Value of the cell</param>
        /// <param name="type">Type of the cell</param>
        /// <param name="column">Column address of the cell (zero-based)</param>
        /// <param name="row">Row address of the cell (zero-based)</param>
        public Cell(object value, CellType type, int column, int row) : this(value, type)
        {
            this.ColumnAddress = column;
            this.RowAddress = row;
        }

        /// <summary>
        /// Method resets the Cell type an tries to find the actual type. This is used if a Cell was created with the CellType DEFAULT. CellType FORMULA will skip this method
        /// </summary>
        public void ResolveCellType()
        {
            if (this.Fieldtype == CellType.FORMULA) { return; }
            Type t = this.Value.GetType();
            if (t == typeof(int)) { this.Fieldtype = CellType.NUMBER; }
            else if (t == typeof(float)) { this.Fieldtype = CellType.NUMBER; }
            else if (t == typeof(double)) { this.Fieldtype = CellType.NUMBER; }
            else if (t == typeof(bool)) { this.Fieldtype = CellType.BOOL; }
            else if (t == typeof(DateTime)) { this.Fieldtype = CellType.DATE; }
            else { this.Fieldtype = CellType.STRING; } // Default
        }

        /// <summary>
        /// Gets the cell Address as string in the format A1 - XFD16384
        /// </summary>
        /// <returns>Cell address</returns>
        public string GetCellAddress()
        {
            return Cell.ResolveCellAddress(this.ColumnAddress, this.RowAddress);
        }

        /// <summary>
        /// Sets the style of the cell
        /// </summary>
        /// <param name="style">style to assign</param>
        /// <param name="workbookReference">Workbook reference. All styles will be managed in this workbook</param>
        /// <returns>If the passed style already exists in the workbook, the existing one will be returned, otherwise the passed one</returns>
        public Style SetStyle(Style style, Workbook workbookReference)
        {
            if (workbookReference == null)
            {
                throw new UndefinedStyleException("No workbook reference was defined while trying to set a style to a cell");
            }
            if (style == null)
            {
                throw new UndefinedStyleException("No style to assign was defined");
            }
            Style s = workbookReference.AddStyle(style, true);
            this.cellStyle = s;
            return s;
        }

        /// <summary>
        /// Removes the assigned style from the cell
        /// </summary>
        /// <param name="workbookReference">Workbook reference. All styles will be managed in this workbook</param>
        public void RemoveStyle(Workbook workbookReference)
        {
            if (workbookReference == null)
            {
                throw new UndefinedStyleException("No workbook reference was defined while trying to remove a style from a cell");
            }
            string styleName = this.cellStyle.Name;
            this.cellStyle = null;
            workbookReference.RemoveStyle(styleName, true);
        }


        /// <summary>
        /// Implemented CompareTo method
        /// </summary>
        /// <param name="other">object to compare</param>
        /// <returns>0 if values are the same, -1 if this object is smaller, 1 if it is bigger</returns>
        public int CompareTo(Cell other)
        {
            if (this.RowAddress == other.RowAddress)
            {
                return this.ColumnAddress.CompareTo(other.ColumnAddress);
            }
            else
            {
                return this.RowAddress.CompareTo(other.RowAddress);
            }
        }

        /// <summary>
        /// Converts a List of supported objects into a list of cells
        /// </summary>
        /// <typeparam name="T">Generic data type</typeparam>
        /// <param name="list">List of generic objects</param>
        /// <returns>List of cells</returns>
        public static List<Cell> ConvertArray<T>(List<T> list)
        {
            List<Cell> output = new List<Cell>();
            Cell c;
            object o;
            Type t;
            foreach(T item in list)
            {
                o = (object)item;
                t = typeof(T);

                if (t == typeof(int))
                {
                    c = new Cell((int)o, CellType.NUMBER);
                }
                else if (t == typeof(float))
                {
                    c = new Cell((float)o, CellType.NUMBER);
                }
                else if (t == typeof(double))
                {
                    c = new Cell((double)o, CellType.NUMBER);
                }
                else if (t == typeof(bool))
                {
                    c = new Cell((bool)o, CellType.BOOL);
                }
                else if (t == typeof(DateTime))
                {
                    c = new Cell((DateTime)o, CellType.DATE);
                }
                else if (t == typeof(string))
                {
                    c = new Cell((string)o, CellType.STRING);
                }
                else // Default = unspecified object
                {
                    c = new Cell((string)o, CellType.DEFAULT);
                    /*
                    try
                    {
                       
                    }
                    catch
                    {
                        throw new UnsupportedDataTypeException("The data type '" + typeof(T).ToString() + "' is not supported");
                    }
                    */
                }
                output.Add(c);
            }
            return output;
        }

        /// <summary>
        /// Get a list of cell addresses from a cell range (format A1:B3 or AAD556:AAD1000)
        /// </summary>
        /// <param name="range">Range to process</param>
        /// <returns>List of cell addresses</returns>
        public static List<Address> GetCellRange(string range)
        {
            Address start, end;
            ResolveCellRange(range, out start, out end);
            return GetCellRange(start, end);
        }

        /// <summary>
        /// Get a list of cell addresses from a cell range
        /// </summary>
        /// <param name="startAddress">Start address as string in the format A1 - XFD16384</param>
        /// <param name="endAddress">End address as string in the format A1 - XFD16384</param>
        /// <returns>List of cell addresses</returns>
        public static List<Address> GetCellRange(string startAddress, string endAddress)
        {
            Address start = ResolveCellCoordinate(startAddress);
            Address end = ResolveCellCoordinate(endAddress);
            return GetCellRange(start, end);
        }

        /// <summary>
        /// Get a list of cell addresses from a cell range
        /// </summary>
        /// <param name="startColumn">Start column (zero based)</param>
        /// <param name="startRow">Start row (zero based)</param>
        /// <param name="endColumn">End column (zero based)</param>
        /// <param name="endRow">End row (zero based)</param>
        /// <returns>List of cell addresses</returns>
        public static List<Address> GetCellRange(int startColumn, int startRow, int endColumn, int endRow)
        {
            Address start = new Address(startColumn, startRow);
            Address end = new Address(endColumn, endRow);
            return GetCellRange(start, end);
        }

        /// <summary>
        /// Get a list of cell addresses from a cell range
        /// </summary>
        /// <param name="startAddress">Start address</param>
        /// <param name="endAddress">End address</param>
        /// <returns>List of cell addresses</returns>
        public static List<Address> GetCellRange(Address startAddress, Address endAddress)
        {
            int startColumn, endColumn, startRow, endRow;
            if (startAddress.Column < endAddress.Column)
            {
                startColumn = startAddress.Column;
                endColumn = endAddress.Column;
            }
            else
            {
                startColumn = endAddress.Column;
                endColumn = startAddress.Column;
            }
            if (startAddress.Row < endAddress.Row)
            {
                startRow = startAddress.Row;
                endRow = endAddress.Row;
            }
            else
            {
                startRow = endAddress.Row;
                endRow = startAddress.Row;
            }
            List<Address> output = new List<Address>();
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    output.Add(new Address(j, i));
                }
            }
            return output;
        }

        /// <summary>
        /// Resolves a cell range from the format like  A1:B3 or AAD556:AAD1000
        /// </summary>
        /// <param name="range">Range to process</param>
        /// <param name="startAddress">Start address as out parameter</param>
        /// <param name="endAddress">End address as out parameter</param>
        /// <exception cref="FormatException">Throws a FormatException if the start or end address was malformed</exception>
        public static void ResolveCellRange(string range, out Address startAddress, out Address endAddress)
        {
            if (string.IsNullOrEmpty(range))
            {
                throw new FormatException("The cell range is null or empty and could not be resolved");
            }
            string[] split = range.Split(':');
            if (split.Length != 2)
            {
                throw new FormatException("The cell range (" + range + ") is malformed and could not be resolved");
            }
            startAddress = ResolveCellCoordinate(split[0]);
            endAddress = ResolveCellCoordinate(split[1]);
        }

        /// <summary>
        /// Gets the address of a cell by the column and row number (zero based)
        /// </summary>
        /// <param name="column">Column address of the cell (zero-based)</param>
        /// <param name="row">Row address of the cell (zero-based)</param>
        /// <exception cref="OutOfRangeException">Throws a OutOfRangeException if the start or end address was out of range</exception>
        /// <returns>Cell Address as string in the format A1 - XFD16384</returns>
        public static string ResolveCellAddress(int column, int row)
        {
            if (row >= 1048576 || row < 0)
            {
                throw new OutOfRangeException("The row number (" + row.ToString() + ") is out of range. Range is from 0 to 1048575 (1048576 rows).");
            }
            if (column >= 16384 || column < 0)
            {
                throw new OutOfRangeException("The column number (" + column.ToString() + ") is out of range. Range is from 0 to 16383 (16384 columns).");
            }
            // A - XFD
            int j = 0;
            int k = 0;
            int l = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= column; i++)
            {
                if (j > 25)
                {
                    k++;
                    j = 0;
                }
                if (k > 25)
                {
                    l++;
                    k = 0;
                }
                j++;
            }
            if (l > 0) { sb.Append((char)(l + 64)); }
            if (k > 0) { sb.Append((char)(k + 64)); }
            sb.Append((char)(j + 64));
            sb.Append((row + 1).ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Gets the column and row number (zero based) of a cell by the address
        /// </summary>
        /// <param name="address">Address as string in the format A1 - XFD16384</param>
        /// <param name="column">Column address of the cell (zero-based) as out parameter</param>
        /// <param name="row">Row address of the cell (zero-based) as out parameter</param>
        /// <exception cref="FormatException">Throws a FormatException if the range address was malformed</exception>
        /// <exception cref="OutOfRangeException">Throws a OutOfRangeException if the start or end address was out of range</exception>
        public static void ResolveCellCoordinate(string address, out int column, out int row)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new FormatException("The cell address is null or empty and could not be resolved");
            }
            address = address.ToUpper();
            Regex rx = new Regex("([A-Z]{1,3})([0-9]{1,7})");
            Match mx = rx.Match(address);
            if (mx.Groups.Count != 3)
            {
                throw new FormatException("The format of the cell address (" + address + ") is malformed");
            }
            int digits = int.Parse(mx.Groups[2].Value);
            column = ResolveColumn(mx.Groups[1].Value);
            row = digits - 1;
            if (row >= 1048576 || row < 0)
            {
                throw new OutOfRangeException("The row number (" + row.ToString() + ") is out of range. Range is from 0 to 1048575 (1048576 rows).");
            }
            if (column >= 16384 || column < 0)
            {
                throw new OutOfRangeException("The column number (" + column.ToString() + ") is out of range. Range is from 0 to 16383 (16384 columns).");
            }
        }

        /// <summary>
        /// Gets the column number from the column address (A - XFD)
        /// </summary>
        /// <param name="columnAddress">Column address (A - XFD)</param>
        /// <returns>Column number (zero-based)</returns>
        public static int ResolveColumn(string columnAddress)
        {
            int temp;
            int result = 0;
            int multiplicator = 1;
            for (int i = columnAddress.Length - 1; i >= 0; i--)
            {
                temp = (int)columnAddress[i];
                temp = temp - 64;
                result = result + (temp * multiplicator);
                multiplicator = multiplicator * 26;
            }
            if (result - 1 >= 16384 || result - 1 < 0)
            {
                throw new OutOfRangeException("The column number (" + (result - 1).ToString() + ") is out of range. Range is from 0 to 16383 (16384 columns).");
            }
            return result - 1;
        }

        /// <summary>
        /// Gets the column and row number (zero based) of a cell by the address
        /// </summary>
        /// <param name="address">Address as string in the format A1 - XFD16384</param>
        /// <returns>Struct with row and column</returns>
        public static Address ResolveCellCoordinate(string address)
        {
            int row, column;
            ResolveCellCoordinate(address, out column, out row);
            return new Address(column, row);
        }

        /// <summary>
        /// Struct representing the cell address as column and row (zero based)
        /// </summary>
        public struct Address
        {
            /// <summary>
            /// Row number (zero based)
            /// </summary>
            public int Row;
            /// <summary>
            /// Column number (zero based)
            /// </summary>
            public int Column;

            /// <summary>
            /// Constructor with arguments
            /// </summary>
            /// <param name="column">Column number (zero based)</param>
            /// <param name="row">Row number (zero based)</param>
            public Address(int column, int row)
            {
                Column = column;
                Row = row;
            }

            /// <summary>
            /// Returns the combined Address
            /// </summary>
            /// <returns>Address as string in the format A1 - XFD16384</returns>
            public string GetAddress()
            {
                return ResolveCellAddress(Column, Row);
            }

            /// <summary>
            /// Overwritten ToString method
            /// </summary>
            /// <returns>Returns the cell address(e.g. 'A15')</returns>
            public override string ToString()
            {
                return GetAddress();
            }
            
        }

    }
}