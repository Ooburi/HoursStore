using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_HoursStore.Models
{
    class InlineKeyboards
    {
        public InlineKeyboardMarkup GetForBuy(string[] dataVariants, string[] Captions)
        {
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[dataVariants.Length][];
            int Length = dataVariants.Length;

            for (int i = 0; i < Length; i++)
            {
                buttons[i] = new InlineKeyboardButton[] {
                    InlineKeyboardButton.WithCallbackData(Captions[i], dataVariants[i])
                };
            }
            return new InlineKeyboardMarkup(buttons);
        }
        public InlineKeyboardMarkup GetFor(string[] dataVariants, string[] Captions)
        {


            InlineKeyboardButton[][] buttons;
            int Length = dataVariants.Length;
            InlineKeyboardMarkup inlineKeyboardMarkup;

            if (Length % 2 == 0)
            {

                buttons = new InlineKeyboardButton[Length / 2][];

                for (int i = 0; i < Length / 2; i++)
                {
                    buttons[i] = new InlineKeyboardButton[]
                    {
                    InlineKeyboardButton.WithCallbackData(Captions[i*2],dataVariants[i*2]), InlineKeyboardButton.WithCallbackData(Captions[i*2+1],dataVariants[i*2+1])
                    };
                }
                inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);

            }
            else
            {

                Length--;

                buttons = new InlineKeyboardButton[Length / 2 + 1][];


                for (int i = 0; i < Length / 2; i++)
                {
                    buttons[i] = new InlineKeyboardButton[]
                    {
                    InlineKeyboardButton.WithCallbackData(Captions[i*2],dataVariants[i*2]), InlineKeyboardButton.WithCallbackData(Captions[i*2+1],dataVariants[i*2+1])
                    };

                }

                buttons[Length / 2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(Captions[Length], dataVariants[Length]) };

                inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);
            }

            return inlineKeyboardMarkup;
        }
    }
}
