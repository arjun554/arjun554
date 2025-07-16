using System.ComponentModel.DataAnnotations;

namespace FoodDash.Shared.DTOs.Menu;

public class MenuItemDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    public string? ImageUrl { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    [Required]
    public int RestaurantId { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsSpicy { get; set; }
    
    public int PreparationTime { get; set; } // in minutes
    public int Calories { get; set; }
    
    public string? Ingredients { get; set; }
    public string? Allergens { get; set; }
    
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MenuCategoryDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    
    [Required]
    public int RestaurantId { get; set; }
    
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<MenuItemDto> MenuItems { get; set; } = new();
}