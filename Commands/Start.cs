using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot_HoursStore.Models;
using TelegramBot_HoursStore.Services;

namespace TelegramBot_HoursStore.Commands
{
    class Start : Command
    {
        DBService _db;
        public override string Name => "/start";
        public Start(DBService db)
        {
            _db = db;
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            Models.User user = _db.FindUser(message.From.Id);
            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = message.From.Id
                };
            }

            string mes = "";
            await _db.CheckAuthorization(user.Id);

            if (!user.Authorized)
            {

                switch (user.Marker)
                {

                    case "Name": mes += "Для работы с ботом нужно пройти авторизацию, введите, пожалуйста, ваше имя"; break;
                    case "Phone": mes += "Введите ваш номер телефона"; break;
                    case "Card": mes += "Введите ваш номер карты"; break;
                    case "noAccess":
                        mes += "У вас нет доступа, свяжитесь с администратором";
                        break;
                }
                await client.SendTextMessageAsync(user.UserId, mes);
            }
            else
            {
                mes += "Выберите действие, которое хотите выполнить:";
                InlineKeyboards inlineKeyboard = new InlineKeyboards();
                string[] captions = new string[] { "Личный кабинет", "Продать часы", "Купить часы" };
                string[] data = new string[] { "account", "sell", "buy" };
                await client.SendTextMessageAsync(user.UserId, mes, replyMarkup: inlineKeyboard.GetFor(data, captions));
            }



        }
    }
}
