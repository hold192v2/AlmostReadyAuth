namespace WebAPI.Extentions
{
    public static class HttpContextExtensions
    {
        public static string GetClientIp(this HttpContext? context)
        {
            if (context == null) return "Unknown";

            // Проверяем заголовок X-Forwarded-For
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',').First().Trim();
            }

            // Если заголовок отсутствует, используем RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
