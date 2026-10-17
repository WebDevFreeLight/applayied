using Infrastructure.Helper;
using JinianNet.JNTemplate;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure
{
    public static class LogoExtension
    {
        public static void AddLogo(this IServiceCollection services)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            var contentTpl = JnHelper.ReadTemplate("", "logo.txt");
            var content = contentTpl?.Render();
            var url = AppSettings.GetConfig("urls");
            Console.WriteLine(content);
            Console.ForegroundColor = ConsoleColor.Blue;







            //Console.WriteLine("??Source Code: https://gitee.com/izory/maham-aiNetCore");
            Console.WriteLine("??Official 2 Documentation: http://www.izhaorui.cn");
            Console.WriteLine("??Sponsor the Author: http://www.izhaorui.cn/vip");
            Console.WriteLine("??Mobile Experience: http://demo.izhaorui.cn/h5");
            Console.WriteLine($"Swagger URL: {url}/swagger/index.html");
            Console.WriteLine($"Initialize Seed Data URL: {url}/common/InitSeedData");



        }
    }
}

