using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configPath = "config.json";
            var configText = System.IO.File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<BotConfig>(configText);
            var Client = new TelegramBotClient(config.TelegramBotToken);
            Client.StartReceiving(Update, Erorr);
            Console.ReadLine();
        }

        private static Task Erorr(ITelegramBotClient botClient, Exception Error, CancellationToken arg3)
        {
            return null;
        }

        async static Task Update(ITelegramBotClient botClient, Update Update, CancellationToken arg3)
        {
            var message = Update.Message;
            if(message.Text != null)
            {
                if(message.Text == "/hello")
                {
                    botClient.SendTextMessageAsync(message.Chat.Id, "Артем Леонов \nartemleon34@gmail.com\n28.10.2023");
                }
                if (message.Text == "/start")
                {
                    botClient.SendTextMessageAsync(message.Chat.Id, "Привет! Я Telegram-бот для поиска информации о компаниях по ИНН.\r\nЕсли ты хочешь узнать, какие команды я поддерживаю, напиши /help. Я готов помочь!\r\n");
                }
                if (message.Text == "/help")
                    botClient.SendTextMessageAsync(message.Chat.Id, "Привет! Я бот, и я могу выполнить следующие команды:\r\n\r\n1. /start - Начать общение с ботом.\r\n2. /help - Вывести справку о доступных командах.\r\n3. /hello - Вывести ваше имя и фамилию, ваш email, и дату получения задания.\r\n4. /inn <ИНН1> <ИНН2> ... - Получить наименования и адреса компаний по указанным ИНН. Вы можете указать несколько ИНН в одном обращении к боту, разделяя их пробелами.\r\n\r\nПример использования команды /inn:\r\n/inn 1234567890 9876543210\r\n\r\nПожалуйста, выберите одну из этих команд, чтобы продолжить.");
                if (message.Text.Contains("/inn"))
                {
                    try
                    {
                        string inn = message.Text.Substring(5);
                        string[] innarray = inn.Split(' ');
                        for (int i = 0; i < innarray.Length; i++)
                        {
                            var companyInfo = await GetCompanyInfoByINN(innarray[i]);
                            if (companyInfo != null)
                            {
                                botClient.SendTextMessageAsync(message.Chat.Id, $"Наименование компании: {companyInfo.Name}\nАдрес компании: {companyInfo.Address}");
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat.Id, "Компания с указанным ИНН не найдена.");
                            }
                        }
                    }
                    catch
                    {
                        botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка команды");
                    }
                }
            }
        }
        static async Task<CompanyInfo> GetCompanyInfoByINN(string inn)
        {
            using (var httpClient = new HttpClient())
            {
                string apiUrl = $"https://companies.rbc.ru/search/?query={inn}";
                var response = await httpClient.GetStringAsync(apiUrl);

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // Найдем наименование и адрес компании на странице
                try
                {
                    var nameNode = doc.DocumentNode.SelectSingleNode("//a[@class='company-name-highlight']");
                    var addressNode = doc.DocumentNode.SelectNodes("//p[@class='company-card__info']");
                    if (nameNode != null && addressNode != null)
                    {
                        string name = nameNode.InnerText.Trim();
                        string address = addressNode[1].InnerText.Trim().Replace("Юридический адрес:", "");
                        return new CompanyInfo { Name = name, Address = address };
                    }
                }
                catch
                {

                }
                return null;
            }
        }
    }

    class CompanyInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
    public class BotConfig
    {
        public string TelegramBotToken { get; set; }
    }
}

