# JWT Bearer Token Migration Guide

## Overview

The Invoice Management API has been successfully migrated from **cookie-based session authentication** to **JWT Bearer token authentication**. This makes the API truly RESTful, stateless, and easier to integrate with various clients.

## What Changed

### 1. Packages Added
- `Microsoft.AspNetCore.Authentication.JwtBearer` (v8.0.11)
- `System.IdentityModel.Tokens.Jwt` (v8.2.1)

### 2. Configuration Added (appsettings.json)
```json
{
  "Jwt": {
    "Key": "7c387550bf92bf47f906f59c63c8aaae",
    "Issuer": "InvoiceManagementAPI",
    "Audience": "InvoiceManagementAPI",
    "ExpiryInMinutes": "30"
  }
}
```

**IMPORTANT FOR PRODUCTION:** Change the `Key` to a strong, secure random key and store it in environment variables or secure key vault.

### 3. New Service Created
- **IJwtTokenService** - Interface for JWT token generation
- **JwtTokenService** - Implementation that generates JWT tokens with user claims

### 4. Program.cs Changes
- ✅ Added JWT Bearer authentication middleware
- ✅ Configured JWT validation parameters
- ✅ Added Bearer authentication to Swagger UI
- ❌ Removed session middleware (`AddSession`, `UseSession`)
- ❌ Removed session timeout middleware (`UseSessionTimeout`)

### 5. AuthController Changes
- ✅ Login endpoint now returns JWT token in response
- ✅ Removed SignInManager dependency (no longer needed)
- ✅ Uses `CheckPasswordAsync` for password verification
- ✅ Logout endpoint simplified (client-side token deletion)

### 6. LoginResponse DTO Updated
Added new fields:
```csharp
public string Token { get; set; } = string.Empty;
public DateTime ExpiresAt { get; set; }
```

---

## How to Restart the Application

1. **Stop the current running instance:**
   ```bash
   # Find the process
   lsof -i :8080

   # Kill it (replace PID with actual process ID)
   kill <PID>
   ```

2. **Rebuild the application:**
   ```bash
   cd /home/river/4projects/openapi_swagger/InvoiceManagement/InvoiceManagement
   dotnet build
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

---

## How to Test JWT Authentication

### Step 1: Login and Get JWT Token

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

**Expected Response:**
```json
{
  "userId": 1,
  "username": "admin",
  "email": "admin@invoice.local",
  "role": "Administrator",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-30T14:30:00Z",
  "message": "Login successful"
}
```

**Copy the `token` value from the response.**

### Step 2: Use Token in Subsequent Requests

```bash
# Set token as variable for convenience
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Get current user info
curl -X GET http://localhost:8080/api/auth/me \
  -H "Authorization: Bearer $TOKEN"

# List all invoices
curl -X GET http://localhost:8080/api/invoices \
  -H "Authorization: Bearer $TOKEN"

# Create new invoice
curl -X POST http://localhost:8080/api/invoices \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "invoiceNumber": "INV-2025-100",
    "issueDate": "2025-10-30T00:00:00Z",
    "dueDate": "2025-11-30T00:00:00Z",
    "type": 1,
    "partnerName": "Test Corp",
    "partnerIdentifier": "TST-001",
    "amountCents": 500000,
    "paymentStatus": 0
  }'
```

### Step 3: Test Unauthorized Access

```bash
# Try without token - should get 401 Unauthorized
curl -X GET http://localhost:8080/api/invoices
```

---

## Testing with Swagger UI

1. **Open Swagger UI:**
   ```
   http://localhost:8080/swagger
   ```

2. **Click the "Authorize" button** (lock icon in top right)

3. **Enter your token:**
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

   Note: Include the word "Bearer" followed by a space, then your token.

4. **Click "Authorize"** - Now all API requests will automatically include the token!

5. **Test any endpoint** - The Authorization header is automatically added.

---

## Complete Test Script

Save this as `test_jwt.sh`:

```bash
#!/bin/bash

echo "=== Testing JWT Authentication ==="
echo ""

# Step 1: Login
echo "1. Logging in as admin..."
RESPONSE=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}')

echo "Response: $RESPONSE"
echo ""

# Extract token (requires jq)
TOKEN=$(echo $RESPONSE | jq -r '.token')

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
  echo "❌ Failed to get token. Make sure the application is restarted with JWT changes."
  exit 1
fi

echo "✅ Token received: ${TOKEN:0:50}..."
echo ""

# Step 2: Get current user
echo "2. Getting current user info..."
curl -s -X GET http://localhost:8080/api/auth/me \
  -H "Authorization: Bearer $TOKEN" | jq '.'
echo ""

# Step 3: List invoices
echo "3. Listing invoices..."
curl -s -X GET http://localhost:8080/api/invoices \
  -H "Authorization: Bearer $TOKEN" | jq '.items | length'
echo ""

# Step 4: Test unauthorized access
echo "4. Testing without token (should fail)..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/invoices)
if [ "$HTTP_CODE" = "401" ]; then
  echo "✅ Correctly returned 401 Unauthorized"
else
  echo "❌ Expected 401 but got $HTTP_CODE"
fi

echo ""
echo "=== JWT Authentication Tests Complete ==="
```

Make it executable and run:
```bash
chmod +x test_jwt.sh
./test_jwt.sh
```

---

## Benefits of JWT vs Cookies

### ✅ Advantages

1. **Stateless** - No server-side session storage needed
2. **Scalable** - Works perfectly in distributed/microservices architecture
3. **Universal** - Works with any HTTP client (mobile apps, desktop apps, IoT)
4. **Standard** - Industry-standard authentication method for REST APIs
5. **Easy Testing** - Simple to copy/paste tokens between tools
6. **Swagger Integration** - Built-in "Authorize" button support
7. **CORS-Friendly** - No credential issues across domains
8. **No CSRF** - Not vulnerable to CSRF attacks

### 📋 JWT Token Structure

A JWT token contains three parts separated by dots:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.    <- Header
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6I...  <- Payload (Claims)
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQ    <- Signature
```

**Claims included in our tokens:**
- `nameid` - User ID
- `unique_name` - Username
- `email` - Email address
- `role` - User role (Administrator, Accountant, Manager)
- `jti` - Unique token ID
- `exp` - Expiration timestamp
- `iss` - Issuer (InvoiceManagementAPI)
- `aud` - Audience (InvoiceManagementAPI)

You can decode tokens at [jwt.io](https://jwt.io) (they're not encrypted, just signed).

---

## Security Considerations

### 🔒 Production Recommendations

1. **Use HTTPS** - Set `RequireHttpsMetadata = true` in production
2. **Strong Secret Key** - Use a cryptographically secure random key (min 32 chars)
3. **Store Key Securely** - Use environment variables or Azure Key Vault
4. **Short Expiry** - Keep token expiry reasonably short (30 min is good)
5. **Refresh Tokens** - Consider implementing refresh tokens for longer sessions
6. **Token Revocation** - For critical apps, implement a token blacklist
7. **HTTPS Only** - Never send tokens over HTTP in production

### Environment Variable Configuration

For production, use environment variables instead of appsettings.json:

```bash
export Jwt__Key="your-super-secure-random-key-here"
export Jwt__Issuer="InvoiceManagementAPI"
export Jwt__Audience="InvoiceManagementAPI"
export Jwt__ExpiryInMinutes="30"
```

---

## Migration Checklist

- [x] Install JWT packages
- [x] Create JWT token service
- [x] Configure JWT authentication in Program.cs
- [x] Remove session middleware
- [x] Update AuthController to generate tokens
- [x] Update LoginResponse DTO
- [x] Configure Swagger for Bearer tokens
- [ ] **Restart application** ⚠️
- [ ] Test login with JWT
- [ ] Test authorized endpoints
- [ ] Test unauthorized access
- [ ] Update README.md with JWT instructions
- [ ] Update any frontend/client code to use Bearer tokens

---

## Troubleshooting

### Token Not Returned on Login
- ✅ Ensure application has been restarted after code changes
- ✅ Check appsettings.json has JWT configuration
- ✅ Check logs for any startup errors

### 401 Unauthorized on All Requests
- ✅ Ensure token is included: `Authorization: Bearer <token>`
- ✅ Check token hasn't expired
- ✅ Verify JWT configuration matches between token generation and validation

### Swagger Authorize Button Not Working
- ✅ Enter "Bearer " (with space) before the token
- ✅ Click Authorize after entering token
- ✅ Green unlocked padlock means authorized

---

## Next Steps

1. **Restart the application** to apply JWT changes
2. **Test authentication** using the test script above
3. **Update README.md** with JWT authentication examples
4. **Update any client applications** to use Bearer token authentication
5. **Consider implementing refresh tokens** for better UX

---

*Generated during JWT migration - 2025-10-30*
