#!/bin/bash

# Run script for .NET projects with Docker
# Similar to de-run.sh but more flexible for any .NET project

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_PATH="${1:-.}"
COMMAND="${2:-run}"

# Functions
print_header() {
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}       .NET Docker Runner${NC}"
    echo -e "${BLUE}============================================${NC}"
}

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Check if Docker is installed
check_docker() {
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker first."
        exit 1
    fi

    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running. Please start Docker."
        exit 1
    fi
}

# Main execution
main() {
    print_header

    # Check Docker
    check_docker

    # Navigate to project path
    cd "$PROJECT_PATH"

    print_info "Working directory: $(pwd)"

    # Check for .csproj file
    if ! ls *.csproj 1> /dev/null 2>&1; then
        print_error "No .csproj file found in the current directory"
        exit 1
    fi

    PROJECT_NAME=$(ls *.csproj | head -1 | sed 's/.csproj$//')
    print_info "Found project: $PROJECT_NAME"

    # Build Docker image if needed
    DOCKER_IMAGE="dotnet-runner:latest"
    print_info "Building Docker image..."

    docker build -t "$DOCKER_IMAGE" - <<EOF
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
RUN dotnet tool install --global dotnet-ef
ENV PATH="\${PATH}:/root/.dotnet/tools"
EXPOSE 5000
EOF

    # Detect if it's a web project
    if grep -q "Microsoft.AspNetCore" *.csproj; then
        print_info "Detected ASP.NET Core project"

        # Run with appropriate settings for web projects
        case "$COMMAND" in
            "run")
                print_info "Starting application with hot reload..."
                docker run --rm -it \
                    -p 5000:5000 \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    -e ASPNETCORE_ENVIRONMENT=Development \
                    -e ASPNETCORE_URLS="http://+:5000" \
                    --name "${PROJECT_NAME}-runner" \
                    "$DOCKER_IMAGE" \
                    dotnet watch run --no-restore
                ;;
            "build")
                print_info "Building project..."
                docker run --rm \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet build
                ;;
            "test")
                print_info "Running tests..."
                docker run --rm \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet test
                ;;
            "ef")
                shift 2
                print_info "Running Entity Framework command..."
                docker run --rm -it \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet ef "$@"
                ;;
            *)
                print_info "Running custom command: dotnet $COMMAND ${@:3}"
                docker run --rm -it \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet "$COMMAND" "${@:3}"
                ;;
        esac
    else
        # Console application or library
        print_info "Detected Console/Library project"

        case "$COMMAND" in
            "run")
                print_info "Running application..."
                docker run --rm -it \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    --name "${PROJECT_NAME}-runner" \
                    "$DOCKER_IMAGE" \
                    dotnet run
                ;;
            "build")
                print_info "Building project..."
                docker run --rm \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet build
                ;;
            "test")
                print_info "Running tests..."
                docker run --rm \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet test
                ;;
            *)
                print_info "Running custom command: dotnet $COMMAND ${@:3}"
                docker run --rm -it \
                    -v "$(pwd):/app" \
                    -v "$HOME/.nuget/packages:/root/.nuget/packages" \
                    "$DOCKER_IMAGE" \
                    dotnet "$COMMAND" "${@:3}"
                ;;
        esac
    fi
}

# Run main function
main "$@"