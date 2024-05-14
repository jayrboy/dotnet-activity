# เริ่มต้น ASP.NET Core MVC

- ติดตั้ง (.NET Core)
  https://dotnet.microsoft.com/en-us/download

- ติดตั้ง (VS Code)
  https://code.visualstudio.com/

- ติดตั้ง (SQL Server)
  https://www.microsoft.com/th-th/sql-server/sql-server-downloads

- ติดตั้ง (SQL Server Management Studio)
  https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16#download-ssms

```sh
DBCC CHECKIDENT ('mytable', RESEED, 0)

dotnet tool install --global dotnet-ef --version 8.*

dotnet new webapi --use-controllers -o activityCore
cd activityCore

dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
code .
```

- ติดตั้ง Extension C# Dev Kit
- เริ่มต้นที่ไฟล์ Program.cs กด F5 เพื่อทดสอบ

# MVC (Model-View-Controller)

- Model
- View -> UI โดย Controller จะนำข้อมูลจาก Model ที่ส่งมาให้ View และทำหน้าที่ในการส่งข้อมูลกลับมาที่ Controller
- Controller -> ควบคุมลำดับการทำงาน และ รับ-ส่ง (req,res) จาก View

# REST API

# SQL Server Management Studio (Connection)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost; Database=Activity; Trusted_Connection=False; TrustServerCertificate=True; User ID=sa; Password=Password "
  }
}
```

```sh
dotnet build

dotnet ef dbcontext scaffold "Data Source=BUMBIM\SQLEXPRESS;Initial Catalog=Activity;Integrated Security=True;Encrypt=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Models --force
```

# Metadata (\*remark\*)

- สิ่งที่เก็บข้อมูลเพิ่มเติมของ Model ต่างๆ เช่น class, method, attribute
- ไฟล์ EmployeeMetadata.cs , EmployeeController.cs

# CRUD

- Create
- Read
- Update
- Delete (Soft Delete)
- Set Route Path

# Jwt

1. Add Packages

```sh
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

## Add Generate Documentation File

2. Add to file "appsettings.json"

```json
{
  "Jwt": {
    "Issuer": "http://localhost:8000",
    "Audience": "http://localhost:8000",
    "Key": "YourSecretKeyForAuthenticationOfApplicationDeveloper"
  }
}
```

3. Config to file Program.cs

```cs
builder.Services.AddSwaggerGen(options =>
{
    // Swagger Header
    // ...

    // Add Security Definition - Jwt (1)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer",
    });

    // Add Security Requirement by OpenApi - Jwt (2)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference
                  {
                      Type = ReferenceType.SecurityScheme,
                      Id = "Bearer"
                  }
              },
              new string[] { }
          }
    });
});

// Add Authorization -Jwt (3)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

app.UseAuthentication();
app.UseAuthorization();
```

## Controller - Jwt

- ใน LoginController สร้างตัวแปร TokenSecret = "something"

```cs
public class LoginController : ControllerBase
{
    private const string TokenSecret = "YourSecretKeyForAuthenticationOfApplicationDeveloper";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(1);

    // ...
}
```

- สร้างเมธอดสำหรับ Action - Generate Token

```cs
  [HttpPost("token", Name = "GenerateToken")]
    public string GenerateToken([FromBody] Account user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
            // Add more claims as needed
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            Issuer = "http://localhost:8000",
            Audience = "http://localhost:8000",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        if (token != null)
        {
            var jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
        else
        {
            return "Failed to write token.";
        }
    }
```

### Login

```cs
    [HttpPost(Name = "Login")]
    public IActionResult SignIn([FromBody] Account requestUser)
    {
        Account? user = _db.Accounts.FirstOrDefault(doc => doc.Username == requestUser.Username && doc.Password == requestUser.Password && doc.IsDelete == false);

        if (user.Role == null)
        {
            return Unauthorized();
        }

        string bearerToken;

        try
        {
            bearerToken = GenerateToken(user);
        }
        catch
        {
            return StatusCode(500);
        }
        return Ok(new Response
        {
            Code = 200,
            Message = "Success",
            Data = new
            {
                token = bearerToken
            }
        });
    }
```

- ทดสอบ Swagger
