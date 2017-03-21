using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Web;
using System.Windows.Forms;
using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;

namespace crawldemo
{
    class Program
    {
        static List<string> links=new List<string>();
        static List<string> pic_url = new List<string>();
        static long n = 0;
        static void Main(string[] args)
        {
            download_url("http://taylorpictures.net/thumbnails.php?album=");
            Console.ReadKey();
        }
        static void download_url(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393";
            request.Timeout = 30000;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream s = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(s,Encoding.UTF8))
                            {
                                string html = sr.ReadToEnd();
                                string encode = HttpUtility.HtmlDecode(html);
                                download_pic(encode);
                                Lexer lexer = new Lexer(encode);
                                Parser par = new Parser(lexer);
                                NodeFilter nodefilter = new TagNameFilter("a");
                                NodeList nodes = par.ExtractAllNodesThatMatch(nodefilter);
                                for (int i = 0; i < nodes.Count; i++)
                                {
                                    ITag tag = nodes[i] as ITag;
                                    bool isexist = false;
                                    foreach (string ss in links)
                                    {
                                        if (ss == tag.GetAttribute("href"))
                                        {
                                            isexist = true;
                                            break;
                                        }
                                    }
                                    if (!isexist)
                                    {
                                        links.Add(tag.GetAttribute("href"));
                                        Console.WriteLine("accessing " + "http://taylorpictures.net/" + tag.GetAttribute("href"));
                                        using (FileStream fs = new FileStream(@"e:/Photos/crawl_log.txt", FileMode.Append))
                                        {
                                            byte[] bytes = Encoding.UTF8.GetBytes("accessing " + "http://taylorpictures.net/" + tag.GetAttribute("href")+"\r\n");
                                            fs.Write(bytes, 0, bytes.Length);
                                        }
                                        download_url("http://taylorpictures.net/" + tag.GetAttribute("href"));
                                    }
                                    else
                                        continue;
                                }
                            }
                        }
                    }
                    else
                        Console.WriteLine("Error");
                }
            }
            catch
            {
                Console.WriteLine("404");
            }
        }
        static void download_pic(string html)
        {
            if (html != "" && html != null)
            {
                try
                {
                    MatchCollection matchs = Regex.Matches(html, @"(\w{1,}\/){1,}\w{1,}.(jpg)", RegexOptions.IgnoreCase);
                    for (int i = 0; i < matchs.Count; i++)
                    {
                        //Console.WriteLine(matchs[i].Value);
                        bool isexist = false;
                        foreach (string ss in pic_url)
                        {
                            if (ss == matchs[i].Value)
                            {
                                isexist = true;
                                break;
                            }
                        }
                        if (!isexist)
                        {
                            pic_url.Add(matchs[i].Value);
                            string imgurl = "http://taylorpictures.net/" + matchs[i].Value;
                            imgurl = imgurl.Replace("normal_", "");
                            HttpWebRequest getpicrequest = (HttpWebRequest)WebRequest.Create(imgurl);
                            HttpWebResponse getpicresponse = (HttpWebResponse)getpicrequest.GetResponse();
                            using (Stream s = getpicresponse.GetResponseStream())
                            {
                                try
                                {
                                    using (Image image = Image.FromStream(s))
                                    {
                                        if (image.Width > 1000 && image.Height > 1000)
                                        {
                                            Console.WriteLine("getting picture from: " + imgurl + " size: " + image.Size);
                                            image.Save(@"e:/Photos/" + ++n + ".jpg");
                                        }
                                    }
                                }
                                catch
                                {
                                    continue;
                                }

                            }
                        }
                        else
                            continue;
                    }
                }
                catch
                { }
            }
            else
            {
                Console.WriteLine("Null");
            }
        }
    }
}
