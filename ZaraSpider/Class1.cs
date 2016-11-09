using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace ZaraSpider
{
    public class ZaraSpider
    {
        string picFolder;
        string startPage;
        public ZaraSpider(string startPage, string picFolder)
        {
            this.picFolder = picFolder;
            this.startPage = startPage;
        }

        public void Statistics()
        {
            int women = 0, men = 0, child = 0;
            using (var linkStream = File.Open(picFolder + "ID2Link.csv", FileMode.Open))
            {
                using (var linkReader = new StreamReader(linkStream))
                {
                    for (; !linkReader.EndOfStream;)
                    {
                        var temp = linkReader.ReadLine();
                        if (temp.Contains("女士"))
                            women++;
                        if (temp.Contains("男士"))
                            men++;
                        if (temp.Contains("儿童"))
                            child++;
                    }
                }
            }
            Console.WriteLine("女士:" + women + " 男士：" + men + " 儿童：" + child);
        }

        public void start()
        {
            var webRq = WebRequest.Create(startPage);
            try
            {
                using (var relaStream = File.Create(picFolder + "ID2ID.csv"))
                {
                    using (var relaWriter = new StreamWriter(relaStream))
                    {
                        relaWriter.AutoFlush = true;
                        using (var linkStream = File.Create(picFolder + "ID2Link.csv"))
                        {
                            using (var linkWriter = new StreamWriter(linkStream))
                            {
                                linkWriter.AutoFlush = true;
                                using (var response = (HttpWebResponse)webRq.GetResponse())
                                {
                                    using (var reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        var htmlString = reader.ReadToEnd();
                                        var classLinkRegex = new Regex("<li class=\"_category-link     \" data-categoryId=\"\\d*\" tabindex=\"\\d*\"><a href=\"\\S*\">");
                                        //包含大类和小类
                                        var classLinks = classLinkRegex.Matches(htmlString);
                                        foreach (Match classLinkMatch in classLinks)
                                        {
                                            var classLink = classLinkMatch.Value.Split('\"')[7];
                                            var classRq = WebRequest.Create(classLink);
                                            try
                                            {
                                                using (var classResponse = (HttpWebResponse)classRq.GetResponse())
                                                {
                                                    using (var classReader = new StreamReader(classResponse.GetResponseStream()))
                                                    {
                                                        var listHtmlString = classReader.ReadToEnd();
                                                        var listRegex = new Regex("<a class=\"item _item\" href=\"\\S*\" target=\"_blank\">");
                                                        //商品列表
                                                        var itemList = listRegex.Matches(listHtmlString);
                                                        foreach (Match itemMatch in itemList)
                                                        {
                                                            var itemLink = itemMatch.Value.Split('\"')[3];
                                                            var itemRq = WebRequest.Create("http:" + itemLink);
                                                            Console.WriteLine(itemLink);
                                                            try
                                                            {
                                                                using (var itemResponse = (HttpWebResponse)itemRq.GetResponse())
                                                                {
                                                                    using (var itemReader = new StreamReader(itemResponse.GetResponseStream()))
                                                                    {
                                                                        var itemHtmlString = itemReader.ReadToEnd();

                                                                        var itemPicUrls = Regex.Matches(itemHtmlString, "a class=\"_seoImg\" href=\"\\S*\"");
                                                                        var itemClassNum = Regex.Match(itemLink, "c\\d+p\\d+");
                                                                        linkWriter.WriteLine(itemClassNum + "," +HttpUtility.UrlDecode(itemLink,UTF8Encoding.UTF8));
                                                                        for (var m = 0; m < itemPicUrls.Count; m++)
                                                                        {
                                                                            //商品图片
                                                                            var itemPicUrl = itemPicUrls[m].Value.Split(new char[] { '?', '"' })[3];
                                                                            //商品编号C+数字+p+数字

                                                                            saveFile("http:" + itemPicUrl, itemClassNum.Value + '-' + m);
                                                                        }
                                                                        //相关商品
                                                                        //连接格式a class="_product-popup-link" href="//www.zara.cn/cn/zh/-c733898p3676034.html"
                                                                        var itemRelatedUrlMatches = Regex.Matches(itemHtmlString, "a class=\"_product-popup-link\" href=\"\\S*\"");
                                                                        foreach (Match relatedItem in itemRelatedUrlMatches)
                                                                        {
                                                                            var relatedClassNum = Regex.Match(relatedItem.Value, "c\\d+p\\d+");
                                                                            relaWriter.WriteLine(itemClassNum + "," + relatedClassNum);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception) { }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void saveFile(string url, string num)
        {
            var webRq = WebRequest.Create(url);
            using (var response = (HttpWebResponse)webRq.GetResponse())
            {
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
}
