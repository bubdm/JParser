/*
 * 
 *  Project     : JParser
 *  File        : Program.cs
 *  Developer   : Rahil Parikh ( rahil@rahilparikh.me )
 *  Date        : August 18, 2012
 *  
 *  Copyright (c) 2012, Rahil Parikh
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
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonEx = @"{""MessageType"":0,
                               ""Message"":""Success"",
                               ""Value"":[
                                          {""listId"":1,
                                           ""listName"":""DemoList"",
                                           ""itemInList"":[
                                                {
                                                 ""fromDate"":""\/Date(1228946400000)\/"",
                                                 ""fromLocation"":null,
                                                 ""toLocation"":null,
                                                 ""originalRequest"":""water"",
                                                 ""creationDate"":""\/Date(1339448400000)\/"",
                                                 ""typeId"":1
                                                },
                                                {
                                                 ""fromDate"":null,
                                                 ""fromLocation"":null,
                                                 ""toLocation"":null,
                                                 ""originalRequest"":""gala"",
                                                 ""creationDate"":""\/Date(1304370000000)\/"",
                                                 ""typeId"":1
                                                }
                                          ]}
                                ]}";
            JParser parser = new JParser(jsonEx);

            foreach (KeyValuePair<string, string> kv in parser.ToDictionary())
            {
                Console.WriteLine("{0}\t\t\t{1}", kv.Key, kv.Value);
            }
            Console.Read();
        }
    }
}
