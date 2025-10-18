# Email Configuration Guide for TETFund PMS

This guide will help you configure email functionality for the TETFund Performance Management System.

## Quick Setup Options

### Option 1: Gmail (Recommended for Testing)

1. **Create a Gmail Account** (if you don't have one)
2. **Enable 2-Factor Authentication** on your Gmail account
3. **Generate an App Password**:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate a password for "Mail"
   - Copy the 16-character password

4. **Update `appsettings.Development.json`**:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-gmail@gmail.com",
       "SenderName": "TETFund PMS - Development",
       "Username": "your-gmail@gmail.com",
       "Password": "your-16-character-app-password",
       "UseSsl": true,
       "RequireAuthentication": true
     }
   }
   ```

### Option 2: Outlook/Hotmail

1. **Update `appsettings.Development.json`**:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp-mail.outlook.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@outlook.com",
       "SenderName": "TETFund PMS - Development",
       "Username": "your-email@outlook.com",
       "Password": "your-outlook-password",
       "UseSsl": true,
       "RequireAuthentication": true
     }
   }
   ```

### Option 3: Custom SMTP Server

1. **Update `appsettings.Development.json`**:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "your-smtp-server.com",
       "SmtpPort": 587,
       "SenderEmail": "noreply@tetfund.gov.ng",
       "SenderName": "TETFund PMS",
       "Username": "your-smtp-username",
       "Password": "your-smtp-password",
       "UseSsl": true,
       "RequireAuthentication": true
     }
   }
   ```

## Testing Email Configuration

### Step 1: Build and Run the Application
```bash
dotnet build
dotnet run --project PMS.API
```

### Step 2: Test Email via API
Use the test email endpoint:

**POST** `https://localhost:7000/api/v1/emails/test`

**Headers:**
```
Authorization: Bearer your-jwt-token
Content-Type: application/json
```

**Body:**
```json
{
  "to": "test@example.com"
}
```

### Step 3: Check Email Logs
**GET** `https://localhost:7000/api/v1/emails/logs`

This will show you all email attempts and their status.

## Common Issues and Solutions

### Issue 1: "Authentication failed"
- **Gmail**: Make sure you're using an App Password, not your regular password
- **Other providers**: Check username/password are correct

### Issue 2: "Connection timeout"
- Check if your firewall blocks SMTP ports (587, 465)
- Try different SMTP ports (465 for SSL, 587 for TLS)

### Issue 3: "SSL/TLS error"
- Try setting `"UseSsl": false` for some providers
- Check if your provider requires specific SSL settings

### Issue 4: "Email not received"
- Check spam/junk folder
- Verify the recipient email address is correct
- Check email logs for error messages

## Debugging Steps

1. **Check Console Output**: Look for debug messages when creating officers
2. **Check Email Logs**: Use the `/api/v1/emails/logs` endpoint
3. **Test with Simple Email**: Use the test endpoint first
4. **Verify SMTP Settings**: Double-check all configuration values

## Production Configuration

For production, use environment variables or secure configuration:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.tetfund.gov.ng",
    "SmtpPort": 587,
    "SenderEmail": "noreply@tetfund.gov.ng",
    "SenderName": "TETFund Performance Management System",
    "Username": "noreply@tetfund.gov.ng",
    "Password": "secure-production-password",
    "UseSsl": true,
    "RequireAuthentication": true
  }
}
```

## Security Notes

- Never commit real email passwords to version control
- Use environment variables for production
- Consider using Azure Key Vault or similar for production secrets
- Use App Passwords for Gmail instead of main passwords
