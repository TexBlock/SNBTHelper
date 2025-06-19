using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Snbt.Library
{
    public class SnbtReader
    {
        private string text;
        private int index;

        public SnbtReader(string t)
        {
            // Annotation filter
            t = t.Replace("\r", "");
            t = Regex.Replace(t, "^\\s+?//.*$", "");
            t = Regex.Replace(t, "^\\s+?#.*$", "");
            text = t;
            index = 0;
        }

        public char? Next()
        {
            index++;
            if (index - 1 >= text.Length)
            {
                return null;
            }
            if (char.IsWhiteSpace(text[index - 1]) && text[index - 1] != '\n')
            {
                return Next();
            }
            return text[index - 1];
        }

        public char? Snext()
        {
            index++;
            if (index - 1 >= text.Length)
            {
                return null;
            }
            return text[index - 1];
        }

        public char GetPoint()
        {
            return text[index - 1];
        }

        public char Last()
        {
            index--;
            return text[index - 1];
        }
    }

    public enum Token
    {
        EMPTY = -1,
        BEGIN_DICT = 0,
        COLON = 2,
        END_DICT = 4,
        BEGIN_LIST = 5,
        END_LIST = 6,
        ENTER = 7,
        STRING = 8,
        STRING_IN_QUOTE = 999,
        NUMBER = 9,
        BOOL = 10,
        INTEGER = 11
    }

    public class TokenElement
    {
        public Token type = Token.EMPTY;
        public string value = "";
    }

    public class TokenIterator
    {
        private List<TokenElement> TokenList;
        private int index;

        public TokenIterator(List<TokenElement> l)
        {
            TokenList = l;
            index = 0;
        }

        public TokenElement Next()
        {
            if (index >= TokenList.Count)
            {
                return null;
            }
            var token = TokenList[index];
            index++;
            return token;
        }

        public TokenElement Last()
        {
            index--;
            return TokenList[index - 1];
        }
    }

    public class SnbtFileHelper
    {
        public static object Loads(string file, bool format = false)
        {
            var snbtToken = SnbtToTokenList(file);
            object snbtDict = null;
            var iterator = new TokenIterator(snbtToken);
            while (true)
            {
                var i = iterator.Next();
                if (i == null)
                {
                    break;
                }
                if (i.type == Token.BEGIN_DICT)
                {
                    snbtDict = DictIterator(iterator);
                    break;
                }
                else if (i.type == Token.BEGIN_LIST)
                {
                    snbtDict = ListIterator(iterator);
                    break;
                }
            }
            if (format)
            {
                return JsonConvert.SerializeObject(snbtDict, Formatting.Indented);
            }
            return snbtDict;
        }

        public static string Dumps(object jsonObj, int indent = 0, bool compact = false)
        {
            if (jsonObj is string jsonString)
            {
                jsonObj = JsonConvert.DeserializeObject(jsonString);
            }

            StringBuilder text = new StringBuilder();

            if (jsonObj is Dictionary<string, object> dict)
            {
                if (dict.Count == 0)
                {
                    text.Append("{ }\n");
                }
                else
                {
                    text.Append("{\n");
                    indent++;
                    foreach (var kvp in dict)
                    {
                        text.Append(new string('\t', indent));
                        text.Append(kvp.Key);
                        text.Append(": ");
                        text.Append(TypeReturn(kvp.Value, indent));
                    }
                    indent--;
                    text.Append(new string('\t', indent));
                    text.Append("}\n");
                }
            }
            else if (jsonObj is List<object> list)
            {
                if (list.Count == 0)
                {
                    text.Append("[ ]\n");
                }
                else if (list.Count == 1 && !(list[0] is Dictionary<string, object> || list[0] is List<object>))
                {
                    text.Append("[");
                    text.Append(TypeReturn(list[0], 0).TrimEnd('\n'));
                    text.Append("]\n");
                }
                else
                {
                    if (list.Count > 0 && list[0] is string firstItem && firstItem == "I;")
                    {
                        text.Append("[I;\n");
                        list = list.GetRange(1, list.Count - 1);
                    }
                    else
                    {
                        text.Append("[\n");
                    }
                    indent++;
                    foreach (var item in list)
                    {
                        text.Append(new string('\t', indent));
                        text.Append(TypeReturn(item, indent));
                    }
                    indent--;
                    text.Append(new string('\t', indent));
                    text.Append("]\n");
                }
            }

            return compact ? Compatible(text.ToString()) : text.ToString();
        }

        private static Dictionary<string, object> DictIterator(TokenIterator token)
        {
            var tdict = new Dictionary<string, object>();
            string key = "";
            while (true)
            {
                var i = token.Next();
                if (i == null)
                {
                    break;
                }
                if (i.type == Token.COLON)
                {
                    var nextI = token.Next();
                    if (nextI.type == Token.BEGIN_DICT)
                    {
                        tdict[key] = DictIterator(token);
                    }
                    else if (nextI.type == Token.BEGIN_LIST)
                    {
                        tdict[key] = ListIterator(token);
                    }
                    else if (new[] { Token.BOOL, Token.STRING, Token.NUMBER, Token.STRING_IN_QUOTE }.Contains(nextI.type))
                    {
                        tdict[key] = nextI.value;
                    }
                }
                else if (i.type == Token.END_DICT)
                {
                    break;
                }
                key = i.value;
                if (key.StartsWith("$number$"))
                {
                    key = key.Substring(8);
                }
                if (i.type == Token.STRING_IN_QUOTE)
                {
                    key = $"\"{key}\"";
                }
            }
            return tdict;
        }

        public static object ListIterator(TokenIterator token)
        {
            var tlist = new List<object>();
            var firstItem = token.Next();
            if (firstItem != null && firstItem.value == "B;")
            {
                var byteList = new List<byte>();
                TokenElement i;
                while ((i = token.Next()) != null && i.type != Token.END_LIST)
                {
                    var value = i.value.Substring(0, i.value.Length - 1);
                    byteList.Add((byte)int.Parse(value));
                }
                return byteList.ToArray();
            }
            else
            {
                if (firstItem != null)
                {
                    token.Last();
                }
            }
            TokenElement item;
            while ((item = token.Next()) != null)
            {
                if (item.type == Token.BEGIN_DICT)
                {
                    tlist.Add(DictIterator(token));
                }
                else if (item.type == Token.BEGIN_LIST)
                {
                    tlist.Add(ListIterator(token));
                }
                else if (new[] { Token.BOOL, Token.STRING, Token.NUMBER, Token.INTEGER, Token.STRING_IN_QUOTE }.Contains(item.type))
                {
                    tlist.Add(item.value);
                }
                else if (item.type == Token.END_LIST)
                {
                    break;
                }
            }
            return tlist;
        }

        private static List<TokenElement> SnbtToTokenList(string t)
        {
            var tokenList = new List<TokenElement>();
            var reader = new SnbtReader(t);
            while (true)
            {
                var i = reader.Next();
                if (i == null)
                {
                    break;
                }
                var token = new TokenElement();
                switch (i.Value)
                {
                    case '{':
                        token.type = Token.BEGIN_DICT;
                        token.value = "{";
                        break;
                    case '[':
                        token.type = Token.BEGIN_LIST;
                        token.value = "[";
                        break;
                    case ':':
                        token.type = Token.COLON;
                        token.value = ":";
                        break;
                    case ']':
                        token.type = Token.END_LIST;
                        token.value = "]";
                        break;
                    case '}':
                        token.type = Token.END_DICT;
                        token.value = "}";
                        break;
                    case ',':
                    case '\n':
                        token.type = Token.ENTER;
                        token.value = "\n";
                        break;
                    default:
                        if (char.IsDigit(i.Value) || i.Value == '-')
                        {
                            token.type = Token.NUMBER;
                            token.value = NumberBuilder(reader);
                        }
                        else
                        {
                            (token.value, token.type) = StringBuilder(reader);
                        }
                        break;
                }
                tokenList.Add(token);
            }
            return tokenList;
        }

        private static string NumberBuilder(SnbtReader r)
        {
            var s = new StringBuilder();
            s.Append(r.GetPoint());
            while (true)
            {
                var i = r.Next();
                if (i == null || "}],\n:".Contains(i.Value) || char.IsWhiteSpace(i.Value))
                {
                    r.Last();
                    break;
                }
                s.Append(i.Value);
            }
            return "$number$" + s.ToString();
        }

        private static (string, Token) StringBuilder(SnbtReader r)
        {
            var s = new StringBuilder();
            var type = Token.STRING;
            if (r.GetPoint() == '\"')
            {
                type = Token.STRING_IN_QUOTE;
                while (true)
                {
                    var i = r.Snext();
                    if (i == null)
                    {
                        break;
                    }
                    if (i.Value == '\\')
                    {
                        s.Append('\\');
                        var next = r.Snext();
                        if (next != null)
                        {
                            s.Append(next.Value);
                        }
                        continue;
                    }
                    else if (i.Value == '\"')
                    {
                        break;
                    }
                    s.Append(i.Value);
                }
            }
            else
            {
                r.Last();
                while (true)
                {
                    var i = r.Next();
                    if (i == null || "},\n:[]".Contains(i.Value) || char.IsWhiteSpace(i.Value))
                    {
                        r.Last();
                        break;
                    }
                    s.Append(i.Value);
                }
            }
            if (type == Token.STRING && (s.ToString() == "true" || s.ToString() == "false"))
            {
                (string, Token) value = (s.ToString(), Token.BOOL);
                return value;
            }
            return (s.ToString(), type);
        }

        private static string TypeReturn(object value, int indent)
        {
            StringBuilder text = new StringBuilder();
            if (value is Dictionary<string, object> || value is List<object>)
            {
                text.Append(Dumps(value, indent));
            }
            else if (value is string str)
            {
                if (str.StartsWith("$number$"))
                {
                    text.Append(str.Substring(8));
                    text.Append("\n");
                }
                else
                {
                    text.Append($"\"{str}\"\n");
                }
            }
            else if (value is bool boolValue)
            {
                text.Append(boolValue ? "true" : "false");
                text.Append("\n");
            }
            else if (value is byte[] byteArray)
            {
                text.Append("[B;\n");
                foreach (var b in byteArray)
                {
                    text.Append(new string('\t', indent + 1));
                    text.Append(b);
                    text.Append("b\n");
                }
                text.Append(new string('\t', indent));
                text.Append("]\n");
            }
            return text.ToString();
        }

        private static string Compatible(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]) && !"[{ ".Contains(lines[i][lines[i].Length - 1]) && !string.IsNullOrEmpty(lines[i + 1]) && !"}] ".Contains(lines[i + 1].TrimStart()[0]))
                {
                    lines[i] += ",";
                }
            }
            return string.Join("\n", lines);
        }
    }
}