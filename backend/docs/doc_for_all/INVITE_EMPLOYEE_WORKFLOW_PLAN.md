# Invite Employee Workflow - Implementation Plan

## Overview

This document outlines the complete implementation plan for the **Invite Employee** and **Forgot Password** workflows for the HR + IAM system. The system allows HR administrators to invite employees by email, generating a secure token-based activation process.

---

## 1. Involved Models & Database Tables

### 1.1 Existing Models (Already in System)

#### `SysUser` (sys_user table)
```
Location: ZR.Model\System\Model\SysUser.cs
```

**Existing Fields Used:**
| Field | Type | Description |
|-------|------|-------------|
| `UserId` | long | Primary key (Snowflake ID) |
| `UserName` | string | Login username |
| `NickName` | string | Display name |
| `Email` | string | User email |
| `Password` | string | Hashed password |
| `EmailVerificationToken` | string | Token for email verification |
| `EmailVerificationExpiry` | DateTime? | Token expiration time |
| `EmailVerified` | int | 0=unverified, 1=verified |
| `Status` | int | 0=active, 1=disabled |

#### `HrEmployeeCore` (hr_employee_core table)
```
Location: ZR.Model\HR\EmployeeCore\HrEmployeeCore.cs
```

**Key Fields:**
| Field | Type | Description |
|-------|------|-------------|
| `EmployeeId` | long | Primary key |
| `PersonId` | long | FK to hr_person |
| `UserId` | long? | **FK to sys_user** (links employee to user account) |
| `EmployeeCode` | string | Employee code/badge number |
| `IsActive` | bool | Active status |

#### `HrPersonContact` (hr_person_contact table)
```
Location: ZR.Model\HR\EmployeeCore\HrPersonContact.cs
```

**Key Fields:**
| Field | Type | Description |
|-------|------|-------------|
| `PersonContactId` | long | Primary key |
| `PersonId` | long | FK to hr_person |
| `ContactTypeDictCode` | long | Type (email, phone, etc.) |
| `ContactValue` | string | Email address / phone number |
| `IsPrimary` | bool | Primary contact flag |
| `IsVerified` | bool | Verification status |

### 1.2 New Fields Required

#### Add to `SysUser`:
```csharp
// Password reset token (separate from email verification)
[SugarColumn(ColumnDescription = "Password reset token")]
public string PasswordResetToken { get; set; }

// Password reset token expiry time
[SugarColumn(ColumnDescription = "Password reset token expiry time")]
public DateTime? PasswordResetExpiry { get; set; }

// Invitation token (for employee invitation flow)
[SugarColumn(ColumnDescription = "Invitation token")]
public string InvitationToken { get; set; }

// Invitation token expiry time
[SugarColumn(ColumnDescription = "Invitation token expiry time")]
public DateTime? InvitationExpiry { get; set; }

// Account activation status (for invited accounts)
[SugarColumn(DefaultValue = "0", ColumnDescription = "Account activated: 0=pending, 1=activated")]
public int IsActivated { get; set; } = 0;
```

---

## 2. Workflow Diagrams

### 2.1 Invite Employee Flow

```
+-------------------+     +--------------------+     +-------------------+
|  HR Admin         |     |  Backend           |     |  Employee Email   |
+-------------------+     +--------------------+     +-------------------+
        |                         |                         |
        | 1. Select employee      |                         |
        |    Enter email          |                         |
        |------------------------>|                         |
        |                         |                         |
        |                         | 2. Create sys_user      |
        |                         |    - Status=1 (disabled)|
        |                         |    - IsActivated=0      |
        |                         |    - Generate token     |
        |                         |    - Set expiry (48h)   |
        |                         |                         |
        |                         | 3. Link employee        |
        |                         |    - Set user_id        |
        |                         |                         |
        |                         | 4. Send invitation      |
        |                         |------------------------>|
        |                         |                         |
        |<------------------------|                         |
        | 5. Success response     |                         |
        |                         |                         |

+-------------------+     +--------------------+     +-------------------+
|  Employee Browser |     |  Backend           |     |  Database         |
+-------------------+     +--------------------+     +-------------------+
        |                         |                         |
        | 6. Click email link     |                         |
        |------------------------>|                         |
        |                         |                         |
        |                         | 7. Validate token       |
        |                         |------------------------>|
        |                         |<------------------------|
        |                         |                         |
        |<------------------------|                         |
        | 8. Show activation page |                         |
        |    (set password form)  |                         |
        |                         |                         |
        | 9. Submit new password  |                         |
        |------------------------>|                         |
        |                         |                         |
        |                         | 10. Update user:        |
        |                         |     - Hash password     |
        |                         |     - Status=0 (active) |
        |                         |     - IsActivated=1     |
        |                         |     - Clear token       |
        |                         |------------------------>|
        |                         |<------------------------|
        |                         |                         |
        |<------------------------|                         |
        | 11. Redirect to login   |                         |
```

### 2.2 Forgot Password Flow

```
+-------------------+     +--------------------+     +-------------------+
|  User Browser     |     |  Backend           |     |  User Email       |
+-------------------+     +--------------------+     +-------------------+
        |                         |                         |
        | 1. Enter email/username |                         |
        |------------------------>|                         |
        |                         |                         |
        |                         | 2. Find user            |
        |                         | 3. Generate reset token |
        |                         | 4. Set expiry (1h)      |
        |                         |                         |
        |                         | 5. Send reset email     |
        |                         |------------------------>|
        |                         |                         |
        |<------------------------|                         |
        | 6. "Check email" msg    |                         |
        |                         |                         |
        | 7. Click email link     |                         |
        |------------------------>|                         |
        |                         |                         |
        |                         | 8. Validate token       |
        |<------------------------|                         |
        | 9. Show reset form      |                         |
        |                         |                         |
        | 10. Submit new password |                         |
        |------------------------>|                         |
        |                         |                         |
        |                         | 11. Hash & save pass    |
        |                         |     Clear reset token   |
        |                         |                         |
        |<------------------------|                         |
        | 12. Success, redirect   |                         |
```

---

## 3. API Endpoints Design

### 3.1 Admin Endpoints (Require Authentication)

#### `POST /api/hr/employees/invite`
**Purpose:** Invite an employee to create a system account

**Request Body:**
```json
{
  "employeeId": 123,
  "email": "employee@company.com",
  "roleIds": [102, 103],
  "sendEmail": true
}
```

**Response:**
```json
{
  "code": 200,
  "msg": "Invitation sent successfully",
  "data": {
    "userId": 456789,
    "email": "employee@company.com",
    "expiresAt": "2025-01-12T10:00:00Z"
  }
}
```

#### `POST /api/hr/employees/resend-invitation`
**Purpose:** Resend invitation email with new token

**Request Body:**
```json
{
  "employeeId": 123
}
```

### 3.2 Public Endpoints (No Authentication)

#### `GET /api/auth/validate-invitation?token={token}`
**Purpose:** Validate invitation token and return employee info

**Response (Success):**
```json
{
  "code": 200,
  "data": {
    "valid": true,
    "employeeName": "Ahmed Mohammed",
    "email": "ahmed@company.com",
    "employeeCode": "EMP001"
  }
}
```

**Response (Invalid/Expired):**
```json
{
  "code": 400,
  "msg": "Invalid or expired invitation token"
}
```

#### `POST /api/auth/activate-account`
**Purpose:** Complete account activation with password

**Request Body:**
```json
{
  "token": "abc123...",
  "password": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

**Response:**
```json
{
  "code": 200,
  "msg": "Account activated successfully",
  "data": {
    "redirectUrl": "/auth/sign-in"
  }
}
```

#### `POST /api/auth/forgot-password`
**Purpose:** Request password reset email

**Request Body:**
```json
{
  "email": "user@company.com"
}
```

**Response:**
```json
{
  "code": 200,
  "msg": "If an account exists, a reset email has been sent"
}
```

#### `GET /api/auth/validate-reset-token?token={token}`
**Purpose:** Validate password reset token

**Response:**
```json
{
  "code": 200,
  "data": {
    "valid": true,
    "email": "user@company.com"
  }
}
```

#### `POST /api/auth/reset-password`
**Purpose:** Reset password with valid token

**Request Body:**
```json
{
  "token": "xyz789...",
  "password": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

---

## 4. Backend Implementation Tasks

### 4.1 Database Migration
```sql
-- Add new columns to sys_user table
ALTER TABLE sys_user ADD COLUMN password_reset_token VARCHAR(255);
ALTER TABLE sys_user ADD COLUMN password_reset_expiry TIMESTAMP;
ALTER TABLE sys_user ADD COLUMN invitation_token VARCHAR(255);
ALTER TABLE sys_user ADD COLUMN invitation_expiry TIMESTAMP;
ALTER TABLE sys_user ADD COLUMN is_activated INT DEFAULT 0;
```

### 4.2 Service Layer

**Create:** `ZR.Service\System\EmployeeInvitationService.cs`
```csharp
public interface IEmployeeInvitationService
{
    Task<InvitationResult> InviteEmployeeAsync(InviteEmployeeDto dto);
    Task<bool> ResendInvitationAsync(long employeeId);
    Task<InvitationValidationResult> ValidateInvitationTokenAsync(string token);
    Task<ActivationResult> ActivateAccountAsync(ActivateAccountDto dto);
}
```

**Create:** `ZR.Service\System\PasswordResetService.cs`
```csharp
public interface IPasswordResetService
{
    Task<bool> RequestPasswordResetAsync(string email);
    Task<ResetTokenValidationResult> ValidateResetTokenAsync(string token);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
}
```

### 4.3 Controller Layer

**Create:** `ZR.Admin.WebApi\Controllers\System\AuthController.cs`
```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    // Public endpoints for activation and password reset
    [AllowAnonymous]
    [HttpGet("validate-invitation")]
    public Task<IActionResult> ValidateInvitation(string token);

    [AllowAnonymous]
    [HttpPost("activate-account")]
    public Task<IActionResult> ActivateAccount([FromBody] ActivateAccountDto dto);

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto);

    [AllowAnonymous]
    [HttpGet("validate-reset-token")]
    public Task<IActionResult> ValidateResetToken(string token);

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto);
}
```

**Extend:** `EmployeesController.cs`
```csharp
[HttpPost("invite")]
public Task<IActionResult> InviteEmployee([FromBody] InviteEmployeeDto dto);

[HttpPost("resend-invitation")]
public Task<IActionResult> ResendInvitation([FromBody] ResendInvitationDto dto);
```

### 4.4 Email Templates

**Create:** `ZR.Admin.WebApi\Templates\InvitationEmail.html`
```html
<h2>Welcome to Maham System</h2>
<p>Dear {{employeeName}},</p>
<p>You have been invited to create your account. Click the link below to set up your password:</p>
<p><a href="{{activationUrl}}">Activate Your Account</a></p>
<p>This link will expire in 48 hours.</p>
```

**Create:** `ZR.Admin.WebApi\Templates\PasswordResetEmail.html`
```html
<h2>Password Reset Request</h2>
<p>Dear {{userName}},</p>
<p>We received a request to reset your password. Click the link below:</p>
<p><a href="{{resetUrl}}">Reset Password</a></p>
<p>This link will expire in 1 hour.</p>
<p>If you didn't request this, please ignore this email.</p>
```

---

## 5. Frontend Implementation Tasks

### 5.1 New Pages (Next.js App Router)

#### Page 1: Account Activation
```
Location: apps/isomorphic-intl/src/app/[locale]/auth/activate/page.tsx
URL: /auth/activate?token=xxx
```

**Components:**
- Token validation on mount
- Employee info display (name, email)
- Password input with confirmation
- Password strength indicator
- Success/error states
- Redirect to login on success

#### Page 2: Forgot Password
```
Location: apps/isomorphic-intl/src/app/[locale]/auth/forgot-password/page.tsx
URL: /auth/forgot-password
```

**Components:**
- Email input form
- Success message (check your email)
- Link back to login

#### Page 3: Reset Password
```
Location: apps/isomorphic-intl/src/app/[locale]/auth/reset-password/page.tsx
URL: /auth/reset-password?token=xxx
```

**Components:**
- Token validation on mount
- New password form
- Password confirmation
- Success redirect to login

### 5.2 HR Admin UI Updates

#### Employee List Enhancement
```
Location: apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/employees/page.tsx
```

**Add:**
- "Invite" button for employees without linked user account
- "Resend Invitation" button for pending invitations
- Status indicator showing: No Account | Invitation Pending | Active Account

#### Invite Employee Modal
```
Location: apps/isomorphic-intl/src/app/shared/hr/invite-employee-modal.tsx
```

**Fields:**
- Employee name (readonly, from selected employee)
- Email (editable, pre-filled from hr_person_contact)
- Role selection (multi-select)
- Send email toggle

### 5.3 Login Page Enhancement

#### Add Forgot Password Link
```
Location: apps/isomorphic-intl/src/app/[locale]/auth/sign-in-1/page.tsx
```

**Add:**
- "Forgot Password?" link below password field
- Link to /auth/forgot-password

---

## 6. Security Considerations

### 6.1 Token Generation
- Use cryptographically secure random token (256-bit)
- Example: `Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))`

### 6.2 Token Storage
- Store hashed token in database (optional but recommended)
- Never log or expose tokens in API responses

### 6.3 Rate Limiting
- Limit forgot-password requests: 3 per email per hour
- Limit activation attempts: 5 per token

### 6.4 Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### 6.5 Token Expiration
- Invitation token: 48 hours
- Password reset token: 1 hour

---

## 7. Email Configuration

### Current Configuration (appsettings.json)
```json
"MailOptions": [
  {
    "FromName": "system",
    "FromEmail": "",      // NEEDS TO BE CONFIGURED
    "Password": "",       // NEEDS TO BE CONFIGURED
    "Smtp": "smtp.qq.com",
    "Port": 587,
    "UseSsl": true
  }
]
```

### Required Setup
1. Configure SMTP credentials in appsettings.json
2. Set sender email address
3. For production: Use environment variables for sensitive data

---

## 8. Implementation Order

### Phase 1: Backend Foundation
1. Add new columns to sys_user table (migration)
2. Update SysUser model with new properties
3. Create DTOs for invitation and password reset
4. Create EmployeeInvitationService
5. Create PasswordResetService
6. Create AuthController with public endpoints
7. Add invite endpoint to EmployeesController

### Phase 2: Email System
1. Configure SMTP settings
2. Create email templates (HTML)
3. Implement email sending in services
4. Test email delivery

### Phase 3: Frontend Pages
1. Create activation page (/auth/activate)
2. Create forgot password page (/auth/forgot-password)
3. Create reset password page (/auth/reset-password)
4. Add translations (ar.json, en.json)

### Phase 4: HR Admin Integration
1. Add invite button to employee list
2. Create invite employee modal
3. Add invitation status indicators
4. Add resend invitation functionality

### Phase 5: Testing & Polish
1. End-to-end testing of invite flow
2. End-to-end testing of forgot password flow
3. Error handling and edge cases
4. Loading states and UX polish

---

## 9. Translations Required

### Arabic (ar.json)
```json
{
  "auth": {
    "activate": {
      "title": "تفعيل الحساب",
      "subtitle": "مرحباً {{name}}، يرجى إنشاء كلمة المرور الخاصة بك",
      "password": "كلمة المرور",
      "confirmPassword": "تأكيد كلمة المرور",
      "submit": "تفعيل الحساب",
      "success": "تم تفعيل حسابك بنجاح",
      "expired": "انتهت صلاحية رابط التفعيل",
      "invalid": "رابط التفعيل غير صالح"
    },
    "forgotPassword": {
      "title": "نسيت كلمة المرور",
      "subtitle": "أدخل بريدك الإلكتروني لإعادة تعيين كلمة المرور",
      "email": "البريد الإلكتروني",
      "submit": "إرسال رابط إعادة التعيين",
      "success": "تم إرسال رابط إعادة التعيين إلى بريدك الإلكتروني",
      "backToLogin": "العودة لتسجيل الدخول"
    },
    "resetPassword": {
      "title": "إعادة تعيين كلمة المرور",
      "newPassword": "كلمة المرور الجديدة",
      "confirmPassword": "تأكيد كلمة المرور الجديدة",
      "submit": "إعادة تعيين كلمة المرور",
      "success": "تم إعادة تعيين كلمة المرور بنجاح"
    }
  },
  "hr": {
    "employees": {
      "invite": "دعوة للنظام",
      "resendInvitation": "إعادة إرسال الدعوة",
      "invitationSent": "تم إرسال الدعوة",
      "accountStatus": {
        "noAccount": "لا يوجد حساب",
        "pending": "دعوة معلقة",
        "active": "حساب مفعل"
      }
    }
  }
}
```

---

## 10. File Structure Summary

### Backend (New Files)
```
backend/
├── ZR.Model/System/Model/
│   └── SysUser.cs (UPDATE - add new fields)
├── ZR.Model/System/Dto/
│   ├── InviteEmployeeDto.cs (NEW)
│   ├── ActivateAccountDto.cs (NEW)
│   ├── ForgotPasswordDto.cs (NEW)
│   └── ResetPasswordDto.cs (NEW)
├── ZR.Service/System/
│   ├── IEmployeeInvitationService.cs (NEW)
│   ├── EmployeeInvitationService.cs (NEW)
│   ├── IPasswordResetService.cs (NEW)
│   └── PasswordResetService.cs (NEW)
├── ZR.Admin.WebApi/Controllers/
│   ├── System/AuthController.cs (NEW)
│   └── HR/EmployeesController.cs (UPDATE)
└── ZR.Admin.WebApi/Templates/
    ├── InvitationEmail.html (NEW)
    └── PasswordResetEmail.html (NEW)
```

### Frontend (New Files)
```
apps/isomorphic-intl/src/
├── app/[locale]/auth/
│   ├── activate/page.tsx (NEW)
│   ├── forgot-password/page.tsx (NEW)
│   └── reset-password/page.tsx (NEW)
├── app/shared/hr/
│   └── invite-employee-modal.tsx (NEW)
├── services/
│   └── authService.ts (UPDATE - add new methods)
└── messages/
    ├── ar.json (UPDATE)
    └── en.json (UPDATE)
```

---

## 11. Database Migration Script

```sql
-- Migration: Add invitation and password reset fields to sys_user
-- Date: 2025-01-10
-- Description: Support for employee invitation and forgot password workflows

-- Add password reset fields
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS password_reset_token VARCHAR(255);
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS password_reset_expiry TIMESTAMP;

-- Add invitation fields
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS invitation_token VARCHAR(255);
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS invitation_expiry TIMESTAMP;

-- Add activation status (0=pending, 1=activated, for invited accounts)
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS is_activated INT DEFAULT 1;

-- Create index for faster token lookups
CREATE INDEX IF NOT EXISTS idx_sys_user_invitation_token ON sys_user(invitation_token);
CREATE INDEX IF NOT EXISTS idx_sys_user_password_reset_token ON sys_user(password_reset_token);

-- Note: Existing users should have is_activated = 1 (already activated)
UPDATE sys_user SET is_activated = 1 WHERE is_activated IS NULL;
```

---

## 12. Testing Checklist

### Invite Employee Flow
- [ ] HR admin can select employee without account
- [ ] HR admin can enter/edit email
- [ ] HR admin can select roles
- [ ] Invitation email is sent successfully
- [ ] Email contains correct activation link
- [ ] Token expires after 48 hours
- [ ] Employee can access activation page
- [ ] Password validation works correctly
- [ ] Account is activated after setting password
- [ ] Employee is linked to sys_user
- [ ] Employee can login with new credentials
- [ ] Resend invitation generates new token
- [ ] Old token becomes invalid after resend

### Forgot Password Flow
- [ ] User can request password reset
- [ ] Reset email is sent successfully
- [ ] Token expires after 1 hour
- [ ] User can access reset page
- [ ] Password validation works correctly
- [ ] Password is updated successfully
- [ ] Token is invalidated after use
- [ ] User can login with new password
- [ ] Rate limiting prevents abuse

---

## Appendix A: Existing Email Infrastructure

### MailHelper.cs
- Location: `ZR.Common\MailHelper.cs`
- Uses MailKit library
- Supports HTML and plain text
- Supports attachments

### Usage Example:
```csharp
var mailHelper = new MailHelper();
mailHelper.SendMail(
    toAddress: "employee@company.com",
    subject: "Account Activation",
    text: "Plain text version",
    html: "<h1>HTML version</h1>"
);
```

---

## Appendix B: Token Generation Example

```csharp
using System.Security.Cryptography;

public static class TokenGenerator
{
    public static string GenerateSecureToken(int byteLength = 32)
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""); // URL-safe base64
    }
}
```
