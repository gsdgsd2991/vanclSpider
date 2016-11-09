using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;


namespace vanclSpider
{
    public class vanclSpider
    {
        UInt64 startNum;
        string picFolder;

        public vanclSpider(UInt64 startNum, string picFolder)
        {
            this.startNum = startNum;
            this.picFolder = picFolder;
        }
        //凡客url格式：http://item.vancl.com/6375247.html
        public void start()
        {
            var URLPrefix = "http://item.vancl.com/";
            var URLEnd = ".html";

            using (var outStream = System.IO.File.Create(picFolder + "ID2ID.csv"))
            {
                using (var writer = new StreamWriter(outStream))
                {
                    using (var titleStream = System.IO.File.Create(picFolder + "Title2ID.csv"))
                    {
                        using (var titleWriter = new StreamWriter(titleStream))
                        {
                            writer.AutoFlush = true;
                            titleWriter.AutoFlush = true;
                            for (var i = startNum; i < startNum + 500000; i++)
                            {
                                Console.WriteLine(i);
                                var webRq = WebRequest.Create(URLPrefix + i + URLEnd);
                                webRq.Timeout = 1000;
                                try
                                {
                                    using (var response = (HttpWebResponse)webRq.GetResponse())
                                    {
                                        using (var reader = new StreamReader(response.GetResponseStream()))
                                        {

                                            var htmlString = reader.ReadToEnd();
                                            //商品标题
                                            var titleRegex = new Regex("<title>.*</title>");
                                            var title = titleRegex.Match(htmlString).Groups[0].Value.Split(new char[] { '<', '>' })[2];
                                            titleWriter.WriteLine(i + "," + title);
                                            //商品图片获取
                                            var midImgRegex = new Regex("img id=\"midimg\" src=\"\\S*\"");
                                            var midImg = midImgRegex.Match(htmlString).Groups;
                                            var midImgURL = midImg[0].Value.Split('"')[3];
                                            SaveFile(midImgURL, i);

                                            //搭配URLs
                                            var rectRegex = new Regex("area shape=\"rect\" coords=\"\\S*\" href=\"\\S*\"");
                                            var recURLs = rectRegex.Matches(htmlString);
                                            foreach (Match recURL in recURLs)
                                            {
                                                var ans = recURL.Value.Split('"')[5].Split('/')[3].Split('.')[0];

                                                writer.WriteLine(i + "," + ans);
                                            }
                                        }

                                    }
                                }
                                catch (Exception)
                                { }
                            }
                        }
                    }
                }
            }
        }

        private void SaveFile(string url, UInt64 num)
        {
            var webRq = WebRequest.Create(url);
            var response = (HttpWebResponse)webRq.GetResponse();
            var fileName = num + ".jpg";
            var buffer = new byte[10240];
            using (var outStream = System.IO.File.Create(picFolder + fileName))
            {
                using (var inStream = response.GetResponseStream())
                {
                    int l;
                    do
                    {
                        l = inStream.Read(buffer, 0, buffer.Length);
                        if (l > 0)
                            outStream.Write(buffer, 0, l);
                    }
                    while (l > 0);
                }
            }
        }
    }
}
