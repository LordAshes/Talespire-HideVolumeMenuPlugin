# Hide Volume Menu Plugin

This unofficial TaleSpire plugin is for adding labels to Hide Volumes and being able to toggle their.
visibility from a common group menu without having to select the Hide Volume first.

Lord Ashes' Talespire Plugins (and their source code) are always available for free but if you really want to
make a donation to the chocolate fund to keep the work going, you can do so using the following link:

Donate: http://198.91.243.185/TalespireDonate/Donate.php

## Change Log

```
1.1.0: Addes support for remote request via Asset Data Plugin
1.0.3: Exposed instance of the plugin via Instance() method to allow it to be used as dependency
1.0.2: Added place holder to ensure CustomData sub-folder is created. No plugin change
1.0.1: Bug fix: Exeption no longer thrown if save is done when labels have no been shown
1.0.0: Initial release
```

## Install

Download and install using R2ModMan.

Parameters including the layout of the menu and the size of the labels is configurable using the R2ModMan
configuration for the plugin. Default values can be used as is.

## Usage

1. Use the normal core TS functionality for making one or more Hide Volumes.
2. Use the Hide Volumes Menu shortcut (default LCTRL+H) to open the Hide Volumes Menu.
3. Click on the toggle before the name to toggle the corresponding Hide Volume.
4. Click on the Hide Volume name button to prompt for a new name.

Note: Labels are above the hide volumes when in Hide Volume mode.

## Configuration

### Menu Layout

``Vertical Start`` indicates the pixel position from the top of the screen where the first menu entry will
be positioned for the first and any succesive columns (if multiple columns are used). 

``Vertical Offset`` indicates the number of vertical pixel between enties in the menu.

``Vertical Count`` indicates the number menu entries before a new column is started.

``Horizontal Start`` indicates the pixel position from the left of the screen to the left side of the first
column of menu entries.

``Horizontal Offset`` indicates the number of horizontal pixel that are added for each successive column.

Note: These values used here are not check for fit to screen. Setting incorrect settings can make menu
entries appear outside the display area of the screen. For example, if Vertical Offset is increased then
the Vertical Count needs to be correspondingly decreased otherwise some entries will be off screen.

### Label Info

``Vertical Offset`` indicates the tile distance (not pixel distance) that labels are above the Hide Volume.

``Font Size`` indicates the size of the font that is used to draw the labels.

