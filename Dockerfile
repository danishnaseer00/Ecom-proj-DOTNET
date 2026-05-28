FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Ecommerce-web-project.sln ./
COPY Ecommerce-web-project.csproj ./Ecommerce-web-project/
COPY ECommerce.Presenter/ECommerce.Presenter.csproj ./ECommerce.Presenter/
COPY ECommerce.Model/ECommerce.Model.csproj ./ECommerce.Model/
RUN dotnet restore ./Ecommerce-web-project/Ecommerce-web-project.csproj

COPY . .
RUN dotnet publish ./Ecommerce-web-project/Ecommerce-web-project.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 7860
ENV ASPNETCORE_URLS=http://0.0.0.0:7860
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Ecommerce-web-project.dll"]
