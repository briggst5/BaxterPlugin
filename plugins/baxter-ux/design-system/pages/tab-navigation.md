# Tab Navigation

Source: https://zeroheight.com/67cfae98e/v/latest/p/912d05-tab-navigation

## Colors found

- `#212121`
- `#605f60`
- `#FFFFFF`
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

## Tab Navigation



### Anatomy

1
2
3
4
Tab Navigation
1
Tab Item
2
Selection Bar
3
Notification dot
4
Border





### Size Variants

Small (sm)

Height: 32px

Border: stroke-sm

Border stroke color: stroke/eutral/primary

Horizontal padding: padding-lg

Spacing between Tab Items: gap-4xl

Internal spacing Tab: gap-md

Internal spacing Label: gap-xs

Medium (md)

Height: 40px

Border: stroke-sm

Border stroke color: stroke/eutral/primary

Horizontal padding: padding-lg

Spacing between Tab Items: gap-4xl

Internal spacing Tab: gap-lg

Internal spacing Label: gap-xs

Large (lg)

Height: 56px

Border: stroke-sm

Border stroke color: stroke/eutral/primary

Horizontal padding: padding-lg

Spacing between Tab Items: gap-4xl

Internal spacing Tab: gap-lg

Internal spacing Label: gap-xs





### Presets

To enhance usability, the component includes a property that allows designers to select from various presets, accommodating up to 10 tab items. At least 2 tabs are required to make a tab component. Read all guidelines on Number of Tabs.







### Padding

The Tab navigation component can be customized to include or exclude horizontal padding. This flexibility allows the component to be used in various contexts, such as within a foreground element like a modal or card.







### Tab Item

The Tab item serves as the foundational element of the Tab navigation component. This building block can be customized in multiple ways to ensure flexibility for various use cases.



#### Interaction States





#### Current Selection

The isCurrent boolean property indicates whether a tab item corresponds to the currently displayed content. Within a single Tab navigation, only one tab item can have isCurrent:true.





#### Notification Dot

A tab that is not currently visible can display a notification to indicate that the user needs to pay attention to something. To activate, set Notification Dot boolean property to true.





#### Leading Icon

The Leading Icon boolean property can be activated when the design can benefit from tabs having distinguishable icons. Generally, this property should be enabled for all tabs or for none at all. However, an exception can be made to display the icon only on the first tab, such as when it represents the main tab.



Example of Tab Navigation on Med Surg Bed.





#### Related components

Side Bar

## Code / specs

```
stroke-sm
```

```
stroke/eutral/primary
```

```
padding-lg
```

```
gap-4xl
```

```
gap-md
```

```
gap-xs
```

```
gap-lg
```

```
isCurrent
```

```
isCurrent:true
```

```
Notification Dot
```

```
Leading Icon
```

## Full page text

Skip to content
