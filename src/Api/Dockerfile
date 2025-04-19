FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443
# Install ICU libraries for globalization support
RUN apk add --no-cache icu-libs
# Set environment variable to enable globalization support
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src
# Install ICU libraries in the build container as well
RUN apk add --no-cache icu-libs
# Enable globalization support in the build container
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# First create directories for the projects
COPY ["Api/Api.csproj", "Api/"]
COPY ["Data/Data.csproj", "Data/"]

# Restore dependencies
WORKDIR /src/Api
RUN dotnet restore "Api.csproj"

# Copy all project files
WORKDIR /src
COPY Api/. Api/
COPY Data/. Data/

# Build
WORKDIR /src/Api
RUN dotnet build "Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]