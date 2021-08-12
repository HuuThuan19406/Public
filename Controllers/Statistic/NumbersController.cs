using Api.Entities;
using GoogleApi.Drive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Controllers.Statistic
{
    [Route("api/statistic/[controller]")]
    [ApiController]
    [Authorize(Roles = "administrator,statistician")]
    public class NumbersController : ControllerBase
    {
        private BestsvContext db = new BestsvContext();

        [HttpOptions]
        public IEnumerable<Dictionary<string, int>> Get([Required] string[] queries)
        {
            foreach (var item in queries)
            {
                var query = item.Split("&");

                int? take = null;
                if (query.Length.Equals(2) && (int.TryParse(query[1], out int number)))
                    take = number;

                switch (query[0].ToLower())
                {
                    case "gender":
                        yield return Gender();
                        break;
                    case "from-come-position":
                        yield return FromComePosition(take);
                        break;
                    case "type-business-accounts":
                        yield return TypeBusinessAccounts();
                        break;
                    case "accounts":
                        yield return Accounts();
                        break;
                    case "orders":
                        yield return Orders();
                        break;
                    case "cloud-files":
                        yield return CloudFiles();
                        break;
                }
            }
        }

        private static Dictionary<string, int> CloudFiles()
        {
            var data = new GoogleDriveApi()
                .GetDriveFiles()
                .Select(p => new
                {
                    MimeType = p.MimeType,
                    Size = p.QuotaBytesUsed / 1024
                });

            int image = (int)data.Where(p => p.MimeType.StartsWith("image")).Sum(p => p.Size).Value;
            int video = (int)data.Where(p => p.MimeType.StartsWith("video")).Sum(p => p.Size).Value;
            int audio = (int)data.Where(p => p.MimeType.StartsWith("audio")).Sum(p => p.Size).Value;
            int word = (int)data.Where(p => p.MimeType.EndsWith("vnd.openxmlformats-officedocument.wordprocessingml.document")).Sum(p => p.Size).Value;
            int powerpoint = (int)data.Where(p => p.MimeType.EndsWith("vnd.openxmlformats-officedocument.presentationml.presentation")).Sum(p => p.Size).Value;
            int frontend = (int)data.Where(p => p.MimeType.EndsWith("html") || p.MimeType.EndsWith("css") || p.MimeType.EndsWith("javascript")).Sum(p => p.Size).Value;
            int compressed = (int)data.Where(p => p.MimeType.EndsWith("x-zip-compressed") || p.MimeType.EndsWith("rar")).Sum(p => p.Size).Value;

            return new Dictionary<string, int>
            {
                {"Hình ảnh", image},
                {"Video", video},
                {"Âm thanh", audio},
                {"Word", word},
                {"PowerPoint", powerpoint},
                {"Frontend",  frontend},
                {"Tệp nén", compressed},
                {"Khác",  (int)data.Sum(p => p.Size).Value - image - video - audio - word - powerpoint - frontend - compressed }
            };
        }

        private Dictionary<string, int> Orders()
        {
            var result = new Dictionary<string, int>();
            var processStatus = db.ProcessStatuses.ToList();

            foreach (var item in processStatus)
            {
                result.Add(item.Name, db.Orders.Count(p => p.ProcessStatusId.Equals(item.ProcessStatusId)));
            }
            result.Add("Đã hủy", db.Orders.Count(p => p.IsDeleted));

            return result;
        }

        private Dictionary<string, int> Accounts()
        {
            return new Dictionary<string, int>
            {
                { "active", db.Accounts.Count(p => !p.IsDeleted) },
                { "suspend", db.Accounts.Count(p => p.IsDeleted) }
            };
        }

        private Dictionary<string, int> TypeBusinessAccounts()
        {
            var accountIdOrder = db.Orders.Select(p => p.AccountId).ToList();

            int buyAndSupply = db.Suppliers.Count(p => accountIdOrder.Contains(p.SupplierId));
            int onlyBuyer = db.Accounts.Count(p => (p.Supplier == null) && (!p.AccountRoles.Any(p => p.Role.RoleName.Equals("administrator"))));
            int onlySupplier = db.Suppliers.Count(p => !accountIdOrder.Contains(p.SupplierId));

            return new Dictionary<string, int>
            {
                { "onlyBuyer", onlyBuyer },
                { "buyAndSupply",  buyAndSupply },
                { "onlySupplier", onlySupplier }
            };
        }

        private Dictionary<string, int> FromComePosition(int? take)
        {
            var zipCodes = db.ZipCodes.ToList();

            Dictionary<string, int> positionCountEach = new Dictionary<string, int>();

            foreach (var item in zipCodes)
            {
                positionCountEach.Add(item.Position, db.Accounts.Count(p => p.ZipCodeId.Equals(item.ZipCodeId)));
            }

            Dictionary<string, int> result;

            if (take.HasValue)
                result = positionCountEach.Where(p => p.Value > 0).OrderByDescending(p => p.Value).Take(take.Value).ToDictionary(p => p.Key, p => p.Value);
            else
                result = positionCountEach.Where(p => p.Value > 0).OrderByDescending(p => p.Value).ToDictionary(p => p.Key, p => p.Value);

            return result;
        }

        private Dictionary<string, int> Gender()
        {
            var data = db.Accounts.Select(p => p.Sex);

            int count = data.Count();
            int male = data.Count(p => p.Value.Equals(true));
            int female = data.Count(p => p.Value.Equals(false));

            return new Dictionary<string, int>
            {
                { "male", male },
                { "female" , female },
                { "other" , count - male - female }
            };
        }
    }
}
