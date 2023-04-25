using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot_HoursStore.Models;

namespace TelegramBot_HoursStore.Services
{
    public class DBService
    {
        private readonly DBContext _context;
        public DBService(DBContext context)
        {
            _context = context;
        }

        public User FindUser(long id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }

        public User FindUser(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        public List<Shift> GetShifts(int userId)
        {
            return _context.Shifts.Where(s => s.SellerId != userId && s.Active).ToList<Shift>();
        }
        public List<Shift> GetActiveShifts(int UserId)
        {
            return _context.Shifts.Where(s => s.SellerId == UserId && s.Active).ToList<Shift>();
        }
        public List<Shift> GetBought(int userId)
        {
            return _context.Shifts.Where(s => s.BuyerId == userId && s.SoldAt != null).ToList<Shift>();
        }
        public List<Shift> GetSold(int userId)
        {
            return _context.Shifts.Where(s => s.SellerId == userId && s.SoldAt != null).ToList<Shift>();
        }
        public Shift GetShift(Guid guid)
        {

            return _context.Shifts.FirstOrDefault(s => s.Guid == guid);
        }
        public async Task ActivateShift(Guid guid)
        {
            var shift = GetShift(guid);
            shift.Active = true;
            shift.ActivetedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        public async Task DeactivateShift(Guid guid)
        {
            var shift = GetShift(guid);
            var user = FindUser(shift.Seller.UserId);
            user.LastShift = shift.Guid;
            shift.Active = false;
            shift.DeactivatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        public async Task AddShiftAsync(Shift shift)
        {
            User us = FindUser(shift.Seller.UserId);
            shift.Guid = Guid.NewGuid();

            us.LastShift = shift.Guid;

            await _context.Shifts.AddAsync(shift);
            await _context.SaveChangesAsync();
        }
        public async Task EditShiftAsync(Shift shift)
        {
            User us = FindUser(shift.Seller.UserId);
            var sh = _context.Shifts.FirstOrDefault(s => s.Guid == us.LastShift);
            sh.Price = shift.Price;
            sh.TimeSlot = shift.TimeSlot;
            sh.HoursNumber = shift.HoursNumber;
            sh.Active = true;
            sh.ActivetedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        public async Task RemoveShiftAsync(Guid shiftGuid)
        {

            Shift shift = _context.Shifts.FirstOrDefault(s => s.Guid == shiftGuid);
            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();
        }
        public async Task SetMarker(long UserId, string Marker)
        {
            User user = FindUser(UserId);
            user.Marker = Marker;
            await _context.SaveChangesAsync();
        }
        public async Task SetName(long UserId, string Name)
        {
            User user = FindUser(UserId);
            user.Username = Name;
            user.Marker = "Phone";

            await _context.SaveChangesAsync();
        }
        public async Task SetPhone(long UserId, string Phone)
        {
            User user = FindUser(UserId);
            user.Phone = Phone;

            user.Marker = "Card";

            await _context.SaveChangesAsync();
        }
        public async Task SetCard(long UserId, string Card)
        {
            User user = FindUser(UserId);
            user.CardNumber = Card;

            Seller s = _context.Sellers.FirstOrDefault(s => s.Username == user.Username);
            if (s != null)
            {
                user.Marker = "Allowed";
                user.Authorized = true;
            }
            else
            {
                user.Marker = "noAccess";
                user.Authorized = false;
            }

            await _context.SaveChangesAsync();
        }
        public async Task BuyShift(Shift shift, User user)
        {
            User u = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            Shift s = _context.Shifts.FirstOrDefault(s => s.Guid == shift.Guid);
            s.DeactivatedAt = DateTime.Now;
            s.Active = false;
            s.Buyer = u;
            s.SoldAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        public async Task CheckAuthorization(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var seller = _context.Sellers.FirstOrDefault(s => s.Username == user.Username);
            if (seller != null)
            {
                user.Marker = "Allowed";
                user.Authorized = true;
                await _context.SaveChangesAsync();
            }
        }
        public async Task CloseConnection()
        {
            await _context.DisposeAsync();
        }
    }
}
