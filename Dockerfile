# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY CSharpProxy.csproj ./
RUN dotnet restore

# Copy the entire project and build it
COPY . ./
RUN dotnet publish -c Release -o out

# Set the entry point for the container
CMD ["dotnet", "out/CSharpProxy.dll"]
