using Snbt.Library;
using System.Text;

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
    }
}
