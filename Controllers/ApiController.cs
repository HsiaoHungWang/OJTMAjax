using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OJTMAjax.Models;
using OJTMAjax.Models.DTO;
using System.Security.Cryptography;

namespace OJTMAjax.Controllers
{
    public class ApiController : Controller
    {
        private readonly ClassDbContext db;
        private readonly IWebHostEnvironment env;
        public ApiController(ClassDbContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }

        public IActionResult Index()
        {
            return Content("Ajax 好!!","text/plain", System.Text.Encoding.UTF8);
        }

        //api/index1?userName=Tom
        public IActionResult Index1(string userName)
        {
            System.Threading.Thread.Sleep(10000); //模擬延遲
            string content = $"Hello {userName}"; //"<h2>Hello World!!</h2>";
            return Content(content, "text/plain", System.Text.Encoding.UTF8);

        }

        //讀取所有城市
        public async Task<IActionResult> Cities()
        {
            var cities = await db.Addresses.Select(a => a.City).Distinct().ToListAsync();
            return Json(cities);
        }

        //根據城市讀取所有鄉鎮區
        [HttpGet]
        public async Task<IActionResult> Sites(string city)
        {
            var sites = await db.Addresses.Where(a => a.City == city).Select(a => a.SiteId).Distinct().ToListAsync();
            return Json(sites);
        }

        //根據鄉鎮區讀取所有路名
        public async Task<IActionResult> Roads(string site)
        {
            var sites = await db.Addresses.Where(a => a.SiteId == site).Select(a => a.Road).Distinct().ToListAsync();
            return Json(sites);
        }

        //回應靜態檔案
        public IActionResult Avatar(string fileName = "cat1.jpg")
        {
            return File($"~/images/{fileName}", "image/jpeg");
        }

        //回應二進位檔案
        public async Task<IActionResult> Avatar1(int id)
        {
            var member = await db.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            byte[]? img = member.FileData;
            if (img == null)
            {
                return NotFound();
            }
            return File(img, "image/jpeg");

        }

        //檢查帳號是否存在
        public async Task<IActionResult> CheckAccount(string name)
        {
            var member = await db.Members.AnyAsync(m => m.Name == name);
            return Content(member.ToString(), "text/plain");
        }

        //傳入 FormData 的資料
        //public IActionResult Register(string name, string email, int age=20)
        //public IActionResult Register(UserDTO _user)
        [HttpPost]
        //public async Task<IActionResult> Register(Member _user, IFormFile Avatar)
        public async Task<IActionResult> Register(Member _user)
        {
            //if (_user.Password == null)
            //{
            //    return BadRequest("Password cannot be null.");
            //}

            ////取得實際路徑
            //string filePath = Path.Combine(env.WebRootPath, "images", Avatar.FileName);
            ////檔案上傳
            //using (var stream = new FileStream(filePath, FileMode.Create))
            //{
            //    Avatar.CopyTo(stream);
            //}

            ////把檔案轉成二進位 
            //using (var ms = new MemoryStream())
            //{
            //    Avatar.CopyTo(ms);
            //    _user.FileData = ms.ToArray();
            //}

            //// 將salt轉成字串寫進資料庫中
            //byte[] salt = GenerateSalt();
            //_user.Salt = Convert.ToBase64String(salt);
            ////將密碼透過salt加密後寫進資料庫中
            //_user.Password = HashPassword(_user.Password, salt);

            //_user.FileName = Avatar.FileName;

            //新增
            await db.Members.AddAsync(_user);
            await db.SaveChangesAsync();

            return Ok(new { Message = "新增成功" });
        }

        public static byte[] GenerateSalt(int size = 16)
        {
            //加密安全隨機數生成器。
            return RandomNumberGenerator.GetBytes(size);
        }

        // 使用 PBKDF2 演算法加密密碼
        private static string HashPassword(string password, byte[] salt)
        {
            // 使用 HMACSHA256，迭代 10,000 次，輸出 256-bit (32 bytes) 雜湊值
            byte[] hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                //Pseudo-Random Function，偽隨機函數，用於計算密碼的雜湊值
                prf: KeyDerivationPrf.HMACSHA256, //使用 HMAC-SHA256 作為哈希演算法
                iterationCount: 10000, // 迭代次數       
                numBytesRequested: 256 / 8);  // 產生密鑰的長度 32 bytes(256-bit) 的雜湊值
          
            return Convert.ToBase64String(hashed);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string email = login.Email ?? string.Empty; // 確保 email 不為 null
            string? pwd = login.Password;

            if (pwd == null)
            {
                return BadRequest(new { Message = "密碼不能為空" });
            }

            
            Member? member = await db.Members.FirstOrDefaultAsync(m => m.Email != null && m.Email.Equals(email));

            if (member == null)
            {
                return NotFound(new { Message = "找不到會員" });
            }

            if (member.Password == null || member.Salt == null)
            {
                return BadRequest(new { Message = "會員資料不完整，無法驗證密碼" });
            }

            bool isPasswordValid = VerifyPassword(pwd, member.Password, member.Salt);
            if (isPasswordValid)
            {
                return Ok(new { Message = "登入成功", Email = email });
            }
            else
            {
                return Unauthorized(new { Message = "密碼驗證失敗" });
            }
        }

        private static bool VerifyPassword(string enteredPassword, string storedHashBase64, string storedSaltBase64)
        {
            // 將資料庫中存的 Base64 字串轉回 byte[]
            byte[] salt = Convert.FromBase64String(storedSaltBase64);
            byte[] storedHash = Convert.FromBase64String(storedHashBase64);

            // 使用相同的參數重新計算輸入密碼的雜湊值
            byte[] enteredHash = KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: storedHash.Length);

            // 比對兩個雜湊值是否相同（使用固定時間比對避免時序攻擊）
            return CryptographicOperations.FixedTimeEquals(enteredHash, storedHash);
        }

        //傳入JSON資料
        [HttpPost]
        public IActionResult Spots([FromBody] SearchDTO searchDTO)
        {
            //根據景點分類讀取景點資料
            var spots = searchDTO.categoryId == 0 ? db.SpotImagesSpots : db.SpotImagesSpots.Where(s => s.CategoryId == searchDTO.categoryId);


            //關鍵字搜尋
            string keyword = "";
            if (!string.IsNullOrEmpty(searchDTO.keyword))
            {
                keyword = searchDTO.keyword;             

                spots = spots.Where(s =>
                    (s.SpotTitle != null && s.SpotTitle.Contains(searchDTO.keyword)) ||
                    (s.SpotDescription != null && s.SpotDescription.Contains(searchDTO.keyword))
                );

                //  spots = spots.Where(s => s.SpotTitle.Contains(searchDTO.keyword) || s.SpotDescription.Contains(searchDTO.keyword));

            }

            //排序
            switch (searchDTO.sortBy)
            {
                case "spotTitle":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                    break;
                case "categoryId":
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                    break;
                default:
                    spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                    break;
            }

            //分頁
            //總共有多少筆資料
            int TotalCount = spots.Count();
            //設定每頁顯示多少筆資料
            int pageSize = searchDTO?.pageSize ?? 9;
            //目前要顯示第幾頁
            int page = searchDTO?.page ?? 1;
            //計算總共有幾頁
            int TotalPages = (int)Math.Ceiling((decimal)TotalCount / pageSize);

            //取出分頁後的資料
            spots = spots.Skip((int)((page - 1) * pageSize)).Take(pageSize);

            //回傳分頁後的資料及總共幾頁
            SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
            spotsPaging.TotalPages = TotalPages;
            spotsPaging.SpotsResult = spots.ToList();

            //return Content(keyword, "text/plain", System.Text.Encoding.UTF8 );
            return Json(spotsPaging);
        }

        public async Task<IActionResult> Keyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                keyword = "";
            }
         
            var titles = await db.Spots.Where(s => s.SpotTitle != null && s.SpotTitle.Contains(keyword))
                                 .OrderBy(s => s.SpotId)
                                 .Select(s => s.SpotTitle)
                                 .Take(8).ToListAsync();

            return Json(titles);

        }
    }
}
