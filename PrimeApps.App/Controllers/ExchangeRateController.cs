using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Context;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("api/exchange_rates"), Authorize]
    public class ExchangeRateController : ApiBaseController
    {
        private PlatformDBContext _dBContext;
        private IConfiguration _configuration;

        public ExchangeRateController( IConfiguration configuration,PlatformDBContext dBContext)
        {
            _dBContext = dBContext;
            _configuration = configuration;
        }
      
        [Route("get_daily_rates"), HttpGet]
        public async Task<IActionResult> GetDailyRates([FromQuery(Name = "year")]int? year = null, [FromQuery(Name = "month")]int? month = null, [FromQuery(Name = "day")]int? day = null)
        {
            ExchangeRate dailyRates;

            //using (var dbContext = new PlatformDBContext(_configuration))
            //{
                if (year.HasValue && month.HasValue && day.HasValue)
                    dailyRates = await _dBContext.ExchangeRates.SingleOrDefaultAsync(x => x.Year == year && x.Month == month && x.Day == day);
                else
                    dailyRates = await _dBContext.ExchangeRates.OrderByDescending(x => x.Date).Take(1).FirstOrDefaultAsync();
          //  }

            return Ok(dailyRates);
        }
    }
}
