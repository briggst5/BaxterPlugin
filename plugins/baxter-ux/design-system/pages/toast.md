# Toast

Source: https://zeroheight.com/67cfae98e/v/latest/p/67a91f-toast

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

## Toast



### Anatomy

1
2
3
4
5
6
Toast
1
Container frame
2
Leading Icon (optional)
3
Title
4
Text (optional)
5
Button (optional)
6
Progress Bar (optional)





### Size Specifications

Toast

Height: Hug

Internal padding: padding-xl

Spacing: gap-2xl

Title text style: header/h4

Title text color: text/neutral/invert/primary

Text style: body/body-md-regular

Text color: text/neutral/invert/secondary

Button Style Variant: Ghost





### Mode Variants

This variant is essential to adapt nested elements correctly, since the component works with an inverted palette. When working in Figma, toasts require an explicit change in Mode variant rather than relying on the file’s overall light/dark mode. Switching this property ensures that all internal elements — text color, buttons, and other nested components styles — seamlessly adjust to the inverted mode.







### Variants

Use the Variant property to adjust layout. In the Stacked variant, the action wraps under the text content and the component can include a second button.







### Leading Icon

Use the Leading Icon boolean property to include a visual indicator of error or success.







### Title

Use the Title boolean property to customize content.







### Text

Use the Text boolean property to hide text if no additional information is needed.









### Actions

Use the isActionable boolean property if you need to display a button.







### Countdown

Use the Progress Bar boolean property to give a visual cue on how much time is left before auto dismissal. It is recommended to use this variant when the toast is actionable.







#### Related Components

Standard Button
Progress Bar



## Code / specs

```
padding-xl
```

```
gap-2xl
```

```
header/h4
```

```
text/neutral/invert/primary
```

```
body/body-md-regular
```

```
text/neutral/invert/secondary
```

```
Ghost
```

```
Mode
```

```
Variant
```

```
Leading Icon
```

```
Title
```

```
Text
```

```
isActionable
```

```
Progress Bar
```

## Full page text

Skip to content
