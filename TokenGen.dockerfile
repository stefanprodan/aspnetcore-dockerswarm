FROM microsoft/dotnet:latest

# Set environment variables
ENV ASPNETCORE_URLS="http://*:5000"
ENV ASPNETCORE_ENVIRONMENT="Staging"

# Copy files to app directory
COPY /src/TokenGen /app

# Set working directory
WORKDIR /app

# Restore NuGet packages
RUN ["dotnet", "restore"]

# Build the app
RUN ["dotnet", "build"]

# Open port
EXPOSE 5000/tcp

# Run the app
ENTRYPOINT ["dotnet", "run"]