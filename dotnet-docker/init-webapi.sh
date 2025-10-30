#!/bin/bash

# Initialize a new .NET WebAPI project with Entity Framework using Docker

set -e

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
PROJECT_NAME="${1:-MyWebAPI}"
DB_PROVIDER="${2:-postgres}"
AUTH_TYPE="${3:-none}"

# Functions
print_header() {
    echo -e "${CYAN}╔════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║        .NET WebAPI Project Initializer                ║${NC}"
    echo -e "${CYAN}╚════════════════════════════════════════════════════════╝${NC}"
}

print_info() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_step() {
    echo -e "\n${BLUE}→${NC} $1"
}

print_usage() {
    echo -e "${YELLOW}Usage:${NC}"
    echo "  ./init-webapi.sh [PROJECT_NAME] [DB_PROVIDER] [AUTH_TYPE]"
    echo ""
    echo -e "${YELLOW}Arguments:${NC}"
    echo "  PROJECT_NAME  - Name of your project (default: MyWebAPI)"
    echo "  DB_PROVIDER   - Database provider: postgres|mysql|sqlite|sqlserver (default: postgres)"
    echo "  AUTH_TYPE     - Authentication type: none|jwt|identity (default: none)"
    echo ""
    echo -e "${YELLOW}Example:${NC}"
    echo "  ./init-webapi.sh TodoAPI postgres jwt"
}

# Build Docker image for initialization
build_init_image() {
    print_step "Building initialization Docker image..."

    docker build -t dotnet-init:latest - <<'EOF'
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Install additional templates
RUN dotnet new install Microsoft.AspNetCore.Identity.UI

WORKDIR /workspace
EOF

    print_info "Docker image built successfully"
}

# Main initialization function
main() {
    print_header

    if [[ "$1" == "--help" || "$1" == "-h" ]]; then
        print_usage
        exit 0
    fi

    print_info "Project Name: $PROJECT_NAME"
    print_info "Database Provider: $DB_PROVIDER"
    print_info "Authentication: $AUTH_TYPE"

    # Create project directory
    print_step "Creating project directory..."
    mkdir -p "$PROJECT_NAME"
    cd "$PROJECT_NAME"

    # Build Docker image
    build_init_image

    # Create the WebAPI project
    print_step "Creating WebAPI project..."
    docker run --rm -v "$(pwd):/workspace" dotnet-init:latest \
        dotnet new webapi -n "$PROJECT_NAME" --no-https

    cd "$PROJECT_NAME"

    # Add necessary NuGet packages based on database provider
    print_step "Adding Entity Framework packages..."

    case "$DB_PROVIDER" in
        postgres)
            docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
                cd $PROJECT_NAME && \
                dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL && \
                dotnet add package Microsoft.EntityFrameworkCore.Design && \
                dotnet add package Microsoft.EntityFrameworkCore.Tools
            "
            CONNECTION_STRING="Host=localhost;Database=${PROJECT_NAME}Db;Username=devuser;Password=devpass123"
            ;;
        mysql)
            docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
                cd $PROJECT_NAME && \
                dotnet add package Pomelo.EntityFrameworkCore.MySql && \
                dotnet add package Microsoft.EntityFrameworkCore.Design && \
                dotnet add package Microsoft.EntityFrameworkCore.Tools
            "
            CONNECTION_STRING="Server=localhost;Database=${PROJECT_NAME}Db;Uid=root;Pwd=rootpass123;"
            ;;
        sqlite)
            docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
                cd $PROJECT_NAME && \
                dotnet add package Microsoft.EntityFrameworkCore.Sqlite && \
                dotnet add package Microsoft.EntityFrameworkCore.Design && \
                dotnet add package Microsoft.EntityFrameworkCore.Tools
            "
            CONNECTION_STRING="Data Source=${PROJECT_NAME}.db"
            ;;
        sqlserver)
            docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
                cd $PROJECT_NAME && \
                dotnet add package Microsoft.EntityFrameworkCore.SqlServer && \
                dotnet add package Microsoft.EntityFrameworkCore.Design && \
                dotnet add package Microsoft.EntityFrameworkCore.Tools
            "
            CONNECTION_STRING="Server=localhost;Database=${PROJECT_NAME}Db;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
            ;;
        *)
            print_error "Unknown database provider: $DB_PROVIDER"
            exit 1
            ;;
    esac

    # Add authentication packages if needed
    if [[ "$AUTH_TYPE" != "none" ]]; then
        print_step "Adding authentication packages..."

        case "$AUTH_TYPE" in
            jwt)
                docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
                    cd $PROJECT_NAME && \
                    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer && \
                    dotnet add package System.IdentityModel.Tokens.Jwt
                "
                ;;
            identity)
                docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
                    cd $PROJECT_NAME && \
                    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore && \
                    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
                "
                ;;
        esac
    fi

    # Create basic DbContext
    print_step "Creating DbContext..."

    mkdir -p Data Models

    cat > Data/AppDbContext.cs <<EOF
using Microsoft.EntityFrameworkCore;
using ${PROJECT_NAME}.Models;

namespace ${PROJECT_NAME}.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Add your DbSets here
        // public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add your model configurations here
        }
    }
}
EOF

    # Create a sample model
    cat > Models/SampleModel.cs <<EOF
using System;
using System.ComponentModel.DataAnnotations;

namespace ${PROJECT_NAME}.Models
{
    public class SampleModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
EOF

    # Update Program.cs to include EF Core
    print_step "Updating Program.cs..."

    cat > Program.cs <<EOF
using Microsoft.EntityFrameworkCore;
using ${PROJECT_NAME}.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
EOF

    # Add the appropriate database provider configuration
    case "$DB_PROVIDER" in
        postgres)
            echo '    options.UseNpgsql(connectionString);' >> Program.cs
            ;;
        mysql)
            echo '    var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));' >> Program.cs
            echo '    options.UseMySql(connectionString, serverVersion);' >> Program.cs
            ;;
        sqlite)
            echo '    options.UseSqlite(connectionString);' >> Program.cs
            ;;
        sqlserver)
            echo '    options.UseSqlServer(connectionString);' >> Program.cs
            ;;
    esac

    cat >> Program.cs <<EOF
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}

app.Run();
EOF

    # Create appsettings.json with connection string
    print_step "Creating appsettings.json..."

    cat > appsettings.json <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "$CONNECTION_STRING"
  }
}
EOF

    # Create docker-compose.yml for the project
    print_step "Creating docker-compose.yml..."

    cat > ../docker-compose.yml <<EOF
version: '3.8'

services:
EOF

    # Add database service based on provider
    case "$DB_PROVIDER" in
        postgres)
            cat >> ../docker-compose.yml <<EOF
  postgres:
    image: postgres:16-alpine
    container_name: ${PROJECT_NAME}-db
    environment:
      POSTGRES_USER: devuser
      POSTGRES_PASSWORD: devpass123
      POSTGRES_DB: ${PROJECT_NAME}Db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - ${PROJECT_NAME}-network

EOF
            ;;
        mysql)
            cat >> ../docker-compose.yml <<EOF
  mysql:
    image: mysql:8.0
    container_name: ${PROJECT_NAME}-db
    environment:
      MYSQL_ROOT_PASSWORD: rootpass123
      MYSQL_DATABASE: ${PROJECT_NAME}Db
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    networks:
      - ${PROJECT_NAME}-network

EOF
            ;;
        sqlserver)
            cat >> ../docker-compose.yml <<EOF
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: ${PROJECT_NAME}-db
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - ${PROJECT_NAME}-network

EOF
            ;;
    esac

    # Add web API service
    cat >> ../docker-compose.yml <<EOF
  webapi:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: ${PROJECT_NAME}-api
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5000
EOF

    # Add connection string based on provider
    case "$DB_PROVIDER" in
        postgres)
            echo '      ConnectionStrings__DefaultConnection: "Host=postgres;Database='${PROJECT_NAME}'Db;Username=devuser;Password=devpass123"' >> ../docker-compose.yml
            ;;
        mysql)
            echo '      ConnectionStrings__DefaultConnection: "Server=mysql;Database='${PROJECT_NAME}'Db;Uid=root;Pwd=rootpass123;"' >> ../docker-compose.yml
            ;;
        sqlite)
            echo '      ConnectionStrings__DefaultConnection: "Data Source=/app/data/'${PROJECT_NAME}'.db"' >> ../docker-compose.yml
            ;;
        sqlserver)
            echo '      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database='${PROJECT_NAME}'Db;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"' >> ../docker-compose.yml
            ;;
    esac

    cat >> ../docker-compose.yml <<EOF
    ports:
      - "5000:5000"
    volumes:
      - ./${PROJECT_NAME}:/app
      - ~/.nuget/packages:/root/.nuget/packages:rw
EOF

    if [[ "$DB_PROVIDER" != "sqlite" ]]; then
        echo "    depends_on:" >> ../docker-compose.yml
        echo "      - ${DB_PROVIDER}" >> ../docker-compose.yml
    fi

    cat >> ../docker-compose.yml <<EOF
    networks:
      - ${PROJECT_NAME}-network
    command: ["dotnet", "watch", "run", "--no-restore"]

networks:
  ${PROJECT_NAME}-network:
    driver: bridge

volumes:
EOF

    case "$DB_PROVIDER" in
        postgres)
            echo "  postgres_data:" >> ../docker-compose.yml
            ;;
        mysql)
            echo "  mysql_data:" >> ../docker-compose.yml
            ;;
        sqlserver)
            echo "  sqlserver_data:" >> ../docker-compose.yml
            ;;
    esac

    # Create Dockerfile
    print_step "Creating Dockerfile..."

    cat > ../Dockerfile <<EOF
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development

WORKDIR /app

# Install Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="\${PATH}:/root/.dotnet/tools"

# Copy csproj and restore dependencies
COPY ${PROJECT_NAME}/*.csproj ./
RUN dotnet restore

# Copy everything else
COPY ${PROJECT_NAME}/ ./

EXPOSE 5000

# Use dotnet watch for hot reload in development
CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
EOF

    # Create .env file
    print_step "Creating .env file..."

    cat > ../.env <<EOF
# Database Configuration
DB_PROVIDER=$DB_PROVIDER
DB_USER=devuser
DB_PASSWORD=devpass123
DB_NAME=${PROJECT_NAME}Db

# API Configuration
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development

# Authentication
AUTH_TYPE=$AUTH_TYPE
EOF

    # Create initial migration
    print_step "Creating initial migration..."

    docker run --rm -v "$(pwd):/workspace" dotnet-init:latest bash -c "
        cd /workspace && \
        dotnet ef migrations add InitialCreate
    " 2>/dev/null || true

    # Create a sample controller
    print_step "Creating sample controller..."

    cat > Controllers/SampleController.cs <<EOF
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ${PROJECT_NAME}.Data;
using ${PROJECT_NAME}.Models;

namespace ${PROJECT_NAME}.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SampleController> _logger;

        public SampleController(AppDbContext context, ILogger<SampleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SampleModel>>> GetAll()
        {
            return await _context.Set<SampleModel>().ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SampleModel>> GetById(int id)
        {
            var item = await _context.Set<SampleModel>().FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        [HttpPost]
        public async Task<ActionResult<SampleModel>> Create(SampleModel item)
        {
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _context.Set<SampleModel>().Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SampleModel item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            item.UpdatedAt = DateTime.UtcNow;
            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ItemExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Set<SampleModel>().FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            _context.Set<SampleModel>().Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ItemExists(int id)
        {
            return await _context.Set<SampleModel>().AnyAsync(e => e.Id == id);
        }
    }
}
EOF

    # Create run script for the project
    print_step "Creating run script..."

    cat > ../run.sh <<'EOF'
#!/bin/bash

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}Starting PROJECT_NAME WebAPI with Docker Compose...${NC}"

# Load environment variables
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

# Start services
docker-compose up --build

# Cleanup on exit
trap "docker-compose down" EXIT
EOF

    sed -i "s/PROJECT_NAME/$PROJECT_NAME/g" ../run.sh
    chmod +x ../run.sh

    # Final summary
    echo ""
    echo -e "${GREEN}════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}✓ Project '$PROJECT_NAME' initialized successfully!${NC}"
    echo -e "${GREEN}════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${CYAN}Project Structure:${NC}"
    echo "  $PROJECT_NAME/"
    echo "  ├── $PROJECT_NAME/        # .NET project files"
    echo "  ├── docker-compose.yml    # Docker compose configuration"
    echo "  ├── Dockerfile           # Docker image definition"
    echo "  ├── .env                 # Environment variables"
    echo "  └── run.sh              # Run script"
    echo ""
    echo -e "${CYAN}Next Steps:${NC}"
    echo "  1. cd $PROJECT_NAME"
    echo "  2. ./run.sh              # Start the application"
    echo "  3. Visit http://localhost:5000/swagger"
    echo ""
    echo -e "${CYAN}Useful Commands:${NC}"
    echo "  docker-compose up        # Start services"
    echo "  docker-compose down      # Stop services"
    echo "  docker-compose logs -f   # View logs"
    echo ""

    if [[ "$DB_PROVIDER" != "sqlite" ]]; then
        echo -e "${CYAN}Database Connection:${NC}"
        case "$DB_PROVIDER" in
            postgres)
                echo "  Host: localhost"
                echo "  Port: 5432"
                echo "  Database: ${PROJECT_NAME}Db"
                echo "  Username: devuser"
                echo "  Password: devpass123"
                ;;
            mysql)
                echo "  Host: localhost"
                echo "  Port: 3306"
                echo "  Database: ${PROJECT_NAME}Db"
                echo "  Username: root"
                echo "  Password: rootpass123"
                ;;
            sqlserver)
                echo "  Host: localhost"
                echo "  Port: 1433"
                echo "  Database: ${PROJECT_NAME}Db"
                echo "  Username: sa"
                echo "  Password: YourStrong@Passw0rd"
                ;;
        esac
    fi
}

# Run the main function
main "$@"