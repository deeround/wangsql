using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WangSql
{
    public class JsonReader
    {
        private TextReader _textReader = null;
        private int _endCount = 0;
        public Dictionary<string, object> Value = new Dictionary<string, object>();
        private JsonReader(TextReader reader, int endCount = 0)
        {
            Value = new Dictionary<string, object>();
            _textReader = reader;
            _endCount = endCount;
            int intByte = ReadInt();
            while (_endCount != 0 && intByte != -1)
            {
                var key = ReadKeyName(intByte);
                var value = ReadValue();
                Value.Add(key, value);
                if (_endCount == 0) { break; }
                intByte = ReadInt();
            }
        }
        public JsonReader(TextReader reader)
            : this(reader, 0)
        { }
        public JsonReader(string value)
            : this(new StringReader(value), 0)
        { }
        private int ReadInt()
        {
            var intByte = _textReader.Read();
            while (intByte == 32)
            {
                intByte = _textReader.Read();
            }
            if (intByte == 123)
            {
                _endCount++;
            }
            else if (intByte == 125)
            {
                _endCount--;
            }
            return intByte;
        }
        private string ReadKeyName(int lastChar)
        {
            StringBuilder strBuilder = new StringBuilder();
            var intByte = lastChar;
            if (intByte == 123)
            {
                intByte = _textReader.Read();
            }
            var lastIntByte = -1;
            int endByteInt = -1;
            if (intByte == -1)
            {
                return null;
            }
            while (intByte == 32)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            if (intByte == 34 || intByte == 39)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            while (intByte != -1)
            {
                if (endByteInt != -1)
                {
                    if (intByte == endByteInt && lastIntByte != 92)
                    {
                        ReadInt();
                        break;
                    }
                }
                else if (intByte == 58)
                {
                    break;
                }

                strBuilder.Append((char)intByte);
                intByte = _textReader.Read();
            }
            return strBuilder.ToString().Trim();
        }
        private object ReadValue()
        {
            var intByte = _textReader.Read();
            while (intByte == 32)
            {
                intByte = _textReader.Read();
            }
            if (intByte == 123)
            {
                var item = new JsonReader(_textReader, 1).Value;
                ReadInt();
                return item;
            }
            else if (intByte == 91)
            {
                return ReadValueArray();
            }
            else
            {
                return ReadValueString(intByte);
            }
        }
        private string ReadValueArrayString(ref int lastChar)
        {
            StringBuilder strBuilder = new StringBuilder();
            var intByte = lastChar;
            if (intByte == 123)
            {
                intByte = _textReader.Read();
            }
            var lastIntByte = -1;
            int endByteInt = -1;
            if (intByte == -1)
            {
                return null;
            }
            while (intByte == 32 || intByte == 34 || intByte == 39)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            while (intByte != -1)
            {
                lastChar = intByte;
                if (endByteInt != -1)
                {
                    if (intByte == endByteInt && lastIntByte != 92)
                    {
                        break;
                    }
                }
                else if (intByte == 44 || (intByte == 93 && lastIntByte != 92))
                {
                    break;
                }

                strBuilder.Append((char)intByte);
                intByte = ReadInt();
            }
            return strBuilder.ToString();
        }
        private object ReadValueString(int lastChar)
        {
            StringBuilder strBuilder = new StringBuilder();
            var intByte = lastChar;
            if (intByte == 123)
            {
                intByte = _textReader.Read();
            }
            var lastIntByte = -1;
            int endByteInt = -1;
            bool isString = true;
            if (intByte == -1)
            {
                return null;
            }
            while (intByte == 32)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            if (intByte == 34 || intByte == 39)
            {
                endByteInt = intByte;
                intByte = _textReader.Read();
            }
            while (intByte != -1)
            {
                if (endByteInt != -1)
                {
                    if (intByte == endByteInt && lastIntByte != 92)
                    {
                        ReadInt();
                        break;
                    }
                }
                else if (intByte == 44 || (intByte == 125 && lastIntByte != 92))
                {
                    isString = false;
                    break;
                }
                strBuilder.Append((char)intByte);
                intByte = ReadInt();
            }
            return isString ? strBuilder.ToString() : strBuilder.ToString().Trim();
        }
        private object[] ReadValueArray()
        {
            List<object> list = new List<object>();
            var intByte = _textReader.Read();
            while (intByte != 93)
            {
                if (intByte == 123)
                {
                    var item = new JsonReader(_textReader, 1).Value;
                    list.Add(item);
                    if (ReadInt() == 93)
                    {
                        break;
                    }
                }
                else if (intByte == 91)
                {
                    list.Add(ReadValueArray());
                }
                else
                {
                    list.Add(ReadValueArrayString(ref intByte));
                    if (intByte == 93) { break; }
                }
                intByte = _textReader.Read();
            }
            return list.ToArray();
        }
    }
}