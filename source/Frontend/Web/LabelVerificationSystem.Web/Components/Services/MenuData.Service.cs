public class MenuDataService
{
    private List<MainMenuItems> MenuData = new List<MainMenuItems>()
    {
        new MainMenuItems(
            menuTitle: "Main"
        ),
        new MainMenuItems(
            type: "sub",
            title: "Dashboards",
            icon: "",
            svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect> <path d='M133.66,34.34a8,8,0,0,0-11.32,0L40,116.69V216h64V152h48v64h64V116.69Z' opacity='0.2'></path> <line x1='16' y1='216' x2='240' y2='216' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line>  <polyline points='152 216 152 152 104 152 104 216' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></polyline><line x1='40' y1='116.69' x2='40' y2='216' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line><line x1='216' y1='216' x2='216' y2='116.69' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line><path d='M24,132.69l98.34-98.35a8,8,0,0,1,11.32,0L232,132.69' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></path></svg>",
            badgeValue: "",
            badgeClass: "",
            selected: false,
            active: false,
            dirChange: false,
            children: new MainMenuItems[]
            {
                new MainMenuItems (
                    path: "/index",
                    type: "link",
                    icon: "",
                    svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu-doublemenu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><path d='M54.46,201.54c-9.2-9.2-3.1-28.53-7.78-39.85C41.82,150,24,140.5,24,128s17.82-22,22.68-33.69C51.36,83,45.26,63.66,54.46,54.46S83,51.36,94.31,46.68C106.05,41.82,115.5,24,128,24S150,41.82,161.69,46.68c11.32,4.68,30.65-1.42,39.85,7.78s3.1,28.53,7.78,39.85C214.18,106.05,232,115.5,232,128S214.18,150,209.32,161.69c-4.68,11.32,1.42,30.65-7.78,39.85s-28.53,3.1-39.85,7.78C150,214.18,140.5,232,128,232s-22-17.82-33.69-22.68C83,204.64,63.66,210.74,54.46,201.54Z' opacity='0.2'></path><path d='M54.46,201.54c-9.2-9.2-3.1-28.53-7.78-39.85C41.82,150,24,140.5,24,128s17.82-22,22.68-33.69C51.36,83,45.26,63.66,54.46,54.46S83,51.36,94.31,46.68C106.05,41.82,115.5,24,128,24S150,41.82,161.69,46.68c11.32,4.68,30.65-1.42,39.85,7.78s3.1,28.53,7.78,39.85C214.18,106.05,232,115.5,232,128S214.18,150,209.32,161.69c-4.68,11.32,1.42,30.65-7.78,39.85s-28.53,3.1-39.85,7.78C150,214.18,140.5,232,128,232s-22-17.82-33.69-22.68C83,204.64,63.66,210.74,54.46,201.54Z' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></path><circle cx='96' cy='96' r='16' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></circle><circle cx='160' cy='160' r='16' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></circle><line x1='88' y1='168' x2='168' y2='88' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line></svg>",
                    title: "Sales",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "/excel-uploads",
                    type: "link",
                    icon: "",
                    title: "Carga Excel v1",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "/parts",
                    type: "link",
                    icon: "",
                    title: "Parts (Admin)",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "/users",
                    type: "link",
                    icon: "",
                    title: "Usuarios (Admin)",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "/authorization-matrix",
                    type: "link",
                    icon: "",
                    title: "Permisos por Rol",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    path: "/roles",
                    type: "link",
                    icon: "",
                    title: "Roles (Catálogo)",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
            }
        ),

        new MainMenuItems(
            menuTitle: "WEB APPS"
        ),
        new MainMenuItems (
            type: "sub",
            title: "Nested Menu",
            icon: "",
            svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><polygon points='32 80 128 136 224 80 128 24 32 80' opacity='0.2'></polygon><polyline points='32 176 128 232 224 176' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></polyline><polyline points='32 128 128 184 224 128' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></polyline><polygon points='32 80 128 136 224 80 128 24 32 80' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></polygon></svg>",
            selected: false,
            active: false,
            dirChange: false,
            children: new MainMenuItems[]
            {
                new MainMenuItems (
                    path: "",
                    type: "empty",
                    svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu-doublemenu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><rect x='32' y='80' width='160' height='128' rx='8' opacity='0.2'></rect><rect x='32' y='80' width='160' height='128' rx='8' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></rect><path d='M64,48H216a8,8,0,0,1,8,8V176' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></path></svg>",
                    title: "Nested-1",
                    selected: false,
                    active: false,
                    dirChange: false
                ),
                new MainMenuItems (
                    type: "sub",
                    svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu-doublemenu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><rect x='40' y='96' width='176' height='112' rx='8' opacity='0.2'></rect><rect x='40' y='96' width='176' height='112' rx='8' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></rect><line x1='56' y1='64' x2='200' y2='64' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line><line x1='72' y1='32' x2='184' y2='32' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line></svg>",
                    title: "Nested-2",
                    selected: false,
                    active: false,
                    dirChange: false,
                    children: new MainMenuItems[]
                    {
                        new MainMenuItems (
                            path: "",
                            type: "empty",
                            svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu-doublemenu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><rect x='40' y='96' width='176' height='112' rx='8' opacity='0.2'></rect><rect x='40' y='96' width='176' height='112' rx='8' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></rect><line x1='56' y1='64' x2='200' y2='64' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line><line x1='72' y1='32' x2='184' y2='32' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line></svg>",
                            title: "Nested-2-1",
                            selected: false,
                            active: false,
                            dirChange: false
                        ),
                        new MainMenuItems (
                            type: "sub",
                            title: "Nested-2-2",
                            selected: false,
                            active: false,
                            dirChange: false,
                            children: new MainMenuItems[]
                            {
                                new MainMenuItems (
                                    path: "",
                                    type: "empty",
                                    title: "Nested-2-2-1",
                                    selected: false,
                                    active: false,
                                    dirChange: false
                                ),
                                new MainMenuItems (
                                    path: "",
                                    type: "empty",
                                    title: "Nested-2-2-2",
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
         new MainMenuItems(
            menuTitle: "Pages"
        ),
        new MainMenuItems(
            type: "sub",
            title: "Pages",
            icon: "",
            svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><polygon points='152 32 152 88 208 88 152 32' opacity='0.2'></polygon><path d='M200,224H56a8,8,0,0,1-8-8V40a8,8,0,0,1,8-8h96l56,56V216A8,8,0,0,1,200,224Z' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></path><polyline points='152 32 152 88 208 88' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></polyline></svg>",
            badgeValue: "",
            badgeClass: "",
            selected: false,
            active: false,
            dirChange: false,
            children: new MainMenuItems[]
            {
                new MainMenuItems (
                    path: "",
                    type: "sub",
                    svg: "<svg xmlns='http://www.w3.org/2000/svg' class='side-menu-doublemenu__icon' viewBox='0 0 256 256'><rect width='256' height='256' fill='none'></rect><circle cx='128' cy='128' r='96' opacity='0.2'></circle><circle cx='128' cy='128' r='96' fill='none' stroke='currentColor' stroke-miterlimit='10' stroke-width='16'></circle><line x1='128' y1='136' x2='128' y2='80' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' stroke-width='16'></line><circle cx='128' cy='172' r='12'></circle></svg>",
                    title: "Error",
                    selected: false,
                    active: false,
                    dirChange: false,
                    children: new MainMenuItems[]
                    {
                        new MainMenuItems (
                            path: "/error401",
                            type: "link",
                            title: "401 - Error",
                            selected: false,
                            active: false,
                            dirChange: false
                        ),
                    }
                ),
            }
        ),
    };
    public List<MainMenuItems> GetMenuData()
    {
        return MenuData;
    }
}
