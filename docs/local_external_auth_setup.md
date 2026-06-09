# Local External Auth Setup

This project uses JWT bearer authentication. For local API testing, the API must receive a valid access token from an external identity provider.

The examples below use Auth0 because it is quick to set up for demos and interviews.

## Current API Expectations

`Zapas.Api` reads these settings:

```json
{
  "Jwt": {
    "Authority": "https://YOUR_AUTH0_DOMAIN/",
    "Audience": "zapas-api"
  }
}
```

Protected routes:

- `GET /sessions` requires an authenticated token.
- `GET /sessions/{id}` requires an authenticated token.
- `POST /sessions` requires an authenticated token with the `Athlete` role.

## 1. Create An Auth0 API

1. Open Auth0 Dashboard.
2. Go to **Applications > APIs**.
3. Create an API:
   - Name: `Zapas API`
   - Identifier: `zapas-api`
   - Signing Algorithm: `RS256`
4. Keep the API identifier. It becomes `Jwt:Audience`.

## 2. Create A Test Application

1. Go to **Applications > Applications**.
2. Create an application:
   - Name: `Zapas Local Test Client`
   - Type: `Single Page Application`
3. In the application settings, add:
   - Allowed Callback URLs: `https://localhost:7146/callback`
   - Allowed Logout URLs: `https://localhost:7146`
   - Allowed Web Origins: `https://localhost:7146`
   - Allowed Origins (CORS): `https://localhost:7146`

You can use any local frontend URL, but it must match the URL used to log in and request a token.

## 3. Add An Athlete Role Claim

The API currently checks ASP.NET Core roles with:

```csharp
policy.RequireRole("Athlete");
```

Create an Auth0 Action that adds the user roles to the token using the ASP.NET Core role claim URI.

1. Go to **Actions > Library > Build Custom**.
2. Choose **Login / Post Login**.
3. Add this code:

```js
exports.onExecutePostLogin = async (event, api) => {
  const roles = event.authorization?.roles ?? [];

  api.accessToken.setCustomClaim(
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
    roles,
  );
};
```

4. Deploy the Action.
5. Go to **Actions > Flows > Login** and add the Action to the Login flow.

## 4. Create A Test User And Assign Role

1. Go to **User Management > Roles**.
2. Create a role named `Athlete`.
3. Go to **User Management > Users**.
4. Create or select a test user.
5. Assign the `Athlete` role to that user.

## 5. Configure Zapas Locally

Update `Zapas.Api/appsettings.Development.json`:

```json
{
  "Jwt": {
    "Authority": "https://YOUR_AUTH0_DOMAIN/",
    "Audience": "zapas-api"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

Example Auth0 authority:

```json
"Authority": "https://dev-example.us.auth0.com/"
```

Do not commit real tenant-specific secrets or client secrets.

## 6. Get A Token

For an interview demo, the fastest path is to use Auth0's application quickstart or a small local frontend to log in as the test user and copy the access token.

The access token must be requested with:

- Audience: `zapas-api`
- Scope: any scopes your client asks for, if applicable
- User: the test user with the `Athlete` role

After login, inspect the access token at `jwt.io`. It should contain:

```json
{
  "aud": "zapas-api",
  "iss": "https://YOUR_AUTH0_DOMAIN/",
  "sub": "auth0|...",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": ["Athlete"]
}
```

## 7. Run And Test The API

Start the API:

```bash
dotnet run --project Zapas.Api
```

Open Swagger:

```text
https://localhost:PORT/swagger
```

Click **Authorize** and enter:

```text
Bearer YOUR_ACCESS_TOKEN
```

Then test:

- `GET /sessions?page=1&pageSize=10`
- `POST /sessions` with a `.fit` file

## Troubleshooting

`401 Unauthorized` usually means the token is missing, expired, signed by a different issuer, or has the wrong `aud`.

`403 Forbidden` on `POST /sessions` usually means the token is valid but does not contain the `Athlete` role claim.

If Swagger accepts the token but requests still fail, copy the access token into `jwt.io` and verify `iss`, `aud`, and the role claim exactly match the local API settings.

npm run auth0-config -- --domain dev-c61swep314gmbrio.us.auth0.com --clientId BvJGQRhVzGOfwmFsUHgZ8YhwSTcRjRmI --port 7145 &&

npm run dev
