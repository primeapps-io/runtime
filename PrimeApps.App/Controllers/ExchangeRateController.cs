
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Context;

namespace PrimeApps.App.Controllers
{
    [Route("api/exchange_rates"), Authorize]
	public class ExchangeRateController : ApiBaseController
    {
        [Route("get_daily_rates"), HttpGet]
        public async Task<IActionResult> GetDailyRates([FromQuery(Name = "year")]int? year = null, [FromQuery(Name = "month")]int? month = null, [FromQuery(Name = "day")]int? day = null)
        {
            ExchangeRate dailyRates;

            using (var dbContext = new PlatformDBContext())
            {
                if (year.HasValue && month.HasValue && day.HasValue)
                    dailyRates = await dbContext.ExchangeRates.SingleOrDefaultAsync(x => x.Year == year && x.Month == month && x.Day == day);
                else
                    dailyRates = await dbContext.ExchangeRates.OrderByDescending(x => x.Date).Take(1).FirstOrDefaultAsync();
            }

            return Ok(dailyRates);
        }
    }
}
