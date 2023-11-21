using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SMCRecycle.Data;
using SMCRecycle.WebApi.Services;
using System.ComponentModel.DataAnnotations;

namespace SMCRecycle.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserDetailController : ControllerBase
    {
        private readonly JomSibuDatabaseContext _database;
        private readonly UserManager<IdentityUser> _userManager;
        private IFileStorageService _fileStorageService;

        public UserDetailController(JomSibuDatabaseContext database, UserManager<IdentityUser> userManager, IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            _database = database;
            _userManager = userManager;
        }
        
        [HttpGet("GetAllUserDetail")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.Admin}")]
        public async Task<List<UserDetailWithEmailAndPhone>> GetAllUserDetail()
        {
            return await _database.UserDetails
                .AsNoTracking()
                .Where(x => x.IsDeleted != true)
                .Include(x => x.City)
                .Include(x => x.RecycleRequestsUserDetail).ThenInclude(x => x.RecycleRequestDetails)
                .Select(x => new UserDetailWithEmailAndPhone()
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    UserRoleId = x.UserRoleId,
                    DateJoined = x.DateJoined!.Value.AddHours(8),
                    Address = x.Address,
                    Postcode = x.Postcode,
                    CityId = x.CityId,
                    City = x.City,
                    ProfileImagePath = x.ProfileImagePath,
                    PhoneNumber = _database.Users.First(y => y.Id == x.AspNetUserId).PhoneNumber ?? string.Empty,
                    Email = _database.Users.First(y => y.Id == x.AspNetUserId).Email ?? string.Empty,
                    UserPoint = x.RecycleRequestsVendorUserDetail.Where(x => x.RecycleRequestStatusId == 3).Sum(x => x.Weight) ?? 0,
                    DateVerified = x.DateVerified,
                    VerifiedByUserDetailId = x.VerifiedByUserDetailId
                })
                .ToListAsync();
        }

        [HttpGet("GetSystemDetails")]
        public async Task<SystemDetailsModel> GetSystemDetails()
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var user = await _database.GetUserByAspNetUserIdAsync(aspNetUser.Id);
            var isVerified = false;
            if (user.DateVerified != null)
            {
                isVerified = true;
            }
            var hasIdentity = false;
            if (user.HasIdentityImages == 1)
            {
                hasIdentity = true;
            }

            var systemDetails = new SystemDetailsModel
            {
                Version = "1.0.17",
                IsVerified = isVerified,
                HasIdentity = hasIdentity,
                SystemUpdateMessage = "Please update your app to the latest version.",
            };

            return systemDetails;
        }

        [HttpGet]
        public async Task<UserDetailWithEmailAndPhone> GetUserDetail(int id = 0)
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var user = await _database.GetUserByAspNetUserIdAsync(aspNetUser.Id);
            if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.SystemAdmin))
            {
                if (id == 0)
                {
                    id = user.Id;
                }
                return await _database.UserDetails
                    .AsNoTracking()
                    .Include(x => x.City)
                    .Include(x => x.RecycleRequestsUserDetail).ThenInclude(x => x.RecycleRequestDetails)
                    .Include(x => x.RewardRedeemRecords).ThenInclude(x => x.RewardDetail)
                    .Select(x => new UserDetailWithEmailAndPhone()
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        DateJoined = x.DateJoined!.Value.AddHours(8),
                        Address = x.Address,
                        Postcode = x.Postcode,
                        CityId = x.CityId,
                        City = x.City,
                        ProfileImagePath = x.ProfileImagePath,
                        PhoneNumber = _database.Users.First(y => y.Id == x.AspNetUserId).PhoneNumber ?? string.Empty,
                        Email = _database.Users.First(y => y.Id == x.AspNetUserId).Email ?? string.Empty,
                        UserPoint = x.RecycleRequestsVendorUserDetail.Where(x => x.RecycleRequestStatusId == 3).Sum(x => x.Weight) ?? 0,
                        DateVerified = x.DateVerified,
                        VerifiedByUserDetailId = x.VerifiedByUserDetailId,
                        //UserPoint = (x.RecycleRequestsUserDetail.Sum(y => y.RecycleRequestDetails.Sum(z => z.Weight))) / 1000 - (x.RewardRedeemRecords.Sum(y => y.RewardDetail.Point)) ?? 0,
                    })
                    .FirstAsync(x => x.Id == id);
            } 
            else if (user.UserRoleId == (int)UserRoleEnum.Vendor || user.UserRoleId == (int)UserRoleEnum.Promoter)
            {
                return await _database.UserDetails
                        .AsNoTracking()
                        .Include(x => x.City)
                        .Include(x => x.RecycleRequestsVendorUserDetail)
                        .Select(x => new UserDetailWithEmailAndPhone()
                        {
                            Id = x.Id,
                            FullName = x.FullName,
                            DateJoined = x.DateJoined!.Value.AddHours(8),
                            Address = x.Address,
                            Postcode = x.Postcode,
                            CityId = x.CityId,
                            City = x.City,
                            ProfileImagePath = x.ProfileImagePath,
                            PhoneNumber = aspNetUser.PhoneNumber,
                            Email = aspNetUser.Email,
                            UserPoint = x.RecycleRequestsVendorUserDetail.Where(x => x.RecycleRequestStatusId == 3).Sum(x => x.Weight) ?? 0,
                            DateVerified = x.DateVerified,
                            VerifiedByUserDetailId = x.VerifiedByUserDetailId
                        })
                        .FirstAsync(x => x.Id == user.Id);
            }
            else 
            {
                return await _database.UserDetails
                    .AsNoTracking()
                    .Include(x => x.City)
                    .Include(x=>x.RecycleRequestsUserDetail)
                    .Select(x => new UserDetailWithEmailAndPhone()
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        DateJoined = x.DateJoined!.Value.AddHours(8),
                        Address = x.Address,
                        Postcode = x.Postcode,
                        CityId = x.CityId,
                        City = x.City,
                        ProfileImagePath = x.ProfileImagePath,
                        PhoneNumber = aspNetUser.PhoneNumber,
                        Email = aspNetUser.Email,
                        UserPoint = x.RecycleRequestsUserDetail.Sum(y => y.Weight) ?? 0,
                        DateVerified = x.DateVerified,
                        VerifiedByUserDetailId = x.VerifiedByUserDetailId
                    })
                    .FirstAsync(x => x.Id == user.Id);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserDetail()
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var userDetailInDb = await _database.UserDetails
                .FirstAsync(x => x.AspNetUserId == aspNetUser.Id);
            var httpRequest = HttpContext.Request;

            if (!httpRequest.HasFormContentType)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new CustomResponse { StatusCode = CustomStatusCodes.InvalidMediaType, Message = "Invalid media type" });
            }

            if (httpRequest.Form.ContainsKey("model"))
            {
                httpRequest.Form.TryGetValue("model", out StringValues model);
                var record = JsonConvert.DeserializeObject<EditProfileModels>(model);
                var context = new ValidationContext(record);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(record, context, results, true);

                if (!isValid)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
                    {
                        StatusCode = CustomStatusCodes.InvalidData,
                        Message = string.Join("\n", results
                            .Select(x => x.ErrorMessage))
                    });
                }

                aspNetUser.PhoneNumber = record.PhoneNumber;
                userDetailInDb.FullName = record.FullName;
                userDetailInDb.Address = record.Address;
                userDetailInDb.ContactNumber = record.PhoneNumber;
                //userDetailInDb.Postcode = record.Postcode;
                //userDetailInDb.CityId = record.CityId;

                if (httpRequest.Form.Files.Count > 0)
                {
                    foreach (var file in httpRequest.Form.Files)
                    {
                        if (file.Name == "files")
                        {
                            var newPath = await _fileStorageService.CreateFileAsync("UserProfileImage/Image", file.FileName, file.OpenReadStream(), file.ContentType);
                            if (!string.IsNullOrWhiteSpace(userDetailInDb.ProfileImagePath))
                            {
                                await _fileStorageService.DeleteFileIfExistsAsync(userDetailInDb.ProfileImagePath);
                            }
                            userDetailInDb.ProfileImagePath = newPath;
                            break;
                        }
                    }
                }

                await _database.SaveChangesAsync();
                return Ok(await GetUserDetail());
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.Admin}")]
        public async Task<IActionResult> DeleteUserDetail(long id)
        {
            var userDetail = await _database.UserDetails.FindAsync(id);
            if (userDetail == null)
            {
                return NotFound();
            }

            userDetail.IsDeleted = true;
            _database.UserDetails.Update(userDetail);
            await _database.SaveChangesAsync();

            return NoContent();
        }
    }
}
