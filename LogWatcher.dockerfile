FROM microsoft/dotnet:latest

# Set environment variables
ENV ASPNETCORE_URLS="http://*:5005"
ENV ASPNETCORE_ENVIRONMENT="Staging"

# Copy files to app directory
COPY /src/RethinkDbLogProvider /app/src/RethinkDbLogProvider
COPY /src/LogWatcher /app/src/LogWatcher
COPY NuGet.Config /app/src/LogWatcher/NuGet.Config

# RethinkDbLogProvider
WORKDIR /app/src/RethinkDbLogProvider
RUN ["dotnet", "restore"]

# Set working directory
WORKDIR /app/src/LogWatcher

# Restore NuGet packages
RUN ["dotnet", "restore"]

# Build the app
RUN ["dotnet", "build"]

# Open port
EXPOSE 5005/tcp

# Run the app
ENTRYPOINT ["dotnet", "run"]