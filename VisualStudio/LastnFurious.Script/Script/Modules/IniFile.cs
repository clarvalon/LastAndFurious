// Module_IniFile - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_IniFile;
using static LastnFurious.IniFileStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_IniFile
    {
        // Fields

        // Methods
        public String ExtensionMethod_Trim(String  thisItem)
        {
            int left = 0;
            int right = thisItem.Length - 1;
            while ((left < right)   && ((thisItem.Chars()[left] == ' ')   || (thisItem.Chars()[left] == 9)))
                left += 1;
            while ((right > left)   && ((thisItem.Chars()[right] == ' ')   || (thisItem.Chars()[right] == 9)))
                right -= 1;
            if (left > right)
                return "";
            return thisItem.Substring(left, right - left + 1);
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int INIFILE_BUFFER_SIZE = 250;


    }

    #endregion

    #region IniFile (AGS struct from .ash converted to class)

    public class IniFile
    {
        // Fields
        public String[] lines = new String[INIFILE_BUFFER_SIZE];
        public int length;

        // Methods
        public bool InsertLine(int index)
        {
            if ((this.length == INIFILE_BUFFER_SIZE) || (index < 0) || (index > this.length))
                return false;
            this.length += 1;
            int i = this.length;
            while (i > index)
            {
                i -= 1;
                this.lines[i + 1] = this.lines[i];
            }
            return true;
        }

        public bool DeleteLine(int index)
        {
            if ((index < 0) || (index >= this.length))
                return false;
            this.length -= 1;
            while (index < this.length)
            {
                this.lines[index] = this.lines[index + 1];
                index += 1;
            }
            this.lines[this.length + 1] = null;
            return true;
        }

        public int FindSection(String section)
        {
            int i = 0;
            int  j = 0;
            String str = null;
            section = section.LowerCase();
            while (i < this.length)
            {
                str = this.lines[i];
                if (!String.IsNullOrEmpty(str) && (str.Chars()[0] == '['))
                {
                    j = str.IndexOf("]");
                    if (j > 1)
                    {
                        str = str.Substring(1, j - 1);
                        if (str.LowerCase() == section)
                            return i;
                    }
                }
                i += 1;
            }
            return i;
        }

        public int FindKey(String section, String key)
        {
            int i = this.FindSection(section) + 1, j;
            String str = null;
            key = key.LowerCase();
            while (i < this.length)
            {
                str = this.lines[i];
                if (!String.IsNullOrEmpty(str))
                {
                    if (str.Chars()[0] == '[')
                        return this.length;
                    j = str.IndexOf(";");
                    if (j >= 0)
                        str = str.Truncate(j);
                    j = str.IndexOf("=");
                    if (j > 0)
                    {
                        str = str.Truncate(j);
                        str = str.Trim();
                        if (str.LowerCase() == key)
                            return i;
                    }
                }
                i += 1;
            }
            return this.length;
        }

        public int FindLastKey(String section)
        {
            int i = this.FindSection(section);
            int last = i;
            int  j = 0;
            String str = null;
            if (i == this.length)
            {
                if (i + 2 > INIFILE_BUFFER_SIZE)
                    return this.length;
                this.length += 2;
                this.lines[i] = "";
                this.lines[i + 1] = StringFormatAGS("[%s]", section);
                return i + 1;
            }
            i += 1;
            while (i < this.length)
            {
                str = this.lines[i];
                if (!String.IsNullOrEmpty(str))
                {
                    if (str.Chars()[0] == '[')
                        return last;
                    j = str.IndexOf(";");
                    if (j >= 0)
                        str = str.Truncate(j);
                    j = str.IndexOf("=");
                    if (j > 0)
                        last = i;
                }
                i += 1;
            }
            return last;
        }

        public void Clear()
        {
            while (this.length > 0)
            {
                this.length -= 1;
                this.lines[this.length] = null;
            }
        }

        public bool Load(String filename)
        {
            File file = File.Open(filename, eFileRead);
            if ((file == null) || (file.Error))
                return false;
            this.Clear();
            while (!file.EOF   && (this.length < INIFILE_BUFFER_SIZE))
            {
                this.lines[this.length] = file.ReadRawLineBack();
                this.length += 1;
            }
            file.Close();
            return true;
        }

        public bool Save(String filename)
        {
            File file = File.Open(filename, eFileWrite);
            if ((file == null) || (file.Error))
                return false;
            int i = 0;
            while (i < this.length)
            {
                file.WriteRawLine(this.lines[i]);
                i += 1;
            }
            file.Close();
            return true;
        }

        public int ListSections(String[] list, int size)
        {
            int count = 0;
            int i = 0;
            int  j = 0;
            String str = null;
            while (i < this.length)
            {
                str = this.lines[i];
                if (!String.IsNullOrEmpty(str) && (str.Chars()[0] == '['))
                {
                    j = str.IndexOf("]");
                    if (j > 1)
                    {
                        if (count < size)
                            list[count] = str.Substring(1, j - 1);
                        count += 1;
                    }
                }
                i += 1;
            }
            return count;
        }

        public int ListKeys(String section, String[] list, int size)
        {
            int count = 0, i = this.FindSection(section) + 1, j;
            String str = null;
            while (i < this.length)
            {
                str = this.lines[i];
                if (!String.IsNullOrEmpty(str))
                {
                    if (str.Chars()[0] == '[')
                        return count;
                    j = str.IndexOf(";");
                    if (j >= 0)
                        str = str.Truncate(j);
                    j = str.IndexOf("=");
                    if (j > 0)
                    {
                        if (count < size)
                        {
                            str = str.Truncate(j);
                            list[count] = str.Trim();
                        }
                        count += 1;
                    }
                }
                i += 1;
            }
            return count;
        }

        public bool SectionExists(String section)
        {
            return (this.FindSection(section) != this.length);
        }

        public void DeleteSection(String section)
        {
            int i = this.FindSection(section);
            if (i == this.length)
                return;
            int last = this.FindLastKey(section) + 1;
            while (last < this.length)
            {
                this.lines[i] = this.lines[last];
                i += 1;
                last += 1;
            }
            while (this.length > i)
            {
                this.length -= 1;
                this.lines[this.length] = null;
            }
        }

        public bool KeyExists(String section, String key)
        {
            return (this.FindKey(section, key) != this.length);
        }

        public void DeleteKey(String section, String key)
        {
            int i = this.FindKey(section, key);
            if (i != this.length)
                this.DeleteLine(i);
        }

        public String Read(String section, String key, String value)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
                return value;
            key = this.lines[i];
            i = key.IndexOf("=") + 1;
            if (i == key.Length)
                return value;
            key = key.Substring(i, key.Length - i);
            key = key.Trim();
            return key;
        }

        public int ReadInt(String section, String key, int value = 0)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
                return value;
            key = this.lines[i];
            i = key.IndexOf("=") + 1;
            if (i == key.Length)
                return value;
            key = key.Substring(i, key.Length - i);
            key = key.Trim();
            return key.AsInt();
        }

        public float ReadFloat(String section, String key, float value = 0)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
                return value;
            key = this.lines[i];
            i = key.IndexOf("=") + 1;
            if (i == key.Length)
                return value;
            key = key.Substring(i, key.Length - i);
            key = key.Trim();
            return key.AsFloat();
        }

        public bool ReadBool(String section, String key, bool value = false)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
                return value;
            key = this.lines[i];
            i = key.IndexOf("=") + 1;
            if (i == key.Length)
                return value;
            key = key.Substring(i, key.Length - i);
            key = key.Trim();
            key = key.LowerCase();
            return ((key == "1") || (key == "true") || (key == "on") || (key == "yes"));
        }

        public bool Write(String section, String key, String value)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
            {
                i = this.FindLastKey(section) + 1;
                if (!this.InsertLine(i))
                    return false;
            }
            this.lines[i] = StringFormatAGS("%s=%s", key, value);
            return true;
        }

        public bool WriteInt(String section, String key, int value)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
            {
                i = this.FindLastKey(section) + 1;
                if (!this.InsertLine(i))
                    return false;
            }
            this.lines[i] = StringFormatAGS("%s=%d", key, value);
            return true;
        }

        public bool WriteFloat(String section, String key, float value)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
            {
                i = this.FindLastKey(section) + 1;
                if (!this.InsertLine(i))
                    return false;
            }
            this.lines[i] = StringFormatAGS("%s=%f", key, value);
            return true;
        }

        public bool WriteBool(String section, String key, bool value)
        {
            int i = this.FindKey(section, key);
            if (i == this.length)
            {
                i = this.FindLastKey(section) + 1;
                if (!this.InsertLine(i))
                    return false;
            }
            this.lines[i] = StringFormatAGS("%s=%d", key, value);
            return true;
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class IniFileStaticRef
    {
        // Static Methods
        public static String Trim(String  thisItem)
        {
            return GlobalBase.IniFileInstance.ExtensionMethod_Trim(thisItem);
        }

    }

    #endregion
    
    #region Extension Methods Wrapper (AGS workaround)

    public static partial class ExtensionMethods
    {
        public static String Trim(this String  thisItem)
        {
            return GlobalBase.IniFileInstance.ExtensionMethod_Trim(thisItem);
        }

    }

    #endregion

}
