# Heuristics & Clinical UI

Source: https://zeroheight.com/67cfae98e/v/latest/p/34aae0-heuristics--clinical-ui

## Colors found

- `#212121`
- `#605f60`
- `#2265c9`
- `#FFFFFF`
- `#F6F6F7`
- `#000000`
- `#606774`
- `#EDEEF0`
- `#2359FB`
- `#F4F6FF`
- `rgb(21,72,152)`
- `rgb(255,255,255)`

## Sections

## Heuristics & Clinical UI



### Touchscreen UI Element Sizing

The following heuristics focus on optimizing the usability and accessibility of our products in clinical environments—especially where users may wear gloves or operate in challenging lighting conditions. The guidance includes:
	•	Optimal dimensions for main CTA/Button and interactive UI elements 
	•	Spacing and padding for clear interaction zones
	•	Font sizes for maximum legibility
	•	Contrast ratios tailored for both dim and bright environments

These sizes align with research on touch target accuracy and medical industry usability guidelines.



#### Key UX Considerations

Glove-Friendly Touchscreen

Use capacitive screens optimized for gloved hands

Include haptic feedback when buttons are pressed

Use large hit areas (avoid small interactive elements)

Ambient Light Adaptation

Auto-brightness adjustment based on environment

Dark mode option for dimly lit surgical settings

Workflow Optimization

Minimize cognitive load → Avoid unnecessary steps in critical workflows

Provide undo options to prevent accidental actions







### Touchscreen UI Element Sizing



##### How to read the tables

The columns in the tables indicate 3 types of size in Pixel and in Millimeters based on a 160 ppi density:

	• Minimum: the minimum tolerable under standard conditions
	• Recommended: for sanitary environments with thin gloves
	• Optimal: for environments with high criticality, with thick gloves or in motion



#### Touch Targets & Button Sizes

UI element

	

Min (mm/px)

	

Rec (mm/px)

	

Opt (mm/px)

	

Notes




Main Touch area (Cards, icons, inputs)

	

10mm

63px




	

12mm

76px

	

15mm

95px

	

Tappable with gloves




Spacing between interactive items

	

3mm

19px

	

6mm

38px

	

8mm

51px

	

Prevents accidental touches when space is limited and elements are smaller




Buttons




	

12x12mm

76x76px

	

14x14mm

89x89px

	

16x16mm

101x101px

	

Main CTA

The design of the specified elements should adhere to the heuristics outlined above. For instance, the main button sizes have been defined as follows:

Small (S): 80px

Medium (M): 92px

Large (L): 104px







#### Padding, Spacing & Layout

UI element

	

Min (mm/px)

	

Rec (mm/px)

	

Opt (mm/px)

	

Notes




Button padding (internal)

	

1.2mm

8px




	

4mm

25px

	

6mm

38px

	







Outer padding

	

4mm

24px

	

8mm

51px

	

12mm

76px

	

Screen edges to prevent UI cutoff
or unintentional touches. Optional on XS screens.




Section / module / grouping

	

6mm

38px

	

10mm

63px

	

12-16mm

101px





#### Font Sizes - viewing distance based

Proximity

	

Min (mm/px)

	

Opt (mm/px)

	

Notes




Very close <40 cm

	

2mm

13px




	

2.5mm

16px

	

Labels, instructions




Near field (40-99 cm)

	

3mm

18px

	

4mm

25px

	







Mid field (1 - 2m)

	

3.5mm

22px

	

4.5mm

28px

	







Far field (3 - 4m)




	

5mm

32px

	

7mm

48px

	

Large critical info





#### Readability & Font Sizes

Text style

	

Min (mm/px)

	

Rec (mm/px)

	

Opt (mm/px)

	

Notes




Body text (non-critical)

	

2.5mm

16px

	

3mm

19px

	

3.5mm

22px

	







Labels / hints

	

2mm

13px

	

2.5mm

16px

	

3mm

19px

	







Critical values (e.g. vitals)

	

4mm

25px

	

5mm

32px

	

6mm

38px

	







Titles / headings

	

5mm

25px

	

6-7mm

38px

	

8+mm

51px





#### Contrast Ratio & Accessibility

Priority

	

Content

	

Min WCAG AA

	

Optimal




Normal

	

Text

	

4.5:1

	

7:1







	

Graphics

	

3:1

	







High

	

Large Text

	

4.5:1

	

7:1 Es: high and medium severity alarm







	

Normal Text

	

7:1

	










	

Graphics

	

3:1

Recommended contrast ratios:

Text vs. Background: 7:1 (for readability in all conditions)

Icons vs. Background: 4.5:1 (for recognizable UI elements)

Critical Alerts: ≥ 7:1 (to ensure visibility)



Contrast Requirements (for clinical environments)

Dimly lit settings (ICUs, surgical suites): High contrast is crucial

Well-lit settings (ERs, general wards): Avoid excessive contrast that causes glare



Color Considerations (for Color Blind Users)

Avoid relying on red/green alone (use patterns or labels instead)

Use blue, orange, and purple (safe for most types of color blindness)

Test with color blindness simulators (e.g. Coblis, Sim Daltonism)



Read more about use of color and contrast in Accessibility Guidelines.



## Tables

| UI element | Min (mm/px) | Rec (mm/px) | Opt (mm/px) | Notes |
| --- | --- | --- | --- | --- |
| Main Touch area (Cards, icons, inputs) | 10mm63px | 12mm76px | 15mm95px | Tappable with gloves |
| Spacing between interactive items | 3mm19px | 6mm38px | 8mm51px | Prevents accidental touches when space is limited and elements are smaller |
| Buttons | 12x12mm76x76px | 14x14mm89x89px | 16x16mm101x101px | Main CTA |

| UI element | Min (mm/px) | Rec (mm/px) | Opt (mm/px) | Notes |
| --- | --- | --- | --- | --- |
| Button padding (internal) | 1.2mm8px | 4mm25px | 6mm38px |  |
| Outer padding | 4mm24px | 8mm51px | 12mm76px | Screen edges to prevent UI cutoffor unintentional touches. Optional on XS screens. |
| Section / module / grouping | 6mm38px | 10mm63px | 12-16mm101px |  |

| Proximity | Min (mm/px) | Opt (mm/px) | Notes |
| --- | --- | --- | --- |
| Very close <40 cm | 2mm13px | 2.5mm16px | Labels, instructions |
| Near field (40-99 cm) | 3mm18px | 4mm25px |  |
| Mid field (1 - 2m) | 3.5mm22px | 4.5mm28px |  |
| Far field (3 - 4m) | 5mm32px | 7mm48px | Large critical info |

| Text style | Min (mm/px) | Rec (mm/px) | Opt (mm/px) | Notes |
| --- | --- | --- | --- | --- |
| Body text (non-critical) | 2.5mm16px | 3mm19px | 3.5mm22px |  |
| Labels / hints | 2mm13px | 2.5mm16px | 3mm19px |  |
| Critical values (e.g. vitals) | 4mm25px | 5mm32px | 6mm38px |  |
| Titles / headings | 5mm25px | 6-7mm38px | 8+mm51px |  |

| Priority | Content | Min WCAG AA | Optimal |
| --- | --- | --- | --- |
| Normal | Text | 4.5:1 | 7:1 |
|  | Graphics | 3:1 |  |
| High | Large Text | 4.5:1 | 7:1 Es: high and medium severity alarm |
|  | Normal Text | 7:1 |  |
|  | Graphics | 3:1 |  |

## Full page text

Skip to content
