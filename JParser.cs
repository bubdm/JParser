/*
 *  Project     : JParser
 *  File        : JParser.cs
 *  Developer   : Rahil Parikh ( rahil@rahilparikh.me )
 *  Date        : Oct 14, 2012
 *  
 *  Copyright (c) 2012, Rahil Parikh
 *  
 *  Note - This program depends on JSON.NET library for .NET v4
 *
 *  As long as you retain this notice and credit author
 *  for his work you can do whatever you want with this
 *  stuff. 
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 *  
 *  ToDo -
 *  1. Add support for DataSets
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace JParser
{
    /// <summary>
    /// JParser class acts as a highlevel
    /// wrapper for Json.Net library and 
    /// extends existing functionality
    /// </summary>
    sealed public class JParser
    {
        Dictionary<string, string> dict;
        List<string> keys;
        JObject jObj;
        string jsonString;
        string parentKey;
        string delim;
        XmlDocument xDoc;
        XmlElement xEle;
        XmlWriter xWriter;
        XmlWriterSettings xWSettings;
        int nextIndexCount;


        /// <summary>
        /// Gets the element specified key
        /// </summary>
        /// <param name="Key"><para>Key</para> is FQN of specific JSON key</param>
        /// <returns>Returns element value or, if not found, <code>null</code></returns>
        public string this[string Key]
        {
            get
            {
                if (Contains(Key))
                {
                    return dict[Key];
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the element at <para>Index</para>th location
        /// </summary>
        /// <param name="Index">Zero based index of element to get</param>
        /// <returns>Returns element value or, if not found, <code>null</code></returns>
        public string this[int Index]
        {
            get
            {
                if (keys.Count > 0)
                {
                    return dict[keys[Index]];
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the number of Key-Value pair in JSON
        /// </summary>
        public int Length
        {
            get
            {
                return keys.Count;
            }
        }

        /// <summary>
        /// Take JSON string and convert to
        /// JObject for further processing
        /// </summary>
        /// <param name="JsonString">Json string that needs to be processed</param>
        /// <param name="Delim">Specifies the delimiter to use during Dictionary processing. If omitted, '_' ( underscore ) will be used as a default delimiter.</param>
        public JParser(string JsonString, char Delim = '_')
        {
            try
            {
                if(Regex.IsMatch(Delim.ToString(), @"[#$%\+\-\.:=_\|~]"))
                {
                    delim = Delim.ToString();
                }
                else
                {
                    delim = "_";
                }

                nextIndexCount = 0;
                parentKey = null;
                dict = new Dictionary<string, string>();
                jsonString = JsonString;
                jObj = JObject.Parse(JsonString);
                parseJTokens(jObj.ToArray<JToken>());
                keys = dict.Keys.ToList<string>();

                xWSettings = new XmlWriterSettings();
                xWSettings.CheckCharacters = true;
                xWSettings.Indent = true;
                xWSettings.NewLineChars = System.Environment.NewLine;
                xWSettings.OmitXmlDeclaration = false;
            }
            catch (Exception e)
            {
                throw (new InvalidJsonException("Error occured while parsing JSON.\r\nJSON --\r\n" + JsonString));
            }
        }

        /// <summary>
        /// Determine whether specified key exists in Json
        /// </summary>
        /// <param name="Key"><para>Key</para> is a FQN of Json Key</param>
        /// <returns></returns>
        public bool Contains(string Key)
        {
            return keys.Contains(Key);
        }

        /// <summary>
        /// Get value for specified JSON key
        /// </summary>
        /// <param name="Key"><para>Key</para> is a FQN of Json Key</param>
        /// <returns></returns>
        public string GetValue(string Key)
        {
            string value = null;
            if (dict.ContainsKey(Key))
            {
                value = dict[Key];
            }
            return value;
        }

        /// <summary>
        /// Returns current value and advance the pointer
        /// </summary>
        /// <returns></returns>
        public string Next()
        {
            return Next(false);
        }

        /// <summary>
        /// Returns current value and advance the pointer
        /// </summary>
        /// <param name="ResetPointer">Resets the pointer to the first record</param>
        /// <returns></returns>
        public string Next(bool ResetPointer)
        {
            if (nextIndexCount >= dict.Count)
            {
                if (ResetPointer)
                {
                    nextIndexCount = 0;
                }
                return null;
            }
            return this[this[nextIndexCount++]];
        }

        /// <summary>
        /// Creates a <code>Dictionary<![CDATA[<string,string>]]></code> containing all Key-Value 
        /// pairs in JSON
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ToDictionary()
        {
            return dict;
        }

        /// <summary>
        /// Creates a <code>List<![CDATA[<string>]]></code> containing all 
        /// Keys in JSON
        /// </summary>
        /// <returns></returns>
        public List<string> ToList()
        {
            if (keys == null)
            {
                keys = dict.Keys.ToList();
            }
            return keys;
        }

        /// <summary>
        /// Converts JSON into XML String
        /// </summary>
        /// <returns></returns>
        public string ToXMLString()
        {
            if (xDoc == null)
            {
                ToXMLDocument();
            }
            return xDoc.OuterXml;
        }

        /// <summary>
        /// Converts JSON into XML Document
        /// </summary>
        /// <returns></returns>
        public XmlDocument ToXMLDocument()
        {
            xDoc = new XmlDocument();
            xDoc.AppendChild(xDoc.CreateXmlDeclaration("1.0", null, null));
            xEle = xDoc.CreateElement("root");
            xDoc.AppendChild(xEle);
            parseJTokensToXML(jObj.ToArray<JToken>(), xEle);
            return xDoc;
        }

        /// <summary>
        /// Converts JSON into XmlTextReader
        /// object for Xml reading
        /// </summary>
        /// <returns></returns>
        public XmlTextReader ToXMLTextReader()
        {
            XmlTextReader xReader = new XmlTextReader(new StringReader(ToXMLString()));
            return xReader;
        }

        public DataSet ToDataSet()
        {
            return ToDataSet("Default");
        }

        public DataSet ToDataSet(string DatabaseName)
        {
            throw new Exception("Not yet implemented. Reserved for future release.");
        }

        /// <summary>
        /// Convert JSON into XML
        /// and write it to the file
        /// </summary>
        /// <param name="FilePath">The path to the XML file</param>
        /// <param name="FormatXML">Specifies if output XML should be formatted</param>
        public void WriteToXML(string FilePath, bool FormatXML)
        {
            XmlWriter xW = XmlWriter.Create(FilePath, xWSettings);
            xW.Settings.Indent = FormatXML;
            WriteToXML(xW, false);
        }

        /// <summary>
        /// Convert JSON into XML
        /// and write it to the file
        /// </summary>
        /// <param name="XmlWriter"></param>
        public void WriteToXML(XmlWriter XmlWriter)
        {
            WriteToXML(XmlWriter, false);
        }

        /// <summary>
        /// Convert JSON into XML
        /// and write it to the file
        /// </summary>
        /// <param name="XWriter"></param>
        /// <param name="HasRootElement">Specifies whether XWriter has already specified XmlDocument Type and XML Root Element. Default value is false.</param>
        public void WriteToXML(XmlWriter XWriter, bool HasRootElement = false)
        {
            if (XWriter == null)
                throw new NullReferenceException("XWriter parameter should be initialized with non-null value.");

            xWriter = XWriter;
            if (!HasRootElement)
            {
                xWriter.WriteStartDocument();
                xWriter.WriteStartElement("root");
                parseJTokensToXML(jObj.ToArray<JToken>());
                xWriter.WriteEndElement();
                xWriter.WriteEndDocument();
            }
        }

        private void parseJTokens(JToken[] jToken)
        {
            //Parse array of values
            foreach (JToken jT in jToken)
            {
                if (jT != null)
                {
                    //Each JSON key-value pair is JPropery
                    JProperty jProp = (JProperty)jT;
                    string keyName = jProp.Name;

                    parentKey = parentKey + delim + keyName;

                    //Check if value of current key 
                    //is sub dictionary of objects
                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        //We have dictionary
                        //Split it in array
                        //for further processing
                        parseJTokens(jProp.Value.ToArray<JToken>());
                        parentKey = parentKey.Substring(0, parentKey.LastIndexOf(delim + keyName));
                    }
                    //Check if value of 
                    //current key is array
                    else if (jProp.Value.Type == JTokenType.Array)
                    {
                        JArray jA = (JArray)jProp.Value;
                        for (int jArrayIndex = 0; jArrayIndex < jA.Count; jArrayIndex++)
                        {
                            //Split the array into subarrays
                            //to generate sub key-value pair
                            parentKey = parentKey + delim.ToString() + jArrayIndex;
                            parseJTokens(jA[jArrayIndex].ToArray<JToken>());
                            parentKey = parentKey.Substring(0, parentKey.LastIndexOf(delim + jArrayIndex));
                        }
                    }
                    else if (jProp.Value.Type == JTokenType.Comment)
                    {
                        //Do nothing
                    }
                    else
                    {
                        //We have a key value pair
                        dict.Add(parentKey, jProp.Value.ToString());
                        parentKey = parentKey.Substring(0, parentKey.LastIndexOf(delim + keyName));
                    }
                }
            }
        }

        private void parseJTokensToXML(JToken[] jToken)
        {
            //Parse array of values
            foreach (JToken jT in jToken)
            {
                if (jT != null)
                {
                    //Each JSON key-value pair is JPropery
                    JProperty jProp = (JProperty)jT;
                    string keyName = jProp.Name;

                    xWriter.WriteStartElement(keyName);

                    //Check if value of current key 
                    //is sub dictionary of objects
                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        //We have dictionary
                        //Split it in array
                        //for further processing
                        parseJTokensToXML(jProp.Value.ToArray<JToken>());
                    }
                    //Check if value of 
                    //current key is array
                    else if (jProp.Value.Type == JTokenType.Array)
                    {
                        JArray jA = (JArray)jProp.Value;
                        for (int jArrayIndex = 0; jArrayIndex < jA.Count; jArrayIndex++)
                        {
                            //Split the array into subarrays
                            //to generate sub key-value pair
                            xWriter.WriteStartElement(String.Format("ArrayElement_{0}", jArrayIndex.ToString()));
                            parseJTokensToXML(jA[jArrayIndex].ToArray<JToken>());
                            xWriter.WriteEndElement();
                        }
                    }
                    else if (jProp.Value.Type == JTokenType.Comment)
                    {
                        //Do nothing
                    }
                    else
                    {
                        //We have a key value pair
                        xWriter.WriteString(jProp.Value.ToString());
                    }
                    xWriter.WriteEndElement();
                }
            }
        }

        private void parseJTokensToXML(JToken[] jToken, XmlNode xNode)
        {
            //Parse array of values
            foreach (JToken jT in jToken)
            {
                if (jT != null)
                {
                    //Each JSON key-value pair is JPropery
                    JProperty jProp = (JProperty)jT;
                    string keyName = jProp.Name;

                    XmlElement xChildEle = xDoc.CreateElement(keyName);
                    xNode.AppendChild(xChildEle);

                    //Check if value of current key 
                    //is sub dictionary of objects
                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        //We have dictionary
                        //Split it in array
                        //for further processing
                        parseJTokensToXML(jProp.Value.ToArray<JToken>(), xChildEle);
                    }
                    //Check if value of 
                    //current key is array
                    else if (jProp.Value.Type == JTokenType.Array)
                    {
                        JArray jA = (JArray)jProp.Value;
                        for (int jArrayIndex = 0; jArrayIndex < jA.Count; jArrayIndex++)
                        {
                            //Split the array into subarrays
                            //to generate sub key-value pair
                            XmlElement xChildArrayEle = xDoc.CreateElement(String.Format("ArrayElement_{0}", jArrayIndex.ToString()));
                            xChildEle.AppendChild(xChildArrayEle);
                            parseJTokensToXML(jA[jArrayIndex].ToArray<JToken>(), xChildArrayEle);
                        }
                    }
                    else if (jProp.Value.Type == JTokenType.Comment)
                    {
                        //Do nothing
                    }
                    else
                    {
                        //We have a key value pair
                        xChildEle.InnerText = jProp.Value.ToString();
                    }
                }
            }
        }

        private void parseJTokensToDataSet(JToken[] jToken, int NormalizationLevel)
        {
            throw new Exception("Not yet implemented. Reserved for future release.");
        }
    }
}