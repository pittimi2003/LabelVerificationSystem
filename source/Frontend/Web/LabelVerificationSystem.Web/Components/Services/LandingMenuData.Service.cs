public class LandingMenuDataService
{
    private List<MainMenuItems> MenuData = new List<MainMenuItems>()
    {
        new MainMenuItems (
            path: "/landing#home",
            type: "link",
            title: "Home",
            selected: false,
            active: false,
            dirChange: false
        ),
        new MainMenuItems (
            path: "/landing#feature",
            type: "link",
            title: "Features",
            selected: false,
            active: false,
            dirChange: false
        ),
        new MainMenuItems (
            path: "/landing#service",
            type: "link",
            title: "Services",
            selected: false,
            active: false,
            dirChange: false
        ),
        
        new MainMenuItems (
            type: "sub",
            title: "Pages",
            selected: false,
            active: false,
            dirChange: false,
            children: new MainMenuItems[]
            {
                new MainMenuItems (
                    path: "javascript:void(0);",
                    type: "link",
                    title: "Abous Us",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "javascript:void(0);",
                    type: "link",
                    title: "Terms & Conditions",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "javascript:void(0);",
                    type: "link",
                    title: "Privacy Policy",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    type: "sub",
                    title: "Level-2",
                    selected: false,
                    active: false,
                    dirChange: false,
                    children: new MainMenuItems[]
                    {
                        new MainMenuItems (
                            path: "",
                            type: "empty",
                            title: "Level-2-1",
                            selected: false,
                            active: false,
                            dirChange: false
                        ),
                        new MainMenuItems (
                            type: "sub",
                            title: "Level-2-2",
                            selected: false,
                            active: false,
                            dirChange: false,
                            children: new MainMenuItems[]
                            {
                                new MainMenuItems (
                                    path: "",
                                    type: "empty",
                                    title: "Level-2-2-1",
                                    selected: false,
                                    active: false,
                                    dirChange: false
                                ),
                                new MainMenuItems (
                                    path: "",
                                    type: "empty",
                                    title: "Level-2-2-2",
                                    selected: false,
                                    active: false,
                                    dirChange: false
                                )
                            }
                        )
                    }
                )
            }
        ),
        
        new MainMenuItems (
            path: "/landing#price",
            type: "link",
            title: "Subscription",
            selected: false,
            active: false,
            dirChange: false
        ),
        new MainMenuItems (
            path: "/landing#contactus",
            type: "link",
            title: "Contact Us",
            selected: false,
            active: false,
            dirChange: false
        ),
    };

    public List<MainMenuItems> GetMenuData()
   {
        return MenuData;
    }
}
