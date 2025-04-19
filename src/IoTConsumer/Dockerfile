FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app
# Install ICU libraries for globalization support
RUN apk add --no-cache icu-libs
# Set environment variable to enable globalization support
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Install ICU libraries in the build container as well
RUN apk add --no-cache icu-libs
# Enable globalization support in the build container
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# First create directories for the projects
COPY ["IoTConsumer/IoTConsumer.csproj", "IoTConsumer/"]
COPY ["Data/Data.csproj", "Data/"]

# Restore dependencies
WORKDIR /src/IoTConsumer
RUN dotnet restore "IoTConsumer.csproj"

# Copy all project files
WORKDIR /src
COPY IoTConsumer/. IoTConsumer/
COPY Data/. Data/

# Build
WORKDIR /src/IoTConsumer
RUN dotnet build "IoTConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IoTConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTConsumer.dll"]