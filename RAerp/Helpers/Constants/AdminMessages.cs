namespace RAerp.Helpers.Constants
{
    public static class AdminMessages
    {
        public const string SuperAdminUsername = "raerpSAdmin";
        public const string SuperAdminRole = "Super Admin";
        public const string SuperAdminName = "RAerp Admin";
        public const string AdminRole = "Admin";
        public const string SelectListNoneValue = "None";
        public const string GuestRole = "Guest";

        public const string HiddenPasswordDisplay = "SECRETPASSWORD";

        public const string JWTSecretKeyPlainText = "welcome2RAerp@123";

        public const string EmailVerificationHtmlContent = $"<!DOCTYPE html>" +
                    $"<html><head><meta charset=\"UTF-8\">" +
                    $"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
                    $"<title>Verification Code</title></head>" +
                    $"<body style=\"margin:0;padding:0;font-family:'Helvetica Neue',Helvetica,Arial,sans-serif;background-color:#f4f7f6;color:#333333;\">" +
                    $"<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:600px;margin:20px auto;background-color:#ffffff;border-radius:8px;box-shadow:0 4px 10px rgba(0,0,0,0.05);overflow:hidden;border:1px solid #e1e5e8;\">" +
                    $"<tr><td style=\"padding:40px 30px;text-align:center;background-color:#1a73e8;\">" +
                    $"<h1 style=\"color:#ffffff;margin:0;font-size:24px;font-weight:600;\">Secure Verification</h1></td></tr>" +
                    $"<tr><td style=\"padding:30px 30px 20px 30px;\"><p style=\"margin:0 0 16px 0;font-size:16px;line-height:1.5;\">Hello, <b>{{0}} {{1}}</b></p>" +
                    $"<p style=\"margin:0 0 24px 0;font-size:16px;line-height:1.5;\">Thank you for choosing our platform. Use the following One-Time Password (OTP) to complete your verification process. " +
                    $"<br/>This code is valid for: <b>{{2}}</b>.</p></td></tr>" +
                    $"<tr><td style=\"padding:0 30px 30px 30px;text-align:center;\"><div style=\"display:inline-block;background-color:#f1f3f4;border:2px dashed #1a73e8;letter-spacing:6px;font-weight:700;font-size:32px;color:#1a73e8;padding:12px 30px;border-radius:6px;\">{{3}}</div></td></tr>" +
                    $"<tr><td style=\"padding:0 30px 30px 30px;\"><p style=\"margin:0;font-size:14px;color:#666666;line-height:1.5;\">If you did not request this code, please ignore this email or contact support if you have security concerns.</p></td></tr>" +
                    $"<tr><td style=\"padding:20px 30px;background-color:#fafafa;text-align:center;border-top:1px solid #eeeeee;\"><p style=\"margin:0;font-size:12px;color:#999999;\">&copy; 2026 RAerp. All rights reserved.</p></td></tr></table></body></html>";

    }
}
