namespace UserService.Services.FileStorageService
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            var homePath = Environment.GetEnvironmentVariable("HOME")
                           ?? _env.WebRootPath
                           ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsFolder = Path.Combine(homePath, "site", "wwwroot", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folderName}/{uniqueFileName}";
        }

        public void DeleteFile(string filePath)
        {
            var homePath = Environment.GetEnvironmentVariable("HOME")
                           ?? _env.WebRootPath
                           ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var fullPath = Path.Combine(homePath, "site", "wwwroot", filePath.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
