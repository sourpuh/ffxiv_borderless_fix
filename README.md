# Borderless window fix plugin

Making well-behaved borderless window is surprisingly non-trivial. Most games, FF included, don't get it quite right:
* you can't easily minimize borderless window, either via win+down shortcut, or by clicking on taskbar
* win+left/right make window that's supposed to be fullscreen actually non-full-screen, which leads to further incorrectness
* win+shift+left/right moves window to different monitors, however if they have different resolutions, swapchain isn't actually resized, leading to graphical and mouse-position artifacts

This wee plugin tries to fix these problems.

As a configuration option, it allows you to select whether you want to maximize window to whole monitor area or to 'work area' (excluding things like taskbar).
