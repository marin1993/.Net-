// See https://aka.ms/new-console-template for more information
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

namespace MyApp
{
    public class StudyInfo {
        public StudyInfo(string userName, int learnNum, List<string> learnName)
        {
            this.userName = userName;
            this.learnNum = learnNum;
            this.learnName = learnName;
        }
        public string userName { get;private set; }
        public int learnNum { get; private set; }
        public List<string> learnName{ get; private set; }
        public void addClass(string className) {
            this.learnNum++;
            this.learnName.Add(className);
        }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            // API 里有课程名，直接写出了为了好认
            var apiInfo = new[]{
                (
                    title:"C# 学习",
                    url:"https://docs.microsoft.com/api/challenges/17c618cc-3c82-4a29-b2c6-d78b1de10b98/leaderboard?%24top=100&%24skip=0"
                ),
                (
                    title:"ASP.NET Core 开发",
                    url:"https://docs.microsoft.com/api/challenges/b64cc891-e999-4652-909b-d545698a2e60/leaderboard?%24top=100&%24skip=0"
                ),
                (
                    title:".NET 移动应用",
                    url:"https://docs.microsoft.com/api/challenges/38ec3c07-3ce6-4fb8-b423-b79166202364/leaderboard?%24top=100&%24skip=0"
                )
            };

            var client = new HttpClient();

            // 不要将中文转义了
            var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

            var user = new List<StudyInfo>();
            foreach (var item in apiInfo)
            {
                string jsoninfo = await client.GetStringAsync(item.url);
                JsonNode jsonNode = JsonNode.Parse(jsoninfo)!;
                // 合集总课程数
                int classnum = (int)jsonNode["totalScoreUnits"]!;
                foreach (var uinfo in jsonNode["results"]!.AsArray())
                {
                    // 学完的用户
                    if ((float)uinfo["score"]! == classnum)
                    {
                        // 看看有没有这个用户的信息
                        var temp = user.FirstOrDefault(e => e.userName == (string)uinfo["userDisplayName"]!);
                        if (temp is null)
                        {
                            // 初始创建这个用户
                            user.Add(new StudyInfo((string)uinfo["userDisplayName"]!, 1, new List<string>() { item.title }));
                        }
                        else
                        {
                            // 有，更新学习数据
                            temp.addClass(item.title);
                        }
                    }
                }
            }

            // 逆序排序后转为json字符存入文件
            var jsonRes = JsonSerializer.Serialize(user.OrderByDescending(x => x.learnNum), options);
            File.WriteAllText("output.json", jsonRes);

            // 简单分析一下
            Console.WriteLine($"完成《C# 学习》{user.Where(x=>x.learnName.IndexOf("C# 学习")>-1).Count()}人");
            Console.WriteLine($"完成《ASP.NET Core 开发》{user.Where(x => x.learnName.IndexOf("ASP.NET Core 开发") > -1).Count()}人");
            Console.WriteLine($"完成《.NET 移动应用》{user.Where(x => x.learnName.IndexOf(".NET 移动应用") > -1).Count()}人");
            Console.WriteLine($"仅完成 1 次挑战 {user.Where(x => x.learnNum == 1).Count()}人");
            Console.WriteLine($"仅完成 2 次挑战 {user.Where(x => x.learnNum ==2).Count()}人");
            Console.WriteLine($"共完成 3 次挑战 {user.Where(x => x.learnNum == 3).Count()}人\n 分别是");

            foreach (var item in user.Where(x => x.learnNum == 3)) {
                Console.WriteLine(item.userName);
            }

        }
    }
}
