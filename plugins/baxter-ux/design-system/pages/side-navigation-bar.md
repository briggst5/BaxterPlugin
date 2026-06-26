# Side Navigation Bar

Source: https://zeroheight.com/67cfae98e/v/latest/p/025ec9-side-navigation-bar

## Colors found

- `#212121`
- `#605f60`
- `#FFFFFF`
- `#A9A9A9`
- `#2265c9`
- `#F6F6F7`
- `#000000`
- `#606774`
- `#EDEEF0`
- `#2359FB`
- `#F4F6FF`
- `rgb(21,72,152)`
- `rgb(255,255,255)`

## Sections

## Side Navigation Bar



### Anatomy

A Side Navigation Bar is composed of at least two Nav Items. Its height and width are variable and should be adjusted depending on the grid, screen size and item label's length although each item's size should ensure a minimum tappable area in compliance with Design for all guidelines.

1
2
3
Side Navigation Bar
1
Nav Item
2
Selection bar
3
Container





### Size Specifications

The Side Navigation Bar is a responsive component that significantly changes in appearance and interaction based on the viewport in which it is displayed, particularly on XS screens.







### Style Variants

Use boolean properties of the Side Navigation Bar component to adjust items’ arrangement. This configuration can contain up to 6 page destinations of equal importance. Set Top nav item, Bottom nav item, Avatar and all n. Item properties.







### Orientation Variants

Nav Items can be vertical, with the text below the icon, or horizontal, with the icon and text beside each other. Choose the most fitting configuration using Style variants based on screen size and ratio, as well as number of items needed to navigate the product.







### Nav Item



#### Current Selection

The selection indicator tells people at a glance which page is active. Use a duotone icon for the active page destination and outlined icons for inactive items. The selection indicator tells people at a glance which page is active. The navigation item corresponding to the currently viewed page is set to isCurrent: true. This means that only one navigation item in the Side Navigation Bar can be set to isCurrent: true at any given time.



##### Prototype





#### Interaction States

As an interactive component, the navigation item has its own State variants.





#### Notification Badge

The Notification Dot boolean property can be activated when it is necessary to draw the user's attention to a section of the product that is currently hidden. This property is only available for navigation items that are not set to isCurrent: true.







#### Related components

Tab Navigation
Bottom Navigation Ba

## Code / specs

```
Top nav item
```

```
Bottom nav item
```

```
Avatar
```

```
n. Item
```

```
Style
```

```
isCurrent: true
```

```
State
```

```
Notification Dot
```

## Full page text

Skip to content
