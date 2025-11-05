using RecommendationService.Models;
using Newtonsoft.Json;

namespace RecommendationService.Services;

public interface IMenuService
{
    List<MenuItem> GetMenuItems();
    List<MenuItem> GetAvailableItems();
}

public class MenuService : IMenuService
{
    private readonly ILogger<MenuService> _logger;
    private List<MenuItem>? _menuItems;
    private readonly string _menuPath;

    public MenuService(ILogger<MenuService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _menuPath = Path.Combine(env.ContentRootPath, "Data", "menu.json");
        LoadMenu();
    }

    private void LoadMenu()
    {
        try
        {
            if (File.Exists(_menuPath))
            {
                var jsonContent = File.ReadAllText(_menuPath);
                _menuItems = JsonConvert.DeserializeObject<List<MenuItem>>(jsonContent) ?? new List<MenuItem>();
                _logger.LogInformation($"Loaded {_menuItems.Count} menu items from {_menuPath}");
            }
            else
            {
                _logger.LogWarning($"Menu file not found at {_menuPath}");
                _menuItems = new List<MenuItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading menu data");
            _menuItems = new List<MenuItem>();
        }
    }

    public List<MenuItem> GetMenuItems()
    {
        return _menuItems ?? new List<MenuItem>();
    }

    public List<MenuItem> GetAvailableItems()
    {
        return (_menuItems ?? new List<MenuItem>()).Where(item => item.Availability).ToList();
    }
}

