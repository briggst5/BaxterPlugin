# Date Input

Source: https://zeroheight.com/67cfae98e/v/latest/p/572ee9-date-input

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

## Date Input



### Anatomy

1
2
3
4
5
6
7
Date Input
1
Label
2
Required mark
3
Text field
4
Calendar Icon
5
Value
6
Date Range arrow (optional)
7
Helper text





### Size Variants

Small (sm)

Max width: 400

Max height: 200

Border color: stroke/neutral/secondary

Border weight: stroke/sm

Border radius: rounded-md

Label Text style: label/md/regular

Label Text color: text/neutral/base/muted

Placeholder Text style: label/lg/regular

Placeholder Text color: text/neutral/base/secondary

Helper Text style: label/sm/regular

Helper text color: text/neutral/base/tertiary

Medium (md)

Max width: 400

Max height: 200

Border color: stroke/neutral/secondary

Border weight: stroke/sm

Border radius: rounded-md

Label Text style: label/md/regular

Label Text color: text/neutral/base/muted

Placeholder Text style: label/lg/regular

Placeholder Text color: text/neutral/base/secondary

Helper Text style: label/sm/regular

Helper text color: text/neutral/base/tertiary





### Filled

A Date Input can either be in a state where the main field has been filled by the user or in a placeholder state, which is the default state, where the main field is empty.







### Interaction States



#### Empty Interaction States

These States are applied to isFilled: false variant.



#### Filled Interaction States

The Disabled variant is valid as "Read Only" for text inputs that are set to isFilled:true.





Example of filled Date input with on Spot Monitor.





### Status Variants

Success variant is enabled only when the field contains input text (isFilled: true). In contrast, the Default, Warning, and Error variants are always available.

Interaction State variants are available for all Status variants, except the Disabled status as it is available exclusively for the Default one.







### Date Range

In some cases, the Date Input must accept a range of dates. The Date Input component features a control that enables designers to switch between accepting a single date or a date range.







### Required







### Helper Text and Alert Label

There are two boolean properties that can be toggled for the Helper text and the Alert label.



The Alert label is specific to the Warning and Error style variants and appears only after the user has entered an invalid input or attempted to submit a form without completing the required fields.







#### Related Components

Text Input
Number Input



## Code / specs

```
stroke/neutral/secondary
```

```
stroke/sm
```

```
rounded-md
```

```
label/md/regular
```

```
text/neutral/base/muted
```

```
label/lg/regular
```

```
text/neutral/base/secondary
```

```
label/sm/regular
```

```
text/neutral/base/tertiary
```

```
isFilled: false
```

```
isFilled:true
```

```
isFilled: true
```

## Full page text

Skip to content
