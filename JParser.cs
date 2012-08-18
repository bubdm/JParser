/*
 *  Project     : JParser
 *  File        : JParser.cs
 *  Developer   : Rahil Parikh ( rahil@rahilparikh.me )
 *  Date        : August 18, 2012
 *  
 *  Copyright (c) 2012, Rahil Parikh
 *  
 *  Note - This program depends on JSON.NET library for .NET 4
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
 *  1. Add support to access members by name
 *  2. Add support to access members by number
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace JParser
{
    sealed public class JParser
    {
        Dictionary<string, string> dict;
        JObject jObj;
        string parentKey;

        //Take JSON string and convert to
        //JObject for further processing
        public JParser(string JSON)
        {
            dict = new Dictionary<string, string>();
            jObj = JObject.Parse(JSON);
            parentKey = null;
            parseJTokens(jObj.ToArray<JToken>());
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
                    parentKey = parentKey + "_" + keyName;

                    //Check if value of current key 
                    //is sub dictionary of objects
                    if (jProp.Value.Type == JTokenType.Object)
                    {
                        //Console.WriteLine(jProp.Value.Type);
                        //We have dictionary
                        //Split it in array
                        //for further processing
                        parseJTokens(jProp.Value.ToArray<JToken>());
                        parentKey = parentKey.Substring(0, parentKey.LastIndexOf("_" + keyName));
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
                            parentKey = parentKey + "_" + jArrayIndex;
                            parseJTokens(jA[jArrayIndex].ToArray<JToken>());
                            parentKey = parentKey.Substring(0, parentKey.LastIndexOf("_" + jArrayIndex));
                        }
                        //Console.WriteLine(jProp.Value.Type);
                    }
                    else if (jProp.Value.Type == JTokenType.Comment)
                    {
                        //Do nothing
                    }
                    else
                    {
                        //We have a key value pair
                        dict.Add(parentKey, jProp.Value.ToString());
                        parentKey = parentKey.Substring(0, parentKey.LastIndexOf("_" + keyName));
                        //Console.WriteLine(jProp.Value.Type);
                    }
                }
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            return dict;
        }
    }
}
