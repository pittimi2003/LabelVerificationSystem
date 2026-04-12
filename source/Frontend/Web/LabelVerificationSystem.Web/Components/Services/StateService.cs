using Microsoft.JSInterop;
using System.Text.Json;

// Make sure to clear the Session if stored. As the values are overrided by the session value automatially.
// If any Changes  to values thne the priority will move to the default values rather than the session.
public class AppState
{
    public string ColorTheme { get; set; } = "light";                   // light, dark
    public string Direction { get; set; } = "ltr";                      // ltr, rtl
    public string NavigationStyles { get; set; } = "vertical";          // vertical, horizontal   
    public string MenuStyles { get; set; } = "";                        // menu-click, menu-hover, icon-click, icon-hover
    public string LayoutStyles { get; set; } = "doublemenu";          // doublemenu, detached, icon-overlay, icontext-menu, closed-menu, default-menu 
    public string PageStyles { get; set; } = "flat";                 // regular, classic, modern, flat
    public string WidthStyles { get; set; } = "fullwidth";              // default, fullwidth, boxed
    public string MenuPosition { get; set; } = "fixed";                 // fixed, scrollable
    public string HeaderPosition { get; set; } = "fixed";               // fixed, scrollable
    public string MenuColor { get; set; } = "transparent";                     // light, dark, color, gradient, transparent
    public string HeaderColor { get; set; } = "transparent";                  // light, dark, color, gradient, transparent
    public string ThemePrimary { get; set; } = "";                      // '106, 91, 204', '100, 149, 237', '0, 123, 167', '10, 180, 255', '46, 81, 145'
    public string ThemeBackground { get; set; } = "";                   //make sure to add rgb valies like example :- '49, 63, 141' and also same for ThemeBackground1
    public string ThemeBackground1 { get; set; } = "";
    public string BackgroundImage { get; set; } = "";                   // bgimg1, bgimg2, bgimg3, bgimg4, bgimg5
    public MainMenuItems? currentItem { get; set; } = null;


    public bool IsDifferentFrom(AppState other)
    {
        return ColorTheme != other.ColorTheme ||
            Direction != other.Direction ||
            NavigationStyles != other.NavigationStyles ||
            MenuStyles != other.MenuStyles ||
            LayoutStyles != other.LayoutStyles ||
            PageStyles != other.PageStyles ||
            WidthStyles != other.WidthStyles ||
            MenuPosition != other.MenuPosition ||
            HeaderPosition != other.HeaderPosition ||
            MenuColor != other.MenuColor ||
            HeaderColor != other.HeaderColor ||
            ThemePrimary != other.ThemePrimary ||
            ThemeBackground != other.ThemeBackground ||
            ThemeBackground1 != other.ThemeBackground1 ||
            BackgroundImage != other.BackgroundImage ||
            (currentItem != null ? !currentItem.Equals(other.currentItem) : other.currentItem != null);
    }

    // Override Equals method to compare properties
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        AppState other = (AppState)obj;

        // Compare each property
        return ColorTheme == other.ColorTheme &&
               Direction == other.Direction &&
               NavigationStyles == other.NavigationStyles &&
               MenuStyles == other.MenuStyles &&
               LayoutStyles == other.LayoutStyles &&
               PageStyles == other.PageStyles &&
               WidthStyles == other.WidthStyles &&
               MenuPosition == other.MenuPosition &&
               HeaderPosition == other.HeaderPosition &&
               MenuColor == other.MenuColor &&
               HeaderColor == other.HeaderColor &&
               ThemePrimary == other.ThemePrimary &&
               ThemeBackground == other.ThemeBackground &&
               ThemeBackground1 == other.ThemeBackground1 &&
               BackgroundImage == other.BackgroundImage &&
               Equals(currentItem, other.currentItem);
        }

    // Override GetHashCode if you override Equals
    public override int GetHashCode()
    {
        // Implement based on your comparison logic
        return base.GetHashCode();
    }

    public async Task InitializeFromSession(AppState sessionState, SessionService _sessionService)
    {
        var _currentState = new AppState();
        var stored = await _sessionService.GetInitalAppStateFromSession();
        if (stored != null && _currentState.IsDifferentFrom(stored))
        {
            ColorTheme = ColorTheme;
            Direction = Direction;
            NavigationStyles = NavigationStyles;
            MenuStyles = MenuStyles;
            LayoutStyles = LayoutStyles;
            PageStyles = PageStyles;
            WidthStyles = WidthStyles;
            MenuPosition = MenuPosition;
            HeaderPosition = HeaderPosition;
            MenuColor = MenuColor;
            HeaderColor = HeaderColor;
            ThemePrimary = ThemePrimary;
            ThemeBackground = ThemeBackground;
            ThemeBackground1 = ThemeBackground1;
            BackgroundImage = BackgroundImage;
            currentItem = currentItem;
            await _sessionService.SetInitalAppStateToSession(_currentState);
        }
        // Check and assign session values if present
        else if (sessionState != null)
        {
            ColorTheme = sessionState.ColorTheme;
            Direction = sessionState.Direction;
            NavigationStyles = sessionState.NavigationStyles;
            MenuStyles = sessionState.MenuStyles;
            LayoutStyles = sessionState.LayoutStyles;
            PageStyles = sessionState.PageStyles;
            WidthStyles = sessionState.WidthStyles;
            MenuPosition = sessionState.MenuPosition;
            HeaderPosition = sessionState.HeaderPosition;
            MenuColor = sessionState.MenuColor;
            HeaderColor = sessionState.HeaderColor;
            ThemePrimary = sessionState.ThemePrimary;
            ThemeBackground = sessionState.ThemeBackground;
            ThemeBackground1 = sessionState.ThemeBackground1;
            BackgroundImage = sessionState.BackgroundImage;
            currentItem = sessionState.currentItem;
        }
    }
}
public class StateService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SessionService _sessionService;
    private readonly AppState _currentState;
    private readonly ILogger<AppState> _logger; // Define ILogger

    // private AppState _currentState = new AppState(); // Initialize with default state

    public AppState GetAppState()
    {
        return _currentState;
    }

    public event Action OnChange;
    // Event to notify subscribers about state changes
    public event Action? OnStateChanged;

    public StateService(IJSRuntime jsRuntime, SessionService sessionService, AppState appState, ILogger<AppState> logger)
    {
        _jsRuntime = jsRuntime;
        _sessionService = sessionService;
        _currentState = new AppState();
        OnChange = () => { };
        _logger = logger; // Initialize ILogger in constructor

        Task.Run(async () => await InitializeAppStateAsync());
    }

    private async Task InitializeAppStateAsync()
    {
        try
        {
            // Retrieve session values asynchronously
            var sessionState = await _sessionService.GetAppStateFromSession();
            var initialAppState = await _sessionService.GetInitalAppStateFromSession();
            if (initialAppState == null)
            {
                await _sessionService.SetInitalAppStateToSession(_currentState);
            }
            // Initialize AppState from session or default values
            await _currentState.InitializeFromSession(sessionState, _sessionService);

            // Notify state change if needed
            OnChange?.Invoke();
            NotifyStateChanged();

            //_logger.LogInformation("AppState initialized: {AppState}", JsonSerializer.Serialize(_currentState));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing AppState");
            // Handle exception as needed
        }
    }
    public async Task InitializeLandingAppState()
    {
        // Retrieve session values
        var sessionState = await _sessionService.GetAppStateFromSession();
        _currentState.NavigationStyles = "horizontal";
        _currentState.MenuStyles = "menu-hover";
        // Initialize AppState from session or default values
        await _currentState.InitializeFromSession(sessionState, _sessionService);

        // Notify state change
        NotifyStateChanged();
    }
    private async void NotifyStateChanged()
    {
        await _sessionService.SetAppStateToSession(_currentState);
        // Invoke the event to notify subscribers
        OnStateChanged?.Invoke();
    }
    public async Task directionFn(string val)
    {
        _currentState.Direction = val; // Update the color theme in the app state

        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "dir", val);
        NotifyStateChanged();
    }
    public Task setCurrentItem(MainMenuItems val)
    {
        _currentState.currentItem = val;
        return Task.CompletedTask;
    }
    public async Task colorthemeFn(string val, bool stateClick)
    {
        _currentState.ColorTheme = val; // Update the color theme in the app state
        if (stateClick)
        {
            _currentState.ThemeBackground = ""; // Update the color theme in the app state
            _currentState.ThemeBackground1 = ""; // Update the color theme in the app state
        }
        // await _jsRuntime.InvokeVoidAsync("interop.setclearCssVariables");

        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-theme-mode", val);
        if (stateClick)
        {
            if (val == "light")
            {
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-styles", "transparent");
                await menuColorFn("transparent");
                await headerColorFn("transparent");
            }
            else
            {   
                await menuColorFn(val);
            }
        }

        await _jsRuntime.InvokeVoidAsync("interop.removeCssVariable", "--body-bg-rgb");
        await _jsRuntime.InvokeVoidAsync("interop.removeCssVariable", "--body-bg-rgb2");
        await _jsRuntime.InvokeVoidAsync("interop.removeCssVariable", "--light-rgb");
        await _jsRuntime.InvokeVoidAsync("interop.removeCssVariable", "--form-control-bg");
        await _jsRuntime.InvokeVoidAsync("interop.removeCssVariable", "--input-border");
        await _jsRuntime.InvokeVoidAsync("interop.removeCssVariable", "--gray-3");
        NotifyStateChanged();
        await PersistState();
    }

    int screenSize = 1268;
    public async Task navigationStylesFn(string val, bool stateClick)
    {
        
        if (string.IsNullOrEmpty(_currentState.MenuStyles) && val == "horizontal")
        {
            _currentState.MenuStyles = "menu-click";
            _currentState.LayoutStyles = "";
            await menuStylesFn("menu-click");
        }
        if (stateClick && val == "vertical")
        {
            _currentState.MenuStyles = "";
            _currentState.LayoutStyles = "doublemenu";
    
        }
        // else
        // {
        //     // _currentState.LayoutStyles = "";
        //     await menuStylesFn(_currentState.MenuStyles);
        // }
        _currentState.NavigationStyles = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", val);
        if (val == "horizontal")
        {
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-vertical-style");

            // string result = await _jsRuntime.InvokeAsync<string>("interop.getAttributeToHtml", "data-nav-style");
            // if (result != null)
            // {
            //     await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-style", "menu-click");
            // }
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", val);
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "doublemenu");
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "double-menu-open");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-nav-style");

            // if (await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth") > 992)
            // {
            //     await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-toggled");
            // }
        }


        screenSize = await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth");

        if (screenSize < 992)
        {
            await _jsRuntime.InvokeAsync<string>("interop.addAttributeToHtml", "data-toggled", "close");
        }
        NotifyStateChanged();
    }
    public async Task layoutStylesFn(string val)
    {
        _currentState.LayoutStyles = val; // Update the color theme in the app state
        _currentState.MenuStyles = ""; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-nav-style");
        switch (val)
        {
            case "default-menu":
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "overlay");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", "vertical");
                if (await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth") > 992)
                {
                    await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-toggled");
                }
                break;
            case "closed-menu":
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "closed");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", "vertical");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "close-menu-close");
                break;
            case "detached":
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "detached");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", "vertical");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "detached-close");
                break;
            case "icontext-menu":
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "icontext");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", "vertical");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "icon-text-close");
                break;
            case "icon-overlay":
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "overlay");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", "vertical");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "icon-overlay-close");
                break;
            case "doublemenu":

                var isdoubleMenuActive = await _jsRuntime.InvokeAsync<bool>("interop.isEleExist", ".double-menu-active");

                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "doublemenu");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-layout", "vertical");
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "double-menu-open");
                if (!isdoubleMenuActive)
                {
                    await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "double-menu-close");
                }
                break;
        }
        screenSize = await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth");

        if (screenSize < 992)
        {
            await _jsRuntime.InvokeAsync<string>("interop.addAttributeToHtml", "data-toggled", "close");
        }
        NotifyStateChanged();
    }
    public async Task menuStylesFn(string val)
    {
        _currentState.LayoutStyles = ""; // Update the color theme in the app state
        _currentState.MenuStyles = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-vertical-style");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-hor-style");
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-style", val);

        screenSize = await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth");
        if (screenSize < 992)
        {
            await _jsRuntime.InvokeAsync<string>("interop.addAttributeToHtml", "data-toggled", "close");
        }
        else{
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", $"{val}-closed");
        }

        NotifyStateChanged();
    }
    public async Task pageStyleFn(string val)
    {
        _currentState.PageStyles = val; // Update the color theme in the app state 
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-page-style", val);
        NotifyStateChanged();
    }
    public async Task widthStylessFn(string val)
    {
        _currentState.WidthStyles = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-width", val);
        NotifyStateChanged();
    }
    public async Task menuPositionFn(string val)
    {
        _currentState.MenuPosition = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-position", val);
        NotifyStateChanged();
    }
    public async Task headerPositionFn(string val)
    {
        _currentState.HeaderPosition = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-header-position", val);
        NotifyStateChanged();
    }
    public async Task menuColorFn(string val)
    {
        _currentState.MenuColor = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-styles", val);
        NotifyStateChanged();
    }
    public async Task headerColorFn(string val)
    {
        _currentState.HeaderColor = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-header-styles", val);
        NotifyStateChanged();
    }
    public async Task themePrimaryFn(string val)
    {
        _currentState.ThemePrimary = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--primary-rgb", val);
        NotifyStateChanged();
    }
    public async Task themeBackgroundFn(string val, string val2,bool stateClick)
    {
        _currentState.ThemeBackground = val; // Update the color theme in the app state
        _currentState.ThemeBackground1 = val2; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-theme-mode", "dark");
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-header-styles", "dark");
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-styles", "dark");
         _currentState.ColorTheme = "dark";
        if (stateClick)
        {
            _currentState.MenuColor = "dark";
            _currentState.HeaderColor = "dark";
        }
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--body-bg-rgb", val);
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--body-bg-rgb2", val2);
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--light-rgb", val2);
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--form-control-bg", $"rgb({val2})");
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--input-border", "rgba(255,255,255,0.1)");
        await _jsRuntime.InvokeVoidAsync("interop.setCssVariable", "--gray-3", $"rgb({val2})");
        NotifyStateChanged();
    }
    public async Task backgroundImageFn(string val)
    {
        _currentState.BackgroundImage = val; // Update the color theme in the app state
        await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-bg-img", val);
        NotifyStateChanged();
    }
    public async Task reset()
    {
        _currentState.ColorTheme = "light";                   // light, dark
        _currentState.Direction = "ltr";                      // ltr, rtl
        _currentState.NavigationStyles = "vertical";          // vertical, horizontal   
        _currentState.MenuStyles = "";                        // menu-click, menu-hover, icon-click, icon-hover
        _currentState.LayoutStyles = "doublemenu";          // doublemenu, detached, icon-overlay, icontext-menu, closed-menu, default-menu 
        _currentState.PageStyles = "flat";                 // regular, classic, modern, flat
        _currentState.WidthStyles = "fullwidth";                // default, fullwidth, boxed
        _currentState.MenuPosition = "fixed";                 // fixed, scrollable
        _currentState.HeaderPosition = "fixed";               // fixed, scrollable
        _currentState.MenuColor = "transparent";                     // light, dark, color, gradient, transparent
        _currentState.HeaderColor = "transparent";                   // light, dark, color, gradient, transparent
        _currentState.ThemePrimary = "";                      // '58, 88, 146', '92, 144, 163', '161, 90, 223', '78, 172, 76', '223, 90, 90'
        _currentState.ThemeBackground = "";
        _currentState.ThemeBackground1 = "";
        _currentState.BackgroundImage = "";                   // bgimg1, bgimg2, bgimg3, bgimg4, bgimg5

        // clearing localstorage
        await _jsRuntime.InvokeVoidAsync("interop.clearAllLocalStorage"); 
        await _jsRuntime.InvokeVoidAsync("interop.setclearCssVariables");

        // reseting to light
        await colorthemeFn("light", false);

        //To reset the light-rgb
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "style");

        // clearing attibutes
        // removing header, menu, pageStyle & boxed
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-nav-style");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-menu-position");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-header-position");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-page-style");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-width");

        // removing theme styles
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-bg-img");

        // reseting to ltr
        await directionFn("ltr");

        //resetting layoutStyle
        await layoutStylesFn("doublemenu");

        // reseting to default
        await widthStylessFn("fullwidth");

        // reseting to vertical
        await navigationStylesFn("vertical", false);

        // resetting the menu Color
        await menuColorFn("transparent");

        var innerWidth = await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth");
        if (innerWidth < 992)
        {
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "close");
        }else{
            
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "double-menu-close");   
        }

  

        // resetting the menu Color
        await headerColorFn("transparent");

        _sessionService.DeleteAppStateFromSession();

        NotifyStateChanged();
    }
    public async Task Landingreset()
    {
        // clearing localstorage
        await _jsRuntime.InvokeVoidAsync("interop.clearAllLocalStorage");

        // reseting to light
        await colorthemeFn("light", false);

        //To reset the light-rgb
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "style");
        // removing theme styles

        // reseting to ltr
        await directionFn("ltr");
        await menuColorFn("transparent");
        _currentState.ThemePrimary = "";
        _sessionService.DeleteAppStateFromSession();
        NotifyStateChanged();
    }
    public async Task retrieveFromLocalStorage()
    {
        string direction = _currentState.Direction;
        await directionFn(direction);
        string navstyles = _currentState.NavigationStyles;
        await navigationStylesFn(navstyles, false);
        string pageStyle = _currentState.PageStyles;
        await pageStyleFn(pageStyle);
        string widthStyles = _currentState.WidthStyles;
        await widthStylessFn(widthStyles);
        string Vyzormenuposition = _currentState.MenuPosition;
        await menuPositionFn(Vyzormenuposition);
        string Vyzorheaderposition = _currentState.HeaderPosition;
        await headerPositionFn(Vyzorheaderposition);
        string Vyzorcolortheme = _currentState.ColorTheme;
        await colorthemeFn(Vyzorcolortheme, false);
        string Vyzorbgimg = _currentState.BackgroundImage;
        if (!string.IsNullOrEmpty(Vyzorbgimg))
        {
            await backgroundImageFn(Vyzorbgimg);
        }
        string Vyzorbgcolor = _currentState.ThemeBackground;
        string Vyzorbgcolor1 = _currentState.ThemeBackground1;
        if (!string.IsNullOrEmpty(Vyzorbgcolor))
        {
            await themeBackgroundFn(Vyzorbgcolor, Vyzorbgcolor1,false);
            _currentState.ColorTheme = "dark";
            // _currentState.MenuColor = "dark";
            // _currentState.HeaderColor = "dark";
        }
        string VyzorMenu = _currentState.MenuColor;
        await menuColorFn(VyzorMenu);
        string VyzorHeader = _currentState.HeaderColor;
        await headerColorFn(VyzorHeader);
        string VyzormenuStyles = _currentState.MenuStyles;
        string Vyzorverticalstyles = _currentState.LayoutStyles;
        if (string.IsNullOrEmpty(Vyzorverticalstyles))
        {
            await menuStylesFn(VyzormenuStyles);
        }
        else
        {
            await layoutStylesFn(Vyzorverticalstyles);
        }
        string VyzorprimaryRGB = _currentState.ThemePrimary;
        await themePrimaryFn(VyzorprimaryRGB);
    }
    public async Task retrieveFromLandingLocalStorage()
    {
        await navigationStylesFn("horizontal", false);
        _currentState.MenuStyles = "menu-hover";
        _currentState.LayoutStyles = "";
        await menuStylesFn("menu-hover");

        string direction = await _jsRuntime.InvokeAsync<string>("interop.getLocalStorageItem", "Vyzordirection") ?? _currentState.Direction;
        await directionFn(direction);
        string Vyzorcolortheme = await _jsRuntime.InvokeAsync<string>("interop.getLocalStorageItem", "Vyzorcolortheme") ?? _currentState.ColorTheme;
        await colorthemeFn(Vyzorcolortheme, false);
        string VyzorprimaryRGB = await _jsRuntime.InvokeAsync<string>("interop.getLocalStorageItem", "VyzorprimaryRGB") ?? _currentState.ThemePrimary;
        await themePrimaryFn(VyzorprimaryRGB);

        // clearing attibutes
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-header-styles");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-menu-styles");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-page-style");
        await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-width");
        // await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-header-position");

        var innerWidth = await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth");
        if (innerWidth < 992)
        {
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", "close");
        }
    }
    private async Task PersistState()
    {
        // Logic to persist state (e.g., save to local storage or database)
        // This can vary depending on your application requirements
        await Task.Delay(0); // Placeholder for actual persistence logic
        await _sessionService.SetAppStateToSession(_currentState);
    }
}
