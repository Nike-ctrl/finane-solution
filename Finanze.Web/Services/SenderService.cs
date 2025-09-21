using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using CSharpFunctionalExtensions;

namespace Finanze.Web.Services
{
    public class SenderService
    {
        public TelegramBotClient Bot { get; set; }
        public long userId { get; set; }
        public SenderService(IConfiguration configuration)
        {
            userId = long.Parse(configuration["id"]);
            Bot = new TelegramBotClient(configuration["token-telegram"]);
        }


        public async Task<Result> SendMessage(string message)
        {
            try
            {
                //var messagebot = await Bot.SendMessage(
                //    userId,
                //    $"```\n{message}\n```",
                //    ParseMode.MarkdownV2,
                //    protectContent: false);


                var messagebot = await Bot.SendMessage(
    userId,
    $"`{message}`",
    ParseMode.MarkdownV2,
    protectContent: false);

                return Result.Success("Messaggio Inviato");
            }
            catch (Exception)
            {
                return Result.Failure("Errore nell inviare il messaggio");

            }




        }


    }
}
