using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Template_Application.Services.DataLayer;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// Visual master data sync - Initial population, delta sync, background refresh
/// </summary>
public class VisualMasterDataSync
{
    private readonly IVisualApiClient _visualApiClient;
    private readonly ICacheService _cacheService;
    private readonly CacheStalenessDetector _stalenessDetector;

    public VisualMasterDataSync(
        IVisualApiClient visualApiClient,
        ICacheService cacheService,
        CacheStalenessDetector stalenessDetector)
    {
        ArgumentNullException.ThrowIfNull(visualApiClient);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(stalenessDetector);

        _visualApiClient = visualApiClient;
        _cacheService = cacheService;
        _stalenessDetector = stalenessDetector;
    }

    /// <summary>
    /// Perform initial cache population from Visual ERP
    /// </summary>
    public async Task InitialPopulationAsync()
    {
        // Check if Visual server is available
        if (!await _visualApiClient.IsServerAvailable())
        {
            throw new InvalidOperationException("Visual server is not available for initial population");
        }

        // Populate different entity types
        await PopulatePartsAsync();
        await PopulateCustomersAsync();
        await PopulateWarehousesAsync();
        await PopulateOrdersAsync();
    }

    /// <summary>
    /// Perform delta sync to update changed entities
    /// </summary>
    public async Task DeltaSyncAsync()
    {
        // Check if Visual server is available
        if (!await _visualApiClient.IsServerAvailable())
        {
            return; // Skip sync if server unavailable
        }

        // Get stale entries
        var staleEntries = await _stalenessDetector.DetectStaleEntriesAsync();

        // Refresh each stale entry
        foreach (var entry in staleEntries)
        {
            await RefreshEntryAsync(entry.Key, entry.EntityType);
        }
    }

    /// <summary>
    /// Background refresh for expiring cache entries
    /// </summary>
    public async Task BackgroundRefreshAsync()
    {
        // Check if Visual server is available
        if (!await _visualApiClient.IsServerAvailable())
        {
            return; // Skip refresh if server unavailable
        }

        // Get entries nearing expiration
        var nearExpiration = await _stalenessDetector.GetEntriesNearExpirationAsync();

        // Refresh each entry
        foreach (var entry in nearExpiration)
        {
            await RefreshEntryAsync(entry.Key, entry.EntityType);
        }
    }

    private async Task PopulatePartsAsync()
    {
        try
        {
            var parts = await _visualApiClient.ExecuteCommandAsync<List<object>>(
                "GetParts",
                new Dictionary<string, object>());

            if (parts != null)
            {
                foreach (var part in parts)
                {
                    var partId = part.GetType().GetProperty("Id")?.GetValue(part)?.ToString();
                    if (partId != null)
                    {
                        await _cacheService.SetAsync($"Part:{partId}", part, TimeSpan.FromHours(24));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but continue with other entity types
            Console.WriteLine($"Error populating parts: {ex.Message}");
        }
    }

    private async Task PopulateCustomersAsync()
    {
        try
        {
            var customers = await _visualApiClient.ExecuteCommandAsync<List<object>>(
                "GetCustomers",
                new Dictionary<string, object>());

            if (customers != null)
            {
                foreach (var customer in customers)
                {
                    var customerId = customer.GetType().GetProperty("Id")?.GetValue(customer)?.ToString();
                    if (customerId != null)
                    {
                        await _cacheService.SetAsync($"Customer:{customerId}", customer, TimeSpan.FromDays(7));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error populating customers: {ex.Message}");
        }
    }

    private async Task PopulateWarehousesAsync()
    {
        try
        {
            var warehouses = await _visualApiClient.ExecuteCommandAsync<List<object>>(
                "GetWarehouses",
                new Dictionary<string, object>());

            if (warehouses != null)
            {
                foreach (var warehouse in warehouses)
                {
                    var warehouseId = warehouse.GetType().GetProperty("Id")?.GetValue(warehouse)?.ToString();
                    if (warehouseId != null)
                    {
                        await _cacheService.SetAsync($"Warehouse:{warehouseId}", warehouse, TimeSpan.FromDays(7));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error populating warehouses: {ex.Message}");
        }
    }

    private async Task PopulateOrdersAsync()
    {
        try
        {
            var orders = await _visualApiClient.ExecuteCommandAsync<List<object>>(
                "GetRecentOrders",
                new Dictionary<string, object> { ["Days"] = 30 });

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    var orderId = order.GetType().GetProperty("Id")?.GetValue(order)?.ToString();
                    if (orderId != null)
                    {
                        await _cacheService.SetAsync($"Order:{orderId}", order, TimeSpan.FromDays(7));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error populating orders: {ex.Message}");
        }
    }

    private async Task RefreshEntryAsync(string key, string entityType)
    {
        try
        {
            // Determine command based on entity type
            var command = entityType switch
            {
                "Part" => "GetPartById",
                "Customer" => "GetCustomerById",
                "Warehouse" => "GetWarehouseById",
                "Order" => "GetOrderById",
                _ => null
            };

            if (command == null)
            {
                return;
            }

            // Extract ID from key (format: "EntityType:Id")
            var id = key.Split(':')[1];

            var entity = await _visualApiClient.ExecuteCommandAsync<object>(
                command,
                new Dictionary<string, object> { ["Id"] = id });

            if (entity != null)
            {
                var ttl = entityType == "Part" ? TimeSpan.FromHours(24) : TimeSpan.FromDays(7);
                await _cacheService.SetAsync(key, entity, ttl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing entry {key}: {ex.Message}");
        }
    }
}
