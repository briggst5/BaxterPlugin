# Text Input

Source: https://zeroheight.com/67cfae98e/v/latest/p/04d824-text-input

## Colors found

- `#212121`
- `#605f60`
- `#FFFFFF`
- `#f6f6f6`
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

## Text Input



### Anatomy

Basic elements are the same for both configurations of this component: Text Input and Text Area.

1
2
3
4
5
Text input
1
Label
2
Required Mark
3
Field
4
Value
5
Helper text





### Size Variants

⚠️ While the Small, Medium and Large variants are intended for general use, the Extra Large size is typically reserved to display numerical data and values. Large and Extra Large sizes don’t cover the Text area variant.



Example of Extra large Text input on Infusion Pump.





### Filled







### Interaction States



#### Empty Interaction States

These States are applied to isFilled: false variant.



#### Filled Interaction States

The Disabled variant is valid as "Read Only" for text inputs that are set to isFilled:true.







### Status Variants

Success variant is enabled only when the field contains input text (isFilled: true). In contrast, the Default, Warning, and Error variants are always available.

Interaction State variants are available for all Status variants, except the Disabled status as it is available exclusively for the Default one.







### Text Area







### Label

There may be a use case where it is possible or necessary to omit the label: it can be hidden using the Show label boolean.







### Placeholder Label

If an example of an accepted value is not meaningful or necessary, turn off the Show Placeholder label boolean.







### Unit suffix

⚠️ Units are not included in the Text area variant.



Example of Text inputs with Unit suffix on Spot Monitor.





### Helper text and Alert Label

There are two boolean properties that can be toggled for the Helper text and the Alert label.



The Alert label is specific to the Warning and Error style variants and can appear after the user has entered an invalid input or attempted to submit a form without completing the required fields.







### Required







#### Related components

Date Input
Selection Input
Number Input

## Code / specs

```
isFilled: false
```

```
isFilled:true
```

```
isFilled: true
```

```
Show label
```

```
Show Placeholder label
```

## Full page text

Skip to content
