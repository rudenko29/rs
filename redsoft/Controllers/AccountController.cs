using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace redsoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApiContext _context;

        public AccountController(ApiContext context)
        {
            _context = context;

            if (_context.Account.Count() == 0)
            {
                var one = new Models.Account { AccountNumber = "Qwe", Balance = 12 };
                var two = new Models.Account { AccountNumber = "Asd", Balance = 15 };

                _context.Account.Add(one);
                _context.Account.Add(two);

                _context.SaveChanges();
            }
        }


        // GET api/account/{account_id}/history
        [HttpGet("{id}/history")]
        public ActionResult<IEnumerable<string>> History(long id)
        {
            var operations = _context.AccountHistory.Include(x => x.Account).Where(x => x.Account.Id == id);

            return new JsonResult(new { Status = "ok", Result = operations });
        }

        // POST api/account/{account_id}/top-up
        [HttpPost("{id}/top-up")]
        public async Task<ActionResult> TopUp(long id, [FromForm] decimal amount)
        {
            if (amount < 0)
            {
                Response.StatusCode = 403;
                return new JsonResult(new { Status = "error", Result = "Невозможно внести сумму меньше или равной нулю" });
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new { Status = "error", Result = "Пользователя не существует" });
            }

            _context.AccountHistory.Add(new Models.AccountHistory { Account = account, Amount = amount, ChangedAt = DateTime.Now });
            account.Balance += amount;
            await _context.SaveChangesAsync();

            return new JsonResult(new { Status = "ok", Result = account.Balance });
        }

        // POST api/account/{account_id}/withdraw
        [HttpPost("{id}/withdraw")]
        public async Task<ActionResult> Withdraw(long id, [FromForm] decimal amount)
        {
            if (amount < 0)
            {
                Response.StatusCode = 403;
                return new JsonResult(new { Status = "error", Result = "Введите корректное значение" });
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new { Status = "error", Result = "Пользователя не существует" });
            }

            if (amount > account.Balance)
            {
                Response.StatusCode = 403;
                return new JsonResult(new { Status = "error", Result = "Недостаточно средств" });
            }

            _context.AccountHistory.Add(new Models.AccountHistory { Account = account, Amount = -amount, ChangedAt = DateTime.Now });
            account.Balance -= amount;
            await _context.SaveChangesAsync();

            return new JsonResult(new { Status = "ok", Result = account.Balance });
        }

        // POST api/account/{source_account_id}/transfer/{destination_account_id}
        [HttpPost("{sourceId}/transfer/{destinationId}")]
        public async Task<ActionResult> Withdraw(long sourceId, long destinationId, [FromForm] decimal amount)
        {
            if (amount < 0)
            {
                Response.StatusCode = 403;
                return new JsonResult(new { Status = "error", Result = "Введите корректное значение" });
            }

            var sourceAccount = await _context.Account.FindAsync(sourceId);
            var destinationIdAccount = await _context.Account.FindAsync(destinationId);

            if (sourceAccount == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new { Status = "error", Result = $"Пользователя с id {sourceId} не существует" });
            }
            if (destinationIdAccount == null)
            {
                Response.StatusCode = 404;
                return new JsonResult(new { Status = "error", Result = $"Пользователя с id {destinationId} не существует" });
            }

            if (amount > sourceAccount.Balance)
            {
                Response.StatusCode = 403;
                return new JsonResult(new { Status = "error", Result = "Недостаточно средств" });
            }

            sourceAccount.Balance -= amount;
            destinationIdAccount.Balance += amount;

            _context.AccountHistory.Add(new Models.AccountHistory { Account = sourceAccount, Amount = -amount, ChangedAt = DateTime.Now });
            _context.AccountHistory.Add(new Models.AccountHistory { Account = destinationIdAccount, Amount = amount, ChangedAt = DateTime.Now });

            await _context.SaveChangesAsync();

            return new JsonResult(new { Status = "ok", Result = new { SourceBalance = sourceAccount.Balance, DestinationBalance = destinationIdAccount.Balance } });
        }

    }
}