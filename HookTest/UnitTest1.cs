using Hook.HookDB;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Reflection;

namespace HookTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {


            //ColNameToPropertyConveter cc = new ColNameToPropertyConveter();
            //cc.Convert();

            //string configFile = Path.Combine(
            //    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            //    "Hook.config");

            //string outputFile = AppDomain.CurrentDomain.BaseDirectory + "Hook.config";

            //Console.WriteLine("�����ַ��" + configFile);
            //Console.WriteLine("�����ַ��" + outputFile);

            //File.Copy(outputFile, configFile, true);

            if (File.Exists("..././Hook.config"))
            {
                Console.WriteLine("�ļ�����");
            }
            else
            {
                Console.WriteLine("�ļ�������");
            }

        }
    }
}