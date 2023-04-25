using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot_HoursStore.Commands;
using TelegramBot_HoursStore.Models;

namespace TelegramBot_HoursStore.Services
{
    public class UpdateManager
    {
        static IReadOnlyList<Command> Commands = null;
        static DBService _db;

        public static async Task Handle(ITelegramBotClient arg1, Update update, CancellationToken cancellationToken, IReadOnlyList<Command> commands, TelegramBotClient client, DBService db)
        {

            _db = db;
            Commands = commands;

            switch (update.Type)
            {

                case UpdateType.Message:
                    try
                    {
                        await HandleMessage(update, client);
                    }
                    catch (Exception e)
                    {

                        await HandleErrorAsync(client, e, cancellationToken);
                    }
                    break;
                case UpdateType.CallbackQuery:
                    try
                    {
                        await HandleCallback(update, client);
                    }
                    catch (Exception e)
                    {
                        await HandleErrorAsync(client, e, cancellationToken);
                    }
                    break;
            }
        }

        private static async Task HandleCallback(Update update, TelegramBotClient client)
        {
            string data = update.CallbackQuery.Data;
            Guid guid = Guid.Empty;

            try
            {
                string[] dataArr = data.Split("#");
                if (dataArr.Length > 1)
                {
                    guid = Guid.Parse(dataArr[1]);
                    data = dataArr[0];
                }
            }
            catch
            {

            }
            Models.User user = _db.FindUser(update.CallbackQuery.From.Id);

            switch (data)
            {
                case "account":
                    string[] caps = new string[3] { "Купленные часы", "Проданные часы", "Активные лоты" };
                    string[] dats = new string[3] { "bought", "sold", "active" };
                    InlineKeyboards ik = new InlineKeyboards();
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Выберите, что вы хотите посмотреть:\n", replyMarkup: ik.GetFor(dats, caps));
                    break;
                case "bought":
                    List<Shift> bougth = _db.GetBought(user.Id);
                    if (bougth == null || bougth.Count == 0)
                    {
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Вы ещё не купили ни одного лота");
                        break;
                    }
                    else
                    {
                        string bougthList = "Список купленных лотов:\n";
                        foreach (Shift b in bougth)
                        {
                            Models.User sel = _db.FindUser((int)b.SellerId);
                            bougthList += $"Продавец:{sel.Username} таймслот:{b.TimeSlot}, всего {b.HoursNumber}часов\n";
                        }
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, bougthList);
                    }
                    break;
                case "sold":
                    List<Shift> sold = _db.GetSold(user.Id);
                    if (sold == null || sold.Count == 0)
                    {
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Вы ещё не продали ни одного лота");
                        break;
                    }
                    else
                    {
                        string soldList = "Список проданных лотов:\n";
                        foreach (Shift b2 in sold)
                        {
                            Models.User sel = _db.FindUser((int)b2.BuyerId);
                            soldList += $"Покупатель:{sel.Username} таймслот:{b2.TimeSlot}, всего {b2.HoursNumber}часов\n";
                        }
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, soldList);
                    }
                    break;
                case "active":
                    List<Shift> shiftsActive = _db.GetActiveShifts(user.Id);
                    if (shiftsActive == null || shiftsActive.Count == 0)
                    {
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "У вас нет выставленных на продажу лотов\n");
                        break;
                    }
                    else
                    {
                        string[] caps2 = new string[shiftsActive.Count + 1];
                        string[] dats2 = new string[shiftsActive.Count + 1];

                        for (int i = 0; i < shiftsActive.Count; i++)
                        {
                            caps2[i] = $"Смена:{shiftsActive[i].TimeSlot},{shiftsActive[i].HoursNumber} часов, оплата {shiftsActive[i].Price} в час";
                            dats2[i] = "edit#" + shiftsActive[i].Guid.ToString();
                        }
                        caps2[shiftsActive.Count] = "Назад";
                        dats2[shiftsActive.Count] = "account";
                        InlineKeyboards ikb = new InlineKeyboards();
                        await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Нажмите на лот, если хотите его отредактировать", replyMarkup: ikb.GetForBuy(dats2, caps2));
                    }
                    break;
                case "edit":
                    await _db.DeactivateShift(guid);
                    await _db.SetMarker(user.UserId, "Edit");
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Введите дату/время смены, кол-во часов и оплату в час через пробел\n" +
                        "Например: 14:00 8 2500р.\n");
                    break;
                case "buy":
                    List<Shift> shifts = _db.GetShifts(user.Id);
                    if (shifts == null || shifts.Count == 0)
                    {
                        await client.SendTextMessageAsync(user.UserId, "В продаже нет активных лотов");
                        break;
                    }
                    string[] captions = new string[shifts.Count];
                    string[] datas = new string[shifts.Count];

                    for (int i = 0; i < shifts.Count; i++)
                    {
                        captions[i] = $"{shifts[i].TimeSlot}:{shifts[i].HoursNumber} часов, оплата {shifts[i].Price} в час";
                        datas[i] = "shift#" + shifts[i].Guid.ToString();
                    }
                    InlineKeyboards inlineKeyboard = new InlineKeyboards();
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Нажмите на лот, чтобы получить детальную информацию", replyMarkup: inlineKeyboard.GetForBuy(datas, captions));
                    break;
                case "sell":
                    //
                    await _db.SetMarker(user.UserId, "Sell");
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Введите время смены, кол-во часов и оплату в час через пробел\n" +
                        "Например: 14:00 8 2500р.\n" +
                        "Введите \"Отмена\", чтобы отменить");
                    break;
                case "change":
                    try
                    {
                        await _db.RemoveShiftAsync(user.LastShift);
                    }
                    catch
                    {

                    }

                    await _db.SetMarker(user.UserId, "Sell");
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Введите время смены, кол-во часов и оплату в час через пробел\n" +
                        "Например: 14:00 8 2500р.\n" +
                        "Введите \"Отмена\", чтобы отменить");
                    break;
                case "confirm":
                    await _db.SetMarker(user.UserId, "Allowed");
                    await _db.ActivateShift(user.LastShift);
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Смена выставлена на продажу, чтобы вызвать меню, нажмите /start");
                    break;
                case "shift":
                    Shift s = _db.GetShift(guid);
                    string[] cap = new string[] { "Назад", "Купить" };
                    string[] datas1 = new string[] { $"back#{guid.ToString()}", $"confirmbuy#{guid.ToString()}" };
                    InlineKeyboards inlineKeyboard1 = new InlineKeyboards();

                    string message = "";
                    try
                    {
                        Models.User seller = _db.FindUser((int)s.SellerId);
                        message = $"Продавец: {seller.Username}" +
                           $"\nВыставлено на продажу: {((DateTime)s.ActivetedAt).ToString("d")}\n" +
                           $"Время: {s.TimeSlot}, Количество часов:{s.HoursNumber}\nОплата в час:{s.Price}\n";
                        await client.SendTextMessageAsync(user.UserId, message, replyMarkup: inlineKeyboard1.GetForBuy(datas1, cap));
                    }
                    catch
                    {
                        message = $"Продавец: unnamed" +
                           $"\nВыставлено на продажу: {((DateTime)s.ActivetedAt).ToString("d")}\n" +
                           $"Время: {s.TimeSlot}, Количество часов:{s.HoursNumber}\nОплата в час:{s.Price}\n";
                        await client.SendTextMessageAsync(user.UserId, message, replyMarkup: inlineKeyboard1.GetForBuy(datas1, cap));
                    }
                    break;
                case "back":
                    List<Shift> shifts2 = _db.GetShifts(user.Id);

                    string[] captions2 = new string[shifts2.Count];
                    string[] datas2 = new string[shifts2.Count];

                    for (int i = 0; i < shifts2.Count; i++)
                    {
                        captions2[i] = $"{shifts2[i].TimeSlot},{shifts2[i].HoursNumber} часов, оплата {shifts2[i].Price} в час";
                        datas2[i] = "shift#" + shifts2[i].Guid.ToString();
                    }
                    InlineKeyboards inlineKeyboard2 = new InlineKeyboards();
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Нажмите на лот, чтобы получить детальную информацию", replyMarkup: inlineKeyboard2.GetForBuy(datas2, captions2));
                    break;
                case "confirmbuy":
                    Shift s2 = _db.GetShift(guid);
                    Models.User owner = _db.FindUser(s2.Seller.UserId);
                    string username = user.Username;
                    if (!string.IsNullOrEmpty(update.CallbackQuery.From.Username)) username = update.CallbackQuery.From.Username;
                    await client.SendTextMessageAsync(owner.UserId, $"Ваш лот: {s2.TimeSlot}, {s2.HoursNumber} часов, с оплатой: {s2.Price} в час\n" +
                        $"Приобрел пользователь:@{username} ");
                    await _db.BuyShift(s2, user);
                    await client.SendTextMessageAsync(update.CallbackQuery.From.Id, "Покупка совершена успешно, чтобы вызвать меню введите /start");
                    break;
            }
        }

        private static async Task HandleMessage(Update update, TelegramBotClient client)
        {
            Message message = update.Message;
            switch (message.Type)
            {
                // если текст (команды)
                case MessageType.Text:
                    await ManageText(update, client);
                    break;
            }
        }
        private async static Task ManageText(Update update, TelegramBotClient client)
        {
            Message message = update.Message;

            var commands = Commands;

            Models.User user = _db.FindUser(message.From.Id);
            if (user == null)
            {
                user = new()
                { UserId = message.From.Id, Marker = "Name" };
                await _db.AddUserAsync(user);
            }

            foreach (var command in commands)
            {
                if (command.Contains(message.Text))
                {
                    try
                    {
                        await command.Execute(message, client);
                        return;
                    }
                    catch
                    {

                    }
                }
            }
            switch (user.Marker)
            {
                case "Name": // text supposed to be Name
                    if (Regex.IsMatch(message.Text, @"^[а-яa-z]+\s*[а-яa-z]+\s*[а-яa-z]+$", RegexOptions.IgnoreCase))
                    {
                        await _db.SetName(message.From.Id, message.Text);
                        user = _db.FindUser(message.From.Id);
                        string mes = $"Имя установлено\nВаше имя:{message.Text}\nТеперь введите ваш номер телефона";
                        await client.SendTextMessageAsync(message.From.Id, mes); /// af fawd
                    }
                    else
                        await client.SendTextMessageAsync(message.From.Id, "Ошибка. Возможно вы ввели знак препинания или цифру, если вводите инициалы, не ставьте точки, пожалуйста, попробуйте ещё раз.");
                    break;
                case "Phone":
                    if (Regex.IsMatch(message.Text, @"^\d{3,12}$", RegexOptions.IgnoreCase))
                    {
                        await _db.SetPhone(message.From.Id, message.Text);
                        user = _db.FindUser(message.From.Id);
                        string mes = $"Телефон записан\nВаш телефон:{message.Text}\n Теперь введите номер карты, вводите только цифры, без пробелов";

                        await client.SendTextMessageAsync(message.From.Id, mes);
                    }
                    else
                        await client.SendTextMessageAsync(message.From.Id, "Ошибка. Введите только цифры без пробелов, пожалуйста, попробуйте ещё раз.");
                    break;
                case "Card":
                    if (Regex.IsMatch(message.Text, @"^\d{16,16}$", RegexOptions.IgnoreCase))
                    {
                        await _db.SetCard(message.From.Id, message.Text);
                        user = _db.FindUser(message.From.Id);
                        string mes = $"Номер карты записан\nВаш номер карты:{message.Text}\n Теперь введите команду /start";

                        await client.SendTextMessageAsync(message.From.Id, mes);
                    }
                    else
                        await client.SendTextMessageAsync(message.From.Id, "Ошибка. Введите только цифры без пробелов, пожалуйста, попробуйте ещё раз.");
                    break;
                case "Edit":
                    Shift sh = new Shift() { };

                    string[] mess2 = message.Text.Split(" ");
                    try
                    {
                        sh.TimeSlot = mess2[0];
                        sh.HoursNumber = Convert.ToInt32(mess2[1]);
                        sh.Price = Convert.ToDouble(mess2[2]);
                        sh.Seller = user;
                        sh.Active = false;
                        await _db.EditShiftAsync(sh);
                        await _db.SetMarker(user.UserId, "Allowed");
                        await client.SendTextMessageAsync(user.UserId, "Изменения применены, нажмите /start чтобы вызвать меню");

                    }
                    catch
                    {
                        await client.SendTextMessageAsync(message.From.Id, "Неверный ввод, попробуйте ещё раз.");
                    }

                    break;
                case "Sell":
                    if (message.Text.ToLower() == "отмена")
                    {
                        await _db.SetMarker(user.UserId, "Allowed");
                        await client.SendTextMessageAsync(message.From.Id, "Отменено, введите /start для вызова меню");
                        break;
                    }

                    Shift shift = new Shift() { };

                    string[] mess = message.Text.Split(" ");
                    try
                    {
                        shift.TimeSlot = mess[0];
                        shift.HoursNumber = Convert.ToInt32(mess[1]);
                        shift.Price = Convert.ToDouble(mess[2]);
                        shift.Seller = user;
                        shift.Active = false;
                        await _db.AddShiftAsync(shift);

                        InlineKeyboards inlineKeyboard = new InlineKeyboards();
                        string[] captions = new string[] { "Изменить", "Подтвердить" };
                        string[] data = new string[] { "change", "confirm" };
                        await client.SendTextMessageAsync(user.UserId, $"Вы создали смену на продажу с следующими данными:" +
                            $"\nВремя смены: {shift.TimeSlot}\n" +
                            $"Кол-во часов: {shift.HoursNumber} \n" +
                            $"Оплата в час: {shift.Price}\n", replyMarkup: inlineKeyboard.GetFor(data, captions));

                    }
                    catch
                    {
                        await client.SendTextMessageAsync(message.From.Id, "Неверный ввод, попробуйте ещё раз.");
                    }
                    break;
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient arg1, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.ToString());
            return Task.CompletedTask;
        }
    }
}
