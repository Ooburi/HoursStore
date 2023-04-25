using Microsoft.EntityFrameworkCore;
using System;
using TelegramBot_HoursStore.Services;

namespace TelegramBot_HoursStore
{
    class Program
    {
        private static DBContext _dbContext;
        private static DBService _db;
        static void Main(string[] args)
        {
            try
            {
                _dbContext = new DBContext();
                try
                {
                    _dbContext.Database.Migrate();
                }
                catch
                {

                }

                _db = new DBService(_dbContext);

                Bot.Get(_db);

                Console.WriteLine("Бот запущен, works!, всё нормально");
                Console.ReadLine();
            }
            catch
            {


            }
        }
    }
}
