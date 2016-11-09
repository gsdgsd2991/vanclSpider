using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vanclSpider;
using ZaraSpider;

namespace vanclSpiderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var vancl = new vanclSpider.vanclSpider(6286865, @"F:\data\vancl\");
            // vancl.start();
            var zara = new ZaraSpider.ZaraSpider("http://www.zara.cn/cn/", @"F:\data\zara\");
            zara.Statistics();
            //zara.start();
        }
    }
}
