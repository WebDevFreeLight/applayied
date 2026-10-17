# Theme Control System - Detailed Implementation Plan

## Overview
This document provides a comprehensive implementation plan for the Theme Control system, enabling users to customize the UI appearance, manage themes, create custom color schemes, and personalize their dashboard experience.

## Theme Control System Architecture

### Core Components
1. **Theme Management**
2. **Color Scheme Customization**
3. **Layout Configuration**
4. **Component Styling**
5. **User Preferences**
6. **Theme Templates**
7. **Dark/Light Mode Toggle**
8. **Brand Customization**
9. **CSS Variable Management**
10. **Theme Preview System**

## 1. Backend Implementation

### Controllers

**ThemeController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ThemeController : BaseController
{
    private readonly IThemeService _themeService;
    private readonly IUserThemeService _userThemeService;
    private readonly IThemeTemplateService _templateService;
    
    public ThemeController(
        IThemeService themeService,
        IUserThemeService userThemeService,
        IThemeTemplateService templateService)
    {
        _themeService = themeService;
        _userThemeService = userThemeService;
        _templateService = templateService;
    }
    
    [HttpGet("themes")]
    public async Task<IActionResult> GetThemes([FromQuery] ThemeQueryDto query)
    {
        var themes = await _themeService.GetThemesAsync(query);
        return Ok(themes);
    }
    
    [HttpGet("themes/{id}")]
    public async Task<IActionResult> GetTheme(long id)
    {
        var theme = await _themeService.GetThemeByIdAsync(id);
        return Ok(theme);
    }
    
    [HttpPost("themes")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateTheme([FromBody] CreateThemeDto theme)
    {
        var result = await _themeService.CreateThemeAsync(theme);
        return Ok(result);
    }
    
    [HttpPut("themes/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateTheme(long id, [FromBody] UpdateThemeDto theme)
    {
        await _themeService.UpdateThemeAsync(id, theme);
        return Ok(new { message = "Theme updated successfully" });
    }
    
    [HttpDelete("themes/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteTheme(long id)
    {
        await _themeService.DeleteThemeAsync(id);
        return Ok(new { message = "Theme deleted successfully" });
    }
    
    [HttpGet("user-theme")]
    public async Task<IActionResult> GetUserTheme()
    {
        var userId = GetCurrentUserId();
        var userTheme = await _userThemeService.GetUserThemeAsync(userId);
        return Ok(userTheme);
    }
    
    [HttpPost("user-theme")]
    public async Task<IActionResult> SaveUserTheme([FromBody] SaveUserThemeDto userTheme)
    {
        var userId = GetCurrentUserId();
        userTheme.UserId = userId;
        var result = await _userThemeService.SaveUserThemeAsync(userTheme);
        return Ok(result);
    }
    
    [HttpPost("user-theme/apply/{themeId}")]
    public async Task<IActionResult> ApplyTheme(long themeId)
    {
        var userId = GetCurrentUserId();
        await _userThemeService.ApplyThemeAsync(userId, themeId);
        return Ok(new { message = "Theme applied successfully" });
    }
    
    [HttpPost("user-theme/reset")]
    public async Task<IActionResult> ResetUserTheme()
    {
        var userId = GetCurrentUserId();
        await _userThemeService.ResetUserThemeAsync(userId);
        return Ok(new { message = "Theme reset to default" });
    }
    
    [HttpGet("templates")]
    public async Task<IActionResult> GetThemeTemplates()
    {
        var templates = await _templateService.GetThemeTemplatesAsync();
        return Ok(templates);
    }
    
    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetThemeTemplate(long id)
    {
        var template = await _templateService.GetThemeTemplateByIdAsync(id);
        return Ok(template);
    }
    
    [HttpPost("templates")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateThemeTemplate([FromBody] CreateThemeTemplateDto template)
    {
        var result = await _templateService.CreateThemeTemplateAsync(template);
        return Ok(result);
    }
    
    [HttpGet("color-palettes")]
    public async Task<IActionResult> GetColorPalettes()
    {
        var palettes = await _themeService.GetColorPalettesAsync();
        return Ok(palettes);
    }
    
    [HttpPost("generate-theme")]
    public async Task<IActionResult> GenerateTheme([FromBody] GenerateThemeDto request)
    {
        var theme = await _themeService.GenerateThemeAsync(request);
        return Ok(theme);
    }
    
    [HttpPost("preview-theme")]
    public async Task<IActionResult> PreviewTheme([FromBody] PreviewThemeDto theme)
    {
        var previewData = await _themeService.GeneratePreviewAsync(theme);
        return Ok(previewData);
    }
    
    [HttpGet("css-variables/{themeId}")]
    public async Task<IActionResult> GetThemeCssVariables(long themeId)
    {
        var cssVariables = await _themeService.GetThemeCssVariablesAsync(themeId);
        return Ok(cssVariables);
    }
    
    [HttpPost("export-theme/{themeId}")]
    public async Task<IActionResult> ExportTheme(long themeId)
    {
        var exportData = await _themeService.ExportThemeAsync(themeId);
        return File(exportData, "application/json", $"theme-{themeId}.json");
    }
    
    [HttpPost("import-theme")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ImportTheme([FromBody] ImportThemeDto importData)
    {
        var result = await _themeService.ImportThemeAsync(importData);
        return Ok(result);
    }
}
```

**UserPreferencesController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserPreferencesController : BaseController
{
    private readonly IUserPreferencesService _preferencesService;
    
    public UserPreferencesController(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }
    
    [HttpGet("preferences")]
    public async Task<IActionResult> GetUserPreferences()
    {
        var userId = GetCurrentUserId();
        var preferences = await _preferencesService.GetUserPreferencesAsync(userId);
        return Ok(preferences);
    }
    
    [HttpPost("preferences")]
    public async Task<IActionResult> SaveUserPreferences([FromBody] SaveUserPreferencesDto preferences)
    {
        var userId = GetCurrentUserId();
        preferences.UserId = userId;
        await _preferencesService.SaveUserPreferencesAsync(preferences);
        return Ok(new { message = "Preferences saved successfully" });
    }
    
    [HttpPut("preferences/layout")]
    public async Task<IActionResult> UpdateLayoutPreferences([FromBody] UpdateLayoutPreferencesDto layout)
    {
        var userId = GetCurrentUserId();
        await _preferencesService.UpdateLayoutPreferencesAsync(userId, layout);
        return Ok(new { message = "Layout preferences updated" });
    }
    
    [HttpPut("preferences/sidebar")]
    public async Task<IActionResult> UpdateSidebarPreferences([FromBody] UpdateSidebarPreferencesDto sidebar)
    {
        var userId = GetCurrentUserId();
        await _preferencesService.UpdateSidebarPreferencesAsync(userId, sidebar);
        return Ok(new { message = "Sidebar preferences updated" });
    }
    
    [HttpPut("preferences/dashboard")]
    public async Task<IActionResult> UpdateDashboardPreferences([FromBody] UpdateDashboardPreferencesDto dashboard)
    {
        var userId = GetCurrentUserId();
        await _preferencesService.UpdateDashboardPreferencesAsync(userId, dashboard);
        return Ok(new { message = "Dashboard preferences updated" });
    }
    
    [HttpPost("preferences/reset")]
    public async Task<IActionResult> ResetPreferences()
    {
        var userId = GetCurrentUserId();
        await _preferencesService.ResetUserPreferencesAsync(userId);
        return Ok(new { message = "Preferences reset to default" });
    }
}
```

### Services

**IThemeService.cs**
```csharp
public interface IThemeService
{
    Task<PagedResult<ThemeDto>> GetThemesAsync(ThemeQueryDto query);
    Task<ThemeDto> GetThemeByIdAsync(long themeId);
    Task<ThemeDto> CreateThemeAsync(CreateThemeDto theme);
    Task UpdateThemeAsync(long themeId, UpdateThemeDto theme);
    Task DeleteThemeAsync(long themeId);
    Task<List<ColorPaletteDto>> GetColorPalettesAsync();
    Task<ThemeDto> GenerateThemeAsync(GenerateThemeDto request);
    Task<ThemePreviewDto> GeneratePreviewAsync(PreviewThemeDto theme);
    Task<Dictionary<string, string>> GetThemeCssVariablesAsync(long themeId);
    Task<byte[]> ExportThemeAsync(long themeId);
    Task<ThemeDto> ImportThemeAsync(ImportThemeDto importData);
    Task<ThemeDto> GetDefaultThemeAsync();
    Task<List<ThemeDto>> GetSystemThemesAsync();
}
```

**ThemeService.cs**
```csharp
[AppService(ServiceLifetime = LifeTime.Transient)]
public class ThemeService : BaseService<Theme>, IThemeService
{
    private readonly IThemeRepository _themeRepository;
    private readonly IColorPaletteService _colorPaletteService;
    private readonly ICssGeneratorService _cssGeneratorService;
    private readonly IThemeValidationService _validationService;
    
    public ThemeService(
        IThemeRepository themeRepository,
        IColorPaletteService colorPaletteService,
        ICssGeneratorService cssGeneratorService,
        IThemeValidationService validationService)
    {
        _themeRepository = themeRepository;
        _colorPaletteService = colorPaletteService;
        _cssGeneratorService = cssGeneratorService;
        _validationService = validationService;
    }
    
    public async Task<PagedResult<ThemeDto>> GetThemesAsync(ThemeQueryDto query)
    {
        var themes = await _themeRepository.GetThemesAsync(query);
        return themes.Adapt<PagedResult<ThemeDto>>();
    }
    
    public async Task<ThemeDto> GetThemeByIdAsync(long themeId)
    {
        var theme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (theme == null)
            throw new NotFoundException("Theme not found");
            
        return theme.Adapt<ThemeDto>();
    }
    
    public async Task<ThemeDto> CreateThemeAsync(CreateThemeDto createTheme)
    {
        // Validate theme data
        await _validationService.ValidateThemeAsync(createTheme);
        
        var theme = createTheme.Adapt<Theme>();
        theme.CreatedAt = DateTime.UtcNow;
        theme.CreatedBy = GetCurrentUserId();
        theme.IsActive = true;
        
        // Generate CSS variables
        theme.CssVariables = await _cssGeneratorService.GenerateCssVariablesAsync(createTheme);
        
        var result = await _themeRepository.CreateThemeAsync(theme);
        return result.Adapt<ThemeDto>();
    }
    
    public async Task UpdateThemeAsync(long themeId, UpdateThemeDto updateTheme)
    {
        var existingTheme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (existingTheme == null)
            throw new NotFoundException("Theme not found");
            
        // Validate theme data
        await _validationService.ValidateThemeAsync(updateTheme);
        
        existingTheme = updateTheme.Adapt(existingTheme);
        existingTheme.UpdatedAt = DateTime.UtcNow;
        existingTheme.UpdatedBy = GetCurrentUserId();
        
        // Regenerate CSS variables
        existingTheme.CssVariables = await _cssGeneratorService.GenerateCssVariablesAsync(updateTheme);
        
        await _themeRepository.UpdateThemeAsync(existingTheme);
    }
    
    public async Task DeleteThemeAsync(long themeId)
    {
        var theme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (theme == null)
            throw new NotFoundException("Theme not found");
            
        if (theme.IsSystemTheme)
            throw new InvalidOperationException("Cannot delete system theme");
            
        await _themeRepository.DeleteThemeAsync(themeId);
    }
    
    public async Task<List<ColorPaletteDto>> GetColorPalettesAsync()
    {
        var palettes = await _colorPaletteService.GetColorPalettesAsync();
        return palettes.Adapt<List<ColorPaletteDto>>();
    }
    
    public async Task<ThemeDto> GenerateThemeAsync(GenerateThemeDto request)
    {
        var generatedTheme = await _colorPaletteService.GenerateThemeFromPaletteAsync(request);
        return generatedTheme.Adapt<ThemeDto>();
    }
    
    public async Task<ThemePreviewDto> GeneratePreviewAsync(PreviewThemeDto theme)
    {
        var cssVariables = await _cssGeneratorService.GenerateCssVariablesAsync(theme);
        var previewHtml = await _cssGeneratorService.GeneratePreviewHtmlAsync(theme, cssVariables);
        
        return new ThemePreviewDto
        {
            CssVariables = cssVariables,
            PreviewHtml = previewHtml,
            ThemeData = theme
        };
    }
    
    public async Task<Dictionary<string, string>> GetThemeCssVariablesAsync(long themeId)
    {
        var theme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (theme == null)
            throw new NotFoundException("Theme not found");
            
        return JsonSerializer.Deserialize<Dictionary<string, string>>(theme.CssVariables ?? "{}");
    }
    
    public async Task<byte[]> ExportThemeAsync(long themeId)
    {
        var theme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (theme == null)
            throw new NotFoundException("Theme not found");
            
        var exportData = theme.Adapt<ThemeExportDto>();
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }
    
    public async Task<ThemeDto> ImportThemeAsync(ImportThemeDto importData)
    {
        // Validate import data
        await _validationService.ValidateImportDataAsync(importData);
        
        var theme = importData.Adapt<Theme>();
        theme.CreatedAt = DateTime.UtcNow;
        theme.CreatedBy = GetCurrentUserId();
        theme.IsActive = true;
        theme.IsSystemTheme = false;
        
        var result = await _themeRepository.CreateThemeAsync(theme);
        return result.Adapt<ThemeDto>();
    }
    
    public async Task<ThemeDto> GetDefaultThemeAsync()
    {
        var defaultTheme = await _themeRepository.GetDefaultThemeAsync();
        return defaultTheme?.Adapt<ThemeDto>();
    }
    
    public async Task<List<ThemeDto>> GetSystemThemesAsync()
    {
        var systemThemes = await _themeRepository.GetSystemThemesAsync();
        return systemThemes.Adapt<List<ThemeDto>>();
    }
}
```

### Models and DTOs

**Theme.cs**
```csharp
[SugarTable("system_themes")]
public class Theme : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long ThemeId { get; set; }
    
    [SugarColumn(Length = 100)]
    public string ThemeName { get; set; }
    
    [SugarColumn(Length = 500)]
    public string Description { get; set; }
    
    [SugarColumn(Length = 20)]
    public string ThemeType { get; set; } = "Custom"; // System, Custom, Template
    
    public bool IsSystemTheme { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsDarkMode { get; set; } = false;
    
    // Primary Colors
    [SugarColumn(Length = 10)]
    public string PrimaryColor { get; set; } = "#409EFF";
    
    [SugarColumn(Length = 10)]
    public string PrimaryLightColor { get; set; } = "#66B2FF";
    
    [SugarColumn(Length = 10)]
    public string PrimaryDarkColor { get; set; } = "#337ECC";
    
    // Secondary Colors
    [SugarColumn(Length = 10)]
    public string SecondaryColor { get; set; } = "#909399";
    
    [SugarColumn(Length = 10)]
    public string SecondaryLightColor { get; set; } = "#B3B6BD";
    
    [SugarColumn(Length = 10)]
    public string SecondaryDarkColor { get; set; } = "#73767A";
    
    // Status Colors
    [SugarColumn(Length = 10)]
    public string SuccessColor { get; set; } = "#67C23A";
    
    [SugarColumn(Length = 10)]
    public string WarningColor { get; set; } = "#E6A23C";
    
    [SugarColumn(Length = 10)]
    public string DangerColor { get; set; } = "#F56C6C";
    
    [SugarColumn(Length = 10)]
    public string InfoColor { get; set; } = "#909399";
    
    // Background Colors
    [SugarColumn(Length = 10)]
    public string BackgroundColor { get; set; } = "#FFFFFF";
    
    [SugarColumn(Length = 10)]
    public string SurfaceColor { get; set; } = "#F5F7FA";
    
    [SugarColumn(Length = 10)]
    public string CardColor { get; set; } = "#FFFFFF";
    
    // Text Colors
    [SugarColumn(Length = 10)]
    public string TextPrimaryColor { get; set; } = "#303133";
    
    [SugarColumn(Length = 10)]
    public string TextSecondaryColor { get; set; } = "#606266";
    
    [SugarColumn(Length = 10)]
    public string TextPlaceholderColor { get; set; } = "#C0C4CC";
    
    // Border Colors
    [SugarColumn(Length = 10)]
    public string BorderColor { get; set; } = "#DCDFE6";
    
    [SugarColumn(Length = 10)]
    public string BorderLightColor { get; set; } = "#E4E7ED";
    
    [SugarColumn(Length = 10)]
    public string BorderDarkColor { get; set; } = "#D3D4D6";
    
    // Layout Settings
    [SugarColumn(Length = 20)]
    public string SidebarWidth { get; set; } = "250px";
    
    [SugarColumn(Length = 20)]
    public string HeaderHeight { get; set; } = "60px";
    
    [SugarColumn(Length = 20)]
    public string FooterHeight { get; set; } = "40px";
    
    [SugarColumn(Length = 20)]
    public string BorderRadius { get; set; } = "4px";
    
    [SugarColumn(Length = 20)]
    public string BoxShadow { get; set; } = "0 2px 12px 0 rgba(0, 0, 0, 0.1)";
    
    // Typography
    [SugarColumn(Length = 100)]
    public string FontFamily { get; set; } = "'Helvetica Neue', Helvetica, 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', Arial, sans-serif";
    
    [SugarColumn(Length = 20)]
    public string FontSizeBase { get; set; } = "14px";
    
    [SugarColumn(Length = 20)]
    public string FontSizeSmall { get; set; } = "12px";
    
    [SugarColumn(Length = 20)]
    public string FontSizeLarge { get; set; } = "16px";
    
    [SugarColumn(Length = 20)]
    public string LineHeight { get; set; } = "1.5";
    
    // Custom CSS Variables (JSON)
    [SugarColumn(ColumnDataType = "TEXT")]
    public string CssVariables { get; set; }
    
    // Custom CSS Rules (JSON)
    [SugarColumn(ColumnDataType = "TEXT")]
    public string CustomCss { get; set; }
    
    // Theme Configuration (JSON)
    [SugarColumn(ColumnDataType = "TEXT")]
    public string ThemeConfig { get; set; }
    
    [SugarColumn(Length = 200)]
    public string PreviewImage { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    [Navigate(NavigateType.OneToMany, nameof(UserTheme.ThemeId))]
    public List<UserTheme> UserThemes { get; set; }
}
```

**UserTheme.cs**
```csharp
[SugarTable("user_themes")]
public class UserTheme : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long UserThemeId { get; set; }
    
    public long UserId { get; set; }
    
    public long? ThemeId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Custom Overrides (JSON)
    [SugarColumn(ColumnDataType = "TEXT")]
    public string CustomColors { get; set; }
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string CustomLayout { get; set; }
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string CustomCss { get; set; }
    
    // Layout Preferences
    public bool SidebarCollapsed { get; set; } = false;
    
    [SugarColumn(Length = 20)]
    public string SidebarPosition { get; set; } = "left"; // left, right
    
    [SugarColumn(Length = 20)]
    public string HeaderStyle { get; set; } = "fixed"; // fixed, static
    
    [SugarColumn(Length = 20)]
    public string LayoutMode { get; set; } = "default"; // default, compact, comfortable
    
    public bool ShowBreadcrumb { get; set; } = true;
    
    public bool ShowTabs { get; set; } = true;
    
    public bool ShowFooter { get; set; } = true;
    
    // Dashboard Preferences
    [SugarColumn(ColumnDataType = "TEXT")]
    public string DashboardLayout { get; set; }
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string WidgetPreferences { get; set; }
    
    public DateTime LastApplied { get; set; } = DateTime.UtcNow;
    
    [Navigate(NavigateType.OneToOne, nameof(ThemeId))]
    public Theme Theme { get; set; }
}
```

**UserPreferences.cs**
```csharp
[SugarTable("user_preferences")]
public class UserPreferences : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long PreferenceId { get; set; }
    
    public long UserId { get; set; }
    
    // General Preferences
    [SugarColumn(Length = 10)]
    public string Language { get; set; } = "en";
    
    [SugarColumn(Length = 50)]
    public string Timezone { get; set; } = "UTC";
    
    [SugarColumn(Length = 20)]
    public string DateFormat { get; set; } = "YYYY-MM-DD";
    
    [SugarColumn(Length = 20)]
    public string TimeFormat { get; set; } = "HH:mm:ss";
    
    [SugarColumn(Length = 10)]
    public string Currency { get; set; } = "USD";
    
    // UI Preferences
    public bool DarkMode { get; set; } = false;
    
    public bool ReducedMotion { get; set; } = false;
    
    public bool HighContrast { get; set; } = false;
    
    [SugarColumn(Length = 20)]
    public string FontSize { get; set; } = "medium"; // small, medium, large
    
    // Notification Preferences
    public bool EmailNotifications { get; set; } = true;
    
    public bool PushNotifications { get; set; } = true;
    
    public bool SoundNotifications { get; set; } = true;
    
    // Dashboard Preferences
    public bool AutoRefresh { get; set; } = true;
    
    public int RefreshInterval { get; set; } = 300; // seconds
    
    public bool ShowWelcomeMessage { get; set; } = true;
    
    public bool ShowQuickActions { get; set; } = true;
    
    // Table Preferences
    public int DefaultPageSize { get; set; } = 20;
    
    public bool ShowRowNumbers { get; set; } = false;
    
    public bool DenseTable { get; set; } = false;
    
    // Custom Preferences (JSON)
    [SugarColumn(ColumnDataType = "TEXT")]
    public string CustomPreferences { get; set; }
}
```

## 2. Frontend Implementation

### Theme Control Vue Component

**views/system/ThemeControl.vue**
```vue
<template>
  <div class="theme-control">
    <!-- Theme Control Header -->
    <div class="control-header">
      <h1>{{ $t('theme.control.title') }}</h1>
      <div class="header-actions">
        <el-button type="primary" @click="showCreateThemeDialog = true">
          <i class="el-icon-plus"></i>
          {{ $t('theme.actions.createTheme') }}
        </el-button>
        <el-button @click="resetToDefault">
          <i class="el-icon-refresh"></i>
          {{ $t('theme.actions.resetDefault') }}
        </el-button>
      </div>
    </div>

    <!-- Theme Tabs -->
    <el-tabs v-model="activeTab" class="theme-tabs">
      <!-- Theme Selection -->
      <el-tab-pane :label="$t('theme.tabs.themes')" name="themes">
        <div class="themes-section">
          <div class="section-header">
            <h3>{{ $t('theme.sections.availableThemes') }}</h3>
            <div class="theme-filters">
              <el-select v-model="themeFilter" @change="filterThemes">
                <el-option :label="$t('theme.filters.all')" value="all" />
                <el-option :label="$t('theme.filters.system')" value="system" />
                <el-option :label="$t('theme.filters.custom')" value="custom" />
                <el-option :label="$t('theme.filters.dark')" value="dark" />
                <el-option :label="$t('theme.filters.light')" value="light" />
              </el-select>
            </div>
          </div>
          
          <div class="themes-grid">
            <ThemeCard
              v-for="theme in filteredThemes"
              :key="theme.themeId"
              :theme="theme"
              :is-active="currentTheme?.themeId === theme.themeId"
              @apply="applyTheme"
              @edit="editTheme"
              @delete="deleteTheme"
              @preview="previewTheme"
            />
          </div>
        </div>
      </el-tab-pane>

      <!-- Color Customization -->
      <el-tab-pane :label="$t('theme.tabs.colors')" name="colors">
        <div class="colors-section">
          <div class="color-categories">
            <!-- Primary Colors -->
            <div class="color-category">
              <h3>{{ $t('theme.colors.primary') }}</h3>
              <div class="color-group">
                <ColorPicker
                  v-model="customColors.primaryColor"
                  :label="$t('theme.colors.primaryMain')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.primaryLightColor"
                  :label="$t('theme.colors.primaryLight')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.primaryDarkColor"
                  :label="$t('theme.colors.primaryDark')"
                  @change="updateCustomColors"
                />
              </div>
            </div>

            <!-- Secondary Colors -->
            <div class="color-category">
              <h3>{{ $t('theme.colors.secondary') }}</h3>
              <div class="color-group">
                <ColorPicker
                  v-model="customColors.secondaryColor"
                  :label="$t('theme.colors.secondaryMain')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.secondaryLightColor"
                  :label="$t('theme.colors.secondaryLight')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.secondaryDarkColor"
                  :label="$t('theme.colors.secondaryDark')"
                  @change="updateCustomColors"
                />
              </div>
            </div>

            <!-- Status Colors -->
            <div class="color-category">
              <h3>{{ $t('theme.colors.status') }}</h3>
              <div class="color-group">
                <ColorPicker
                  v-model="customColors.successColor"
                  :label="$t('theme.colors.success')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.warningColor"
                  :label="$t('theme.colors.warning')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.dangerColor"
                  :label="$t('theme.colors.danger')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.infoColor"
                  :label="$t('theme.colors.info')"
                  @change="updateCustomColors"
                />
              </div>
            </div>

            <!-- Background Colors -->
            <div class="color-category">
              <h3>{{ $t('theme.colors.background') }}</h3>
              <div class="color-group">
                <ColorPicker
                  v-model="customColors.backgroundColor"
                  :label="$t('theme.colors.backgroundMain')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.surfaceColor"
                  :label="$t('theme.colors.surface')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.cardColor"
                  :label="$t('theme.colors.card')"
                  @change="updateCustomColors"
                />
              </div>
            </div>

            <!-- Text Colors -->
            <div class="color-category">
              <h3>{{ $t('theme.colors.text') }}</h3>
              <div class="color-group">
                <ColorPicker
                  v-model="customColors.textPrimaryColor"
                  :label="$t('theme.colors.textPrimary')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.textSecondaryColor"
                  :label="$t('theme.colors.textSecondary')"
                  @change="updateCustomColors"
                />
                <ColorPicker
                  v-model="customColors.textPlaceholderColor"
                  :label="$t('theme.colors.textPlaceholder')"
                  @change="updateCustomColors"
                />
              </div>
            </div>
          </div>

          <!-- Color Palette Generator -->
          <div class="palette-generator">
            <h3>{{ $t('theme.generator.title') }}</h3>
            <div class="generator-controls">
              <el-input
                v-model="baseColor"
                :placeholder="$t('theme.generator.baseColor')"
                class="color-input"
              >
                <template #append>
                  <el-color-picker v-model="baseColor" />
                </template>
              </el-input>
              <el-button type="primary" @click="generatePalette">
                {{ $t('theme.generator.generate') }}
              </el-button>
            </div>
            <div v-if="generatedPalette" class="generated-palette">
              <div
                v-for="(color, index) in generatedPalette"
                :key="index"
                class="palette-color"
                :style="{ backgroundColor: color }"
                @click="selectPaletteColor(color)"
              >
                {{ color }}
              </div>
            </div>
          </div>
        </div>
      </el-tab-pane>

      <!-- Layout Settings -->
      <el-tab-pane :label="$t('theme.tabs.layout')" name="layout">
        <div class="layout-section">
          <el-row :gutter="20">
            <el-col :span="12">
              <div class="layout-group">
                <h3>{{ $t('theme.layout.sidebar') }}</h3>
                <el-form :model="layoutSettings" label-width="120px">
                  <el-form-item :label="$t('theme.layout.sidebarWidth')">
                    <el-slider
                      v-model="layoutSettings.sidebarWidth"
                      :min="200"
                      :max="400"
                      :step="10"
                      show-input
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.sidebarPosition')">
                    <el-radio-group v-model="layoutSettings.sidebarPosition" @change="updateLayoutSettings">
                      <el-radio label="left">{{ $t('theme.layout.left') }}</el-radio>
                      <el-radio label="right">{{ $t('theme.layout.right') }}</el-radio>
                    </el-radio-group>
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.collapsible')">
                    <el-switch
                      v-model="layoutSettings.sidebarCollapsible"
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                </el-form>
              </div>
            </el-col>
            <el-col :span="12">
              <div class="layout-group">
                <h3>{{ $t('theme.layout.header') }}</h3>
                <el-form :model="layoutSettings" label-width="120px">
                  <el-form-item :label="$t('theme.layout.headerHeight')">
                    <el-slider
                      v-model="layoutSettings.headerHeight"
                      :min="50"
                      :max="100"
                      :step="5"
                      show-input
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.headerStyle')">
                    <el-radio-group v-model="layoutSettings.headerStyle" @change="updateLayoutSettings">
                      <el-radio label="fixed">{{ $t('theme.layout.fixed') }}</el-radio>
                      <el-radio label="static">{{ $t('theme.layout.static') }}</el-radio>
                    </el-radio-group>
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.showBreadcrumb')">
                    <el-switch
                      v-model="layoutSettings.showBreadcrumb"
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                </el-form>
              </div>
            </el-col>
          </el-row>

          <el-row :gutter="20" style="margin-top: 20px;">
            <el-col :span="12">
              <div class="layout-group">
                <h3>{{ $t('theme.layout.general') }}</h3>
                <el-form :model="layoutSettings" label-width="120px">
                  <el-form-item :label="$t('theme.layout.layoutMode')">
                    <el-select v-model="layoutSettings.layoutMode" @change="updateLayoutSettings">
                      <el-option label="Default" value="default" />
                      <el-option label="Compact" value="compact" />
                      <el-option label="Comfortable" value="comfortable" />
                    </el-select>
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.borderRadius')">
                    <el-slider
                      v-model="layoutSettings.borderRadius"
                      :min="0"
                      :max="20"
                      :step="1"
                      show-input
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.showTabs')">
                    <el-switch
                      v-model="layoutSettings.showTabs"
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                </el-form>
              </div>
            </el-col>
            <el-col :span="12">
              <div class="layout-group">
                <h3>{{ $t('theme.layout.typography') }}</h3>
                <el-form :model="layoutSettings" label-width="120px">
                  <el-form-item :label="$t('theme.layout.fontFamily')">
                    <el-select v-model="layoutSettings.fontFamily" @change="updateLayoutSettings">
                      <el-option label="System Default" value="system" />
                      <el-option label="Helvetica" value="helvetica" />
                      <el-option label="Arial" value="arial" />
                      <el-option label="Roboto" value="roboto" />
                    </el-select>
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.fontSize')">
                    <el-radio-group v-model="layoutSettings.fontSize" @change="updateLayoutSettings">
                      <el-radio label="small">{{ $t('theme.layout.small') }}</el-radio>
                      <el-radio label="medium">{{ $t('theme.layout.medium') }}</el-radio>
                      <el-radio label="large">{{ $t('theme.layout.large') }}</el-radio>
                    </el-radio-group>
                  </el-form-item>
                  <el-form-item :label="$t('theme.layout.lineHeight')">
                    <el-slider
                      v-model="layoutSettings.lineHeight"
                      :min="1.2"
                      :max="2.0"
                      :step="0.1"
                      show-input
                      @change="updateLayoutSettings"
                    />
                  </el-form-item>
                </el-form>
              </div>
            </el-col>
          </el-row>
        </div>
      </el-tab-pane>

      <!-- Advanced Settings -->
      <el-tab-pane :label="$t('theme.tabs.advanced')" name="advanced">
        <div class="advanced-section">
          <el-row :gutter="20">
            <el-col :span="12">
              <div class="advanced-group">
                <h3>{{ $t('theme.advanced.accessibility') }}</h3>
                <el-form :model="advancedSettings" label-width="150px">
                  <el-form-item :label="$t('theme.advanced.highContrast')">
                    <el-switch
                      v-model="advancedSettings.highContrast"
                      @change="updateAdvancedSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.advanced.reducedMotion')">
                    <el-switch
                      v-model="advancedSettings.reducedMotion"
                      @change="updateAdvancedSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.advanced.focusIndicator')">
                    <el-switch
                      v-model="advancedSettings.focusIndicator"
                      @change="updateAdvancedSettings"
                    />
                  </el-form-item>
                </el-form>
              </div>
            </el-col>
            <el-col :span="12">
              <div class="advanced-group">
                <h3>{{ $t('theme.advanced.performance') }}</h3>
                <el-form :model="advancedSettings" label-width="150px">
                  <el-form-item :label="$t('theme.advanced.animations')">
                    <el-switch
                      v-model="advancedSettings.animations"
                      @change="updateAdvancedSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.advanced.transitions')">
                    <el-switch
                      v-model="advancedSettings.transitions"
                      @change="updateAdvancedSettings"
                    />
                  </el-form-item>
                  <el-form-item :label="$t('theme.advanced.shadows')">
                    <el-switch
                      v-model="advancedSettings.shadows"
                      @change="updateAdvancedSettings"
                    />
                  </el-form-item>
                </el-form>
              </div>
            </el-col>
          </el-row>

          <!-- Custom CSS -->
          <div class="custom-css-section">
            <h3>{{ $t('theme.advanced.customCss') }}</h3>
            <div class="css-editor">
              <el-input
                v-model="customCss"
                type="textarea"
                :rows="10"
                :placeholder="$t('theme.advanced.cssPlaceholder')"
                @blur="updateCustomCss"
              />
            </div>
            <div class="css-actions">
              <el-button @click="validateCss">{{ $t('theme.advanced.validateCss') }}</el-button>
              <el-button @click="previewCss">{{ $t('theme.advanced.previewCss') }}</el-button>
              <el-button @click="resetCss">{{ $t('theme.advanced.resetCss') }}</el-button>
            </div>
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>

    <!-- Theme Preview -->
    <div class="theme-preview">
      <div class="preview-header">
        <h3>{{ $t('theme.preview.title') }}</h3>
        <div class="preview-actions">
          <el-button @click="refreshPreview">
            <i class="el-icon-refresh"></i>
            {{ $t('theme.preview.refresh') }}
          </el-button>
          <el-button type="primary" @click="saveTheme">
            <i class="el-icon-check"></i>
            {{ $t('theme.actions.saveTheme') }}
          </el-button>
        </div>
      </div>
      <div class="preview-content">
        <ThemePreview :theme-data="previewThemeData" />
      </div>
    </div>

    <!-- Create Theme Dialog -->
    <CreateThemeDialog
      v-model="showCreateThemeDialog"
      @created="handleThemeCreated"
    />

    <!-- Edit Theme Dialog -->
    <EditThemeDialog
      v-model="showEditThemeDialog"
      :theme="editingTheme"
      @updated="handleThemeUpdated"
    />
  </div>
</template>

<script>
import { ref, reactive, onMounted, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  getThemes, 
  getUserTheme, 
  saveUserTheme, 
  applyTheme as applyThemeApi,
  deleteTheme as deleteThemeApi,
  generateTheme,
  previewTheme as previewThemeApi
} from '@/api/system/theme'
import ThemeCard from '@/components/Theme/ThemeCard.vue'
import ColorPicker from '@/components/Theme/ColorPicker.vue'
import ThemePreview from '@/components/Theme/ThemePreview.vue'
import CreateThemeDialog from '@/components/Theme/CreateThemeDialog.vue'
import EditThemeDialog from '@/components/Theme/EditThemeDialog.vue'
import { useThemeStore } from '@/stores/theme'

export default {
  name: 'ThemeControl',
  components: {
    ThemeCard,
    ColorPicker,
    ThemePreview,
    CreateThemeDialog,
    EditThemeDialog
  },
  setup() {
    const { t } = useI18n()
    const themeStore = useThemeStore()
    
    const loading = ref(false)
    const activeTab = ref('themes')
    const themeFilter = ref('all')
    const showCreateThemeDialog = ref(false)
    const showEditThemeDialog = ref(false)
    const editingTheme = ref(null)
    
    const themes = ref([])
    const currentTheme = ref(null)
    const baseColor = ref('#409EFF')
    const generatedPalette = ref(null)
    const customCss = ref('')
    
    const customColors = reactive({
      primaryColor: '#409EFF',
      primaryLightColor: '#66B2FF',
      primaryDarkColor: '#337ECC',
      secondaryColor: '#909399',
      secondaryLightColor: '#B3B6BD',
      secondaryDarkColor: '#73767A',
      successColor: '#67C23A',
      warningColor: '#E6A23C',
      dangerColor: '#F56C6C',
      infoColor: '#909399',
      backgroundColor: '#FFFFFF',
      surfaceColor: '#F5F7FA',
      cardColor: '#FFFFFF',
      textPrimaryColor: '#303133',
      textSecondaryColor: '#606266',
      textPlaceholderColor: '#C0C4CC'
    })
    
    const layoutSettings = reactive({
      sidebarWidth: 250,
      sidebarPosition: 'left',
      sidebarCollapsible: true,
      headerHeight: 60,
      headerStyle: 'fixed',
      showBreadcrumb: true,
      layoutMode: 'default',
      borderRadius: 4,
      showTabs: true,
      fontFamily: 'system',
      fontSize: 'medium',
      lineHeight: 1.5
    })
    
    const advancedSettings = reactive({
      highContrast: false,
      reducedMotion: false,
      focusIndicator: true,
      animations: true,
      transitions: true,
      shadows: true
    })
    
    const filteredThemes = computed(() => {
      if (themeFilter.value === 'all') return themes.value
      
      return themes.value.filter(theme => {
        switch (themeFilter.value) {
          case 'system':
            return theme.isSystemTheme
          case 'custom':
            return !theme.isSystemTheme
          case 'dark':
            return theme.isDarkMode
          case 'light':
            return !theme.isDarkMode
          default:
            return true
        }
      })
    })
    
    const previewThemeData = computed(() => ({
      colors: customColors,
      layout: layoutSettings,
      advanced: advancedSettings,
      customCss: customCss.value
    }))
    
    const loadThemes = async () => {
      try {
        const response = await getThemes()
        themes.value = response.data
      } catch (error) {
        ElMessage.error(t('theme.messages.loadError'))
      }
    }
    
    const loadUserTheme = async () => {
      try {
        const response = await getUserTheme()
        if (response.data) {
          currentTheme.value = response.data.theme
          if (response.data.customColors) {
            Object.assign(customColors, JSON.parse(response.data.customColors))
          }
          if (response.data.customLayout) {
            Object.assign(layoutSettings, JSON.parse(response.data.customLayout))
          }
        }
      } catch (error) {
        console.error('Load user theme error:', error)
      }
    }
    
    const applyTheme = async (theme) => {
      try {
        await applyThemeApi(theme.themeId)
        currentTheme.value = theme
        themeStore.setTheme(theme)
        ElMessage.success(t('theme.messages.applySuccess'))
      } catch (error) {
        ElMessage.error(t('theme.messages.applyError'))
      }
    }
    
    const editTheme = (theme) => {
      editingTheme.value = theme
      showEditThemeDialog.value = true
    }
    
    const deleteTheme = async (theme) => {
      try {
        await ElMessageBox.confirm(
          t('theme.messages.deleteConfirm'),
          t('common.warning'),
          {
            confirmButtonText: t('common.confirm'),
            cancelButtonText: t('common.cancel'),
            type: 'warning'
          }
        )
        
        await deleteThemeApi(theme.themeId)
        await loadThemes()
        ElMessage.success(t('theme.messages.deleteSuccess'))
      } catch (error) {
        if (error !== 'cancel') {
          ElMessage.error(t('theme.messages.deleteError'))
        }
      }
    }
    
    const previewTheme = async (theme) => {
      try {
        const response = await previewThemeApi(theme)
        // Apply preview styles temporarily
        themeStore.previewTheme(response.data)
      } catch (error) {
        ElMessage.error(t('theme.messages.previewError'))
      }
    }
    
    const generatePalette = async () => {
      try {
        const response = await generateTheme({ baseColor: baseColor.value })
        generatedPalette.value = response.data.palette
      } catch (error) {
        ElMessage.error(t('theme.messages.generateError'))
      }
    }
    
    const selectPaletteColor = (color) => {
      customColors.primaryColor = color
      updateCustomColors()
    }
    
    const updateCustomColors = () => {
      themeStore.updateColors(customColors)
    }
    
    const updateLayoutSettings = () => {
      themeStore.updateLayout(layoutSettings)
    }
    
    const updateAdvancedSettings = () => {
      themeStore.updateAdvanced(advancedSettings)
    }
    
    const updateCustomCss = () => {
      themeStore.updateCustomCss(customCss.value)
    }
    
    const validateCss = () => {
      // CSS validation logic
      try {
        // Simple CSS validation
        const style = document.createElement('style')
        style.textContent = customCss.value
        document.head.appendChild(style)
        document.head.removeChild(style)
        ElMessage.success(t('theme.messages.cssValid'))
      } catch (error) {
        ElMessage.error(t('theme.messages.cssInvalid'))
      }
    }
    
    const previewCss = () => {
      themeStore.previewCustomCss(customCss.value)
    }
    
    const resetCss = () => {
      customCss.value = ''
      updateCustomCss()
    }
    
    const refreshPreview = () => {
      themeStore.refreshPreview()
    }
    
    const saveTheme = async () => {
      try {
        const themeData = {
          customColors: JSON.stringify(customColors),
          customLayout: JSON.stringify(layoutSettings),
          customCss: customCss.value
        }
        
        await saveUserTheme(themeData)
        ElMessage.success(t('theme.messages.saveSuccess'))
      } catch (error) {
        ElMessage.error(t('theme.messages.saveError'))
      }
    }
    
    const resetToDefault = async () => {
      try {
        await ElMessageBox.confirm(
          t('theme.messages.resetConfirm'),
          t('common.warning'),
          {
            confirmButtonText: t('common.confirm'),
            cancelButtonText: t('common.cancel'),
            type: 'warning'
          }
        )
        
        await themeStore.resetToDefault()
        await loadUserTheme()
        ElMessage.success(t('theme.messages.resetSuccess'))
      } catch (error) {
        if (error !== 'cancel') {
          ElMessage.error(t('theme.messages.resetError'))
        }
      }
    }
    
    const filterThemes = () => {
      // Filter logic is handled by computed property
    }
    
    const handleThemeCreated = (theme) => {
      themes.value.push(theme)
      showCreateThemeDialog.value = false
      ElMessage.success(t('theme.messages.createSuccess'))
    }
    
    const handleThemeUpdated = (theme) => {
      const index = themes.value.findIndex(t => t.themeId === theme.themeId)
      if (index !== -1) {
        themes.value[index] = theme
      }
      showEditThemeDialog.value = false
      ElMessage.success(t('theme.messages.updateSuccess'))
    }
    
    // Watch for changes and apply them in real-time
    watch(customColors, updateCustomColors, { deep: true })
    watch(layoutSettings, updateLayoutSettings, { deep: true })
    watch(advancedSettings, updateAdvancedSettings, { deep: true })
    
    onMounted(() => {
      loadThemes()
      loadUserTheme()
    })
    
    return {
      loading,
      activeTab,
      themeFilter,
      showCreateThemeDialog,
      showEditThemeDialog,
      editingTheme,
      themes,
      filteredThemes,
      currentTheme,
      baseColor,
      generatedPalette,
      customColors,
      layoutSettings,
      advancedSettings,
      customCss,
      previewThemeData,
      applyTheme,
      editTheme,
      deleteTheme,
      previewTheme,
      generatePalette,
      selectPaletteColor,
      updateCustomColors,
      updateLayoutSettings,
      updateAdvancedSettings,
      updateCustomCss,
      validateCss,
      previewCss,
      resetCss,
      refreshPreview,
      saveTheme,
      resetToDefault,
      filterThemes,
      handleThemeCreated,
      handleThemeUpdated
    }
  }
}
</script>

<style lang="scss" scoped>
.theme-control {
  padding: 20px;
  
  .control-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    
    h1 {
      margin: 0;
      color: var(--el-text-color-primary);
    }
    
    .header-actions {
      display: flex;
      gap: 10px;
    }
  }
  
  .theme-tabs {
    .themes-section {
      .section-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 20px;
        
        h3 {
          margin: 0;
        }
      }
      
      .themes-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
        gap: 20px;
      }
    }
    
    .colors-section {
      .color-categories {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: 20px;
        margin-bottom: 30px;
        
        .color-category {
          padding: 20px;
          border: 1px solid var(--el-border-color);
          border-radius: 8px;
          
          h3 {
            margin: 0 0 15px 0;
            color: var(--el-text-color-primary);
          }
          
          .color-group {
            display: flex;
            flex-direction: column;
            gap: 10px;
          }
        }
      }
      
      .palette-generator {
        padding: 20px;
        border: 1px solid var(--el-border-color);
        border-radius: 8px;
        
        h3 {
          margin: 0 0 15px 0;
        }
        
        .generator-controls {
          display: flex;
          gap: 10px;
          margin-bottom: 15px;
          
          .color-input {
            width: 200px;
          }
        }
        
        .generated-palette {
          display: flex;
          gap: 10px;
          flex-wrap: wrap;
          
          .palette-color {
            width: 80px;
            height: 40px;
            border-radius: 4px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 12px;
            font-weight: bold;
            text-shadow: 1px 1px 1px rgba(0, 0, 0, 0.5);
            transition: transform 0.2s;
            
            &:hover {
              transform: scale(1.1);
            }
          }
        }
      }
    }
    
    .layout-section {
      .layout-group {
        padding: 20px;
        border: 1px solid var(--el-border-color);
        border-radius: 8px;
        
        h3 {
          margin: 0 0 15px 0;
          color: var(--el-text-color-primary);
        }
      }
    }
    
    .advanced-section {
      .advanced-group {
        padding: 20px;
        border: 1px solid var(--el-border-color);
        border-radius: 8px;
        
        h3 {
          margin: 0 0 15px 0;
          color: var(--el-text-color-primary);
        }
      }
      
      .custom-css-section {
        margin-top: 20px;
        padding: 20px;
        border: 1px solid var(--el-border-color);
        border-radius: 8px;
        
        h3 {
          margin: 0 0 15px 0;
        }
        
        .css-editor {
          margin-bottom: 15px;
        }
        
        .css-actions {
          display: flex;
          gap: 10px;
        }
      }
    }
  }
  
  .theme-preview {
    margin-top: 30px;
    padding: 20px;
    border: 1px solid var(--el-border-color);
    border-radius: 8px;
    
    .preview-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      
      h3 {
        margin: 0;
      }
      
      .preview-actions {
        display: flex;
        gap: 10px;
      }
    }
    
    .preview-content {
      min-height: 300px;
      border: 1px solid var(--el-border-color-light);
      border-radius: 4px;
    }
  }
}
</style>
```

### Supporting Components

**components/Theme/ThemeCard.vue**
```vue
<template>
  <div class="theme-card" :class="{ active: isActive }">
    <div class="theme-preview" :style="previewStyle">
      <div class="preview-header" :style="headerStyle">
        <div class="preview-title">{{ theme.themeName }}</div>
      </div>
      <div class="preview-content">
        <div class="preview-sidebar" :style="sidebarStyle"></div>
        <div class="preview-main" :style="mainStyle">
          <div class="preview-card" :style="cardStyle"></div>
          <div class="preview-card" :style="cardStyle"></div>
        </div>
      </div>
    </div>
    
    <div class="theme-info">
      <h4>{{ theme.themeName }}</h4>
      <p>{{ theme.description }}</p>
      <div class="theme-meta">
        <el-tag v-if="theme.isSystemTheme" type="info" size="small">System</el-tag>
        <el-tag v-if="theme.isDarkMode" type="warning" size="small">Dark</el-tag>
        <el-tag v-else type="success" size="small">Light</el-tag>
      </div>
    </div>
    
    <div class="theme-actions">
      <el-button size="small" @click="$emit('preview', theme)">
        {{ $t('theme.actions.preview') }}
      </el-button>
      <el-button 
        type="primary" 
        size="small" 
        :disabled="isActive"
        @click="$emit('apply', theme)"
      >
        {{ isActive ? $t('theme.actions.active') : $t('theme.actions.apply') }}
      </el-button>
      <el-dropdown v-if="!theme.isSystemTheme" @command="handleCommand">
        <el-button size="small" type="text">
          <i class="el-icon-more"></i>
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="edit">
              <i class="el-icon-edit"></i>
              {{ $t('theme.actions.edit') }}
            </el-dropdown-item>
            <el-dropdown-item command="delete" divided>
              <i class="el-icon-delete"></i>
              {{ $t('theme.actions.delete') }}
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </div>
</template>

<script>
import { computed } from 'vue'

export default {
  name: 'ThemeCard',
  props: {
    theme: {
      type: Object,
      required: true
    },
    isActive: {
      type: Boolean,
      default: false
    }
  },
  emits: ['apply', 'edit', 'delete', 'preview'],
  setup(props, { emit }) {
    const previewStyle = computed(() => ({
      backgroundColor: props.theme.backgroundColor,
      color: props.theme.textPrimaryColor
    }))
    
    const headerStyle = computed(() => ({
      backgroundColor: props.theme.primaryColor,
      color: '#ffffff'
    }))
    
    const sidebarStyle = computed(() => ({
      backgroundColor: props.theme.surfaceColor
    }))
    
    const mainStyle = computed(() => ({
      backgroundColor: props.theme.backgroundColor
    }))
    
    const cardStyle = computed(() => ({
      backgroundColor: props.theme.cardColor,
      borderColor: props.theme.borderColor
    }))
    
    const handleCommand = (command) => {
      if (command === 'edit') {
        emit('edit', props.theme)
      } else if (command === 'delete') {
        emit('delete', props.theme)
      }
    }
    
    return {
      previewStyle,
      headerStyle,
      sidebarStyle,
      mainStyle,
      cardStyle,
      handleCommand
    }
  }
}
</script>

<style lang="scss" scoped>
.theme-card {
  border: 2px solid var(--el-border-color);
  border-radius: 8px;
  overflow: hidden;
  transition: all 0.3s;
  
  &:hover {
    border-color: var(--el-color-primary);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
  
  &.active {
    border-color: var(--el-color-primary);
    box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.2);
  }
  
  .theme-preview {
    height: 120px;
    position: relative;
    
    .preview-header {
      height: 30px;
      display: flex;
      align-items: center;
      padding: 0 10px;
      
      .preview-title {
        font-size: 12px;
        font-weight: bold;
      }
    }
    
    .preview-content {
      height: 90px;
      display: flex;
      
      .preview-sidebar {
        width: 40px;
        height: 100%;
      }
      
      .preview-main {
        flex: 1;
        padding: 8px;
        display: flex;
        flex-direction: column;
        gap: 4px;
        
        .preview-card {
          height: 20px;
          border: 1px solid;
          border-radius: 2px;
        }
      }
    }
  }
  
  .theme-info {
    padding: 15px;
    
    h4 {
      margin: 0 0 5px 0;
      font-size: 16px;
    }
    
    p {
      margin: 0 0 10px 0;
      font-size: 12px;
      color: var(--el-text-color-secondary);
      line-height: 1.4;
    }
    
    .theme-meta {
      display: flex;
      gap: 5px;
    }
  }
  
  .theme-actions {
    padding: 0 15px 15px 15px;
    display: flex;
    gap: 8px;
    align-items: center;
  }
}
</style>
```

## 3. Database Schema

### Theme Tables

```sql
-- System Themes Table
CREATE TABLE system_themes (
    theme_id BIGINT PRIMARY KEY IDENTITY(1,1),
    theme_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    theme_type NVARCHAR(20) DEFAULT 'Custom',
    is_system_theme BIT DEFAULT 0,
    is_active BIT DEFAULT 1,
    is_dark_mode BIT DEFAULT 0,
    
    -- Primary Colors
    primary_color NVARCHAR(10) DEFAULT '#409EFF',
    primary_light_color NVARCHAR(10) DEFAULT '#66B2FF',
    primary_dark_color NVARCHAR(10) DEFAULT '#337ECC',
    
    -- Secondary Colors
    secondary_color NVARCHAR(10) DEFAULT '#909399',
    secondary_light_color NVARCHAR(10) DEFAULT '#B3B6BD',
    secondary_dark_color NVARCHAR(10) DEFAULT '#73767A',
    
    -- Status Colors
    success_color NVARCHAR(10) DEFAULT '#67C23A',
    warning_color NVARCHAR(10) DEFAULT '#E6A23C',
    danger_color NVARCHAR(10) DEFAULT '#F56C6C',
    info_color NVARCHAR(10) DEFAULT '#909399',
    
    -- Background Colors
    background_color NVARCHAR(10) DEFAULT '#FFFFFF',
    surface_color NVARCHAR(10) DEFAULT '#F5F7FA',
    card_color NVARCHAR(10) DEFAULT '#FFFFFF',
    
    -- Text Colors
    text_primary_color NVARCHAR(10) DEFAULT '#303133',
    text_secondary_color NVARCHAR(10) DEFAULT '#606266',
    text_placeholder_color NVARCHAR(10) DEFAULT '#C0C4CC',
    
    -- Border Colors
    border_color NVARCHAR(10) DEFAULT '#DCDFE6',
    border_light_color NVARCHAR(10) DEFAULT '#E4E7ED',
    border_dark_color NVARCHAR(10) DEFAULT '#D3D4D6',
    
    -- Layout Settings
    sidebar_width NVARCHAR(20) DEFAULT '250px',
    header_height NVARCHAR(20) DEFAULT '60px',
    footer_height NVARCHAR(20) DEFAULT '40px',
    border_radius NVARCHAR(20) DEFAULT '4px',
    box_shadow NVARCHAR(100) DEFAULT '0 2px 12px 0 rgba(0, 0, 0, 0.1)',
    
    -- Typography
    font_family NVARCHAR(200) DEFAULT '''Helvetica Neue'', Helvetica, ''PingFang SC'', ''Hiragino Sans GB'', ''Microsoft YaHei'', Arial, sans-serif',
    font_size_base NVARCHAR(20) DEFAULT '14px',
    font_size_small NVARCHAR(20) DEFAULT '12px',
    font_size_large NVARCHAR(20) DEFAULT '16px',
    line_height NVARCHAR(20) DEFAULT '1.5',
    
    -- Custom Data
    css_variables NTEXT,
    custom_css NTEXT,
    theme_config NTEXT,
    preview_image NVARCHAR(200),
    
    sort_order INT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETUTCDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    
    INDEX IX_system_themes_type (theme_type),
    INDEX IX_system_themes_active (is_active),
    INDEX IX_system_themes_system (is_system_theme)
);

-- User Themes Table
CREATE TABLE user_themes (
    user_theme_id BIGINT PRIMARY KEY IDENTITY(1,1),
    user_id BIGINT NOT NULL,
    theme_id BIGINT,
    is_active BIT DEFAULT 1,
    
    -- Custom Overrides
    custom_colors NTEXT,
    custom_layout NTEXT,
    custom_css NTEXT,
    
    -- Layout Preferences
    sidebar_collapsed BIT DEFAULT 0,
    sidebar_position NVARCHAR(20) DEFAULT 'left',
    header_style NVARCHAR(20) DEFAULT 'fixed',
    layout_mode NVARCHAR(20) DEFAULT 'default',
    show_breadcrumb BIT DEFAULT 1,
    show_tabs BIT DEFAULT 1,
    show_footer BIT DEFAULT 1,
    
    -- Dashboard Preferences
    dashboard_layout NTEXT,
    widget_preferences NTEXT,
    
    last_applied DATETIME2 DEFAULT GETUTCDATE(),
    created_at DATETIME2 DEFAULT GETUTCDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    
    FOREIGN KEY (theme_id) REFERENCES system_themes(theme_id),
    INDEX IX_user_themes_user (user_id),
    INDEX IX_user_themes_active (is_active)
);

-- User Preferences Table
CREATE TABLE user_preferences (
    preference_id BIGINT PRIMARY KEY IDENTITY(1,1),
    user_id BIGINT NOT NULL UNIQUE,
    
    -- General Preferences
    language NVARCHAR(10) DEFAULT 'en',
    timezone NVARCHAR(50) DEFAULT 'UTC',
    date_format NVARCHAR(20) DEFAULT 'YYYY-MM-DD',
    time_format NVARCHAR(20) DEFAULT 'HH:mm:ss',
    currency NVARCHAR(10) DEFAULT 'USD',
    
    -- UI Preferences
    dark_mode BIT DEFAULT 0,
    reduced_motion BIT DEFAULT 0,
    high_contrast BIT DEFAULT 0,
    font_size NVARCHAR(20) DEFAULT 'medium',
    
    -- Notification Preferences
    email_notifications BIT DEFAULT 1,
    push_notifications BIT DEFAULT 1,
    sound_notifications BIT DEFAULT 1,
    
    -- Dashboard Preferences
    auto_refresh BIT DEFAULT 1,
    refresh_interval INT DEFAULT 300,
    show_welcome_message BIT DEFAULT 1,
    show_quick_actions BIT DEFAULT 1,
    
    -- Table Preferences
    default_page_size INT DEFAULT 20,
    show_row_numbers BIT DEFAULT 0,
    dense_table BIT DEFAULT 0,
    
    -- Custom Preferences
    custom_preferences NTEXT,
    
    created_at DATETIME2 DEFAULT GETUTCDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    
    INDEX IX_user_preferences_user (user_id)
);

-- Theme Templates Table
CREATE TABLE theme_templates (
    template_id BIGINT PRIMARY KEY IDENTITY(1,1),
    template_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    category NVARCHAR(50),
    template_data NTEXT NOT NULL,
    preview_image NVARCHAR(200),
    is_active BIT DEFAULT 1,
    is_featured BIT DEFAULT 0,
    download_count INT DEFAULT 0,
    rating DECIMAL(3,2) DEFAULT 0,
    
    created_at DATETIME2 DEFAULT GETUTCDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    
    INDEX IX_theme_templates_category (category),
    INDEX IX_theme_templates_active (is_active),
    INDEX IX_theme_templates_featured (is_featured)
);
```

## 4. API Integration

### Theme API Endpoints

```javascript
// api/system/theme.js
import request from '@/utils/request'

// Theme Management
export function getThemes(params) {
  return request({
    url: '/api/theme/themes',
    method: 'get',
    params
  })
}

export function getTheme(id) {
  return request({
    url: `/api/theme/themes/${id}`,
    method: 'get'
  })
}

export function createTheme(data) {
  return request({
    url: '/api/theme/themes',
    method: 'post',
    data
  })
}

export function updateTheme(id, data) {
  return request({
    url: `/api/theme/themes/${id}`,
    method: 'put',
    data
  })
}

export function deleteTheme(id) {
  return request({
    url: `/api/theme/themes/${id}`,
    method: 'delete'
  })
}

// User Theme Management
export function getUserTheme() {
  return request({
    url: '/api/theme/user-theme',
    method: 'get'
  })
}

export function saveUserTheme(data) {
  return request({
    url: '/api/theme/user-theme',
    method: 'post',
    data
  })
}

export function applyTheme(themeId) {
  return request({
    url: `/api/theme/user-theme/apply/${themeId}`,
    method: 'post'
  })
}

export function resetUserTheme() {
  return request({
    url: '/api/theme/user-theme/reset',
    method: 'post'
  })
}

// Theme Templates
export function getThemeTemplates() {
  return request({
    url: '/api/theme/templates',
    method: 'get'
  })
}

export function getThemeTemplate(id) {
  return request({
    url: `/api/theme/templates/${id}`,
    method: 'get'
  })
}

export function createThemeTemplate(data) {
  return request({
    url: '/api/theme/templates',
    method: 'post',
    data
  })
}

// Color Palettes
export function getColorPalettes() {
  return request({
    url: '/api/theme/color-palettes',
    method: 'get'
  })
}

export function generateTheme(data) {
  return request({
    url: '/api/theme/generate-theme',
    method: 'post',
    data
  })
}

export function previewTheme(data) {
  return request({
    url: '/api/theme/preview-theme',
    method: 'post',
    data
  })
}

// CSS Variables
export function getThemeCssVariables(themeId) {
  return request({
    url: `/api/theme/css-variables/${themeId}`,
    method: 'get'
  })
}

// Import/Export
export function exportTheme(themeId) {
  return request({
    url: `/api/theme/export-theme/${themeId}`,
    method: 'post',
    responseType: 'blob'
  })
}

export function importTheme(data) {
  return request({
    url: '/api/theme/import-theme',
    method: 'post',
    data
  })
}

// User Preferences
export function getUserPreferences() {
  return request({
    url: '/api/userpreferences/preferences',
    method: 'get'
  })
}

export function saveUserPreferences(data) {
  return request({
    url: '/api/userpreferences/preferences',
    method: 'post',
    data
  })
}

export function updateLayoutPreferences(data) {
  return request({
    url: '/api/userpreferences/preferences/layout',
    method: 'put',
    data
  })
}

export function updateSidebarPreferences(data) {
  return request({
    url: '/api/userpreferences/preferences/sidebar',
    method: 'put',
    data
  })
}

export function updateDashboardPreferences(data) {
  return request({
    url: '/api/userpreferences/preferences/dashboard',
    method: 'put',
    data
  })
}

export function resetPreferences() {
  return request({
    url: '/api/userpreferences/preferences/reset',
    method: 'post'
  })
}
```

## 5. Implementation Timeline

### Phase 1: Foundation (Week 1-2)
- **Backend Setup**
  - Create database tables and migrations
  - Implement basic Theme and UserTheme models
  - Set up Theme and UserPreferences controllers
  - Create basic CRUD operations for themes

- **Frontend Setup**
  - Create theme store (Pinia/Vuex)
  - Implement basic theme switching functionality
  - Set up CSS variable system
  - Create basic theme selection UI

### Phase 2: Core Features (Week 3-4)
- **Theme Management**
  - Complete theme CRUD operations
  - Implement theme templates system
  - Add theme import/export functionality
  - Create theme preview system

- **User Customization**
  - Implement color customization interface
  - Add layout preference controls
  - Create user theme persistence
  - Implement real-time preview

### Phase 3: Advanced Features (Week 5-6)
- **Advanced Customization**
  - Add custom CSS editor
  - Implement color palette generator
  - Create accessibility options
  - Add typography controls

- **User Experience**
  - Implement theme validation
  - Add theme sharing capabilities
  - Create theme marketplace (optional)
  - Add theme analytics

### Phase 4: Polish & Testing (Week 7-8)
- **Testing & Optimization**
  - Unit and integration testing
  - Performance optimization
  - Cross-browser compatibility
  - Mobile responsiveness

- **Documentation & Deployment**
  - API documentation
  - User guides
  - Admin documentation
  - Production deployment

## 6. Key Features

### Core Theme Features
1. **Pre-built Themes**
   - Light and dark mode themes
   - Professional business themes
   - High contrast accessibility themes
   - Seasonal and branded themes

2. **Color Customization**
   - Primary and secondary color selection
   - Status color customization (success, warning, error, info)
   - Background and surface color controls
   - Text and border color adjustments
   - Color palette generator from base color

3. **Layout Controls**
   - Sidebar width and position
   - Header height and style
   - Content spacing and padding
   - Border radius and shadows
   - Typography settings

4. **Advanced Customization**
   - Custom CSS editor with validation
   - CSS variable override system
   - Component-specific styling
   - Responsive design controls
   - Animation and transition settings

### User Experience Features
1. **Real-time Preview**
   - Live theme preview as you customize
   - Component-level preview
   - Mobile and desktop preview modes
   - Before/after comparison

2. **Theme Management**
   - Save custom themes
   - Share themes with other users
   - Import/export theme configurations
   - Theme versioning and history

3. **Accessibility Support**
   - High contrast mode
   - Reduced motion preferences
   - Font size scaling
   - Color blindness friendly palettes

4. **User Preferences**
   - Personal layout preferences
   - Dashboard customization
   - Notification settings
   - Language and localization

### Administrative Features
1. **Theme Administration**
   - System theme management
   - User theme monitoring
   - Theme usage analytics
   - Performance impact tracking

2. **Template System**
   - Pre-designed theme templates
   - Template categories and tags
   - Template rating and reviews
   - Template marketplace (future)

## 7. Technical Considerations

### Performance Optimization
1. **CSS Loading Strategy**
   - Lazy load theme CSS
   - CSS-in-JS for dynamic themes
   - Critical CSS inlining
   - Theme caching mechanisms

2. **Bundle Optimization**
   - Tree-shake unused theme code
   - Dynamic imports for theme components
   - Minimize CSS bundle size
   - Optimize color calculations

### Security Considerations
1. **CSS Injection Prevention**
   - Sanitize custom CSS input
   - Whitelist allowed CSS properties
   - Validate color values
   - Prevent XSS through CSS

2. **User Data Protection**
   - Encrypt sensitive theme data
   - Validate theme configurations
   - Rate limit theme operations
   - Audit theme changes

### Scalability Features
1. **Multi-tenant Support**
   - Organization-level themes
   - Role-based theme access
   - Theme inheritance hierarchy
   - Bulk theme operations

2. **Integration Capabilities**
   - Theme API for external systems
   - Webhook notifications for theme changes
   - SSO integration for theme preferences
   - Mobile app theme synchronization

### Browser Compatibility
1. **CSS Variable Support**
   - Fallback for older browsers
   - Progressive enhancement
   - Polyfills for missing features
   - Cross-browser testing

2. **Responsive Design**
   - Mobile-first approach
   - Tablet-specific optimizations
   - Touch-friendly controls
   - Adaptive layouts

## 8. Success Metrics

### User Engagement
- Theme customization adoption rate
- Average time spent in theme editor
- Number of custom themes created
- Theme sharing and usage statistics

### Performance Metrics
- Theme loading time
- CSS bundle size impact
- Memory usage optimization
- Rendering performance

### User Satisfaction
- Theme customization satisfaction scores
- Support ticket reduction
- User retention improvement
- Accessibility compliance metrics

## 9. Future Enhancements

### Advanced Features
1. **AI-Powered Themes**
   - Automatic color palette generation
   - Smart theme recommendations
   - Accessibility optimization suggestions
   - Brand color extraction from logos

2. **Collaborative Features**
   - Team theme collaboration
   - Theme version control
   - Comment and review system
   - Theme approval workflows

3. **Integration Expansions**
   - Design system integration
   - Figma/Sketch plugin support
   - Brand guideline enforcement
   - Marketing campaign themes

### Technical Improvements
1. **Performance Enhancements**
   - WebAssembly color calculations
   - Service worker theme caching
   - Edge computing for theme delivery
   - Real-time collaboration

2. **Developer Experience**
   - Theme development SDK
   - Visual theme builder
   - Component library integration
   - Automated testing tools

This comprehensive Theme Control system will provide users with powerful customization capabilities while maintaining performance, security, and accessibility standards. The modular architecture allows for incremental implementation and future enhancements based on user feedback and requirements.