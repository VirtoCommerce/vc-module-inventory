set V=2.10.4
nuget push VirtoCommerce.InventoryModule.Client.%V%.nupkg -Source nuget.org -ApiKey %1
nuget push VirtoCommerce.InventoryModule.Data.%V%.nupkg -Source nuget.org -ApiKey %1
pause
