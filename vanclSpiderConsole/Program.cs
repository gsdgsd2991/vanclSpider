using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vanclSpider;

namespace vanclSpiderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var vancl = new vanclSpider.vanclSpider(6286865, @"c:\data\vancl\");
            vancl.start();
        }
    }
}
