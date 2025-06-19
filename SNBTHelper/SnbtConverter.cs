using Newtonsoft.Json;
using Snbt.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Snbt.Helper
{
    internal class SnbtConverter
    {
        public static void ConvertAllSnbtToJson(string inputDir, string outputDir)
        {
            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine("输入目录不存在。");
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var snbtFiles = Directory.GetFiles(inputDir, "*.snbt");

            foreach (var snbtFile in snbtFiles)
            {
                try
                {
                    using (var reader = new StreamReader(snbtFile, Encoding.UTF8))
                    {
                        var snbtContent = reader.ReadToEnd();
                        var snbtObject = SnbtFileHelper.Loads(snbtContent, true);
                      
                        var fileName = Path.GetFileNameWithoutExtension(snbtFile);
                        var outputFilePath = Path.Combine(outputDir, $"{fileName}.json");

                        using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
                        {
                            writer.Write(snbtObject);

                            Console.WriteLine($"{snbtObject}");
                            Console.WriteLine($"已写入 {outputFilePath}");
                            
                        }

                        Console.WriteLine($"已将 {snbtFiles} 转换为 {outputFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理文件 {snbtFiles} 时出错: {ex.Message}");
                }
            }
         }

        public static void ConvertAllJsonToSnbt(string inputDir, string outputDir)
        {

            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine("输入目录不存在。");
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach (string filePath in Directory.GetFiles(inputDir, "*.json"))
            {
                try
                {
                    using (var reader = new StreamReader(filePath, Encoding.UTF8))
                    {
                        var jsonContent = reader.ReadToEnd();

                        if (jsonContent == null)
                        {
                            Console.WriteLine("JSON2SNBT 内容为空。");
                            break;
                        }

                        var jsonObject = SnbtFileHelper.Dumps(jsonContent);

                        if (jsonObject == null || jsonObject.Length == 0)
                        {
                            Console.WriteLine("JSON2SNBT 内容为空。");
                            break;
                        }

                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        string outputFilePath = Path.Combine(outputDir, $"{fileName}.snbt");

                        using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
                        {
                            writer.Write(jsonObject);

                            Console.WriteLine($"{jsonObject}");
                            Console.WriteLine($"已写入 {outputFilePath}");

                        }

                        Console.WriteLine($"成功转换 {filePath} 到 {outputFilePath}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理 {filePath} 时出错: {ex.Message}");
                }
            }
        }
    }
}
