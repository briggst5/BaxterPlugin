# Focus Mode Overlay

Source: https://zeroheight.com/67cfae98e/v/latest/p/26da74-focus-mode-overlay

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

## Focus Mode Overlay



### Anatomy









### Interaction types

The Focus Mode Overlay can be triggered either through a digital control or a physical button. Its visual treatment looks similar to a Modal component since the design objective of a Focus Mode is to reduce visual clutter while performing a task. Once active, it supports three modes of interaction for executing actions: fully digital, fully physical, or hybrid, depending on the product’s context, usage and available controls.

See: Product Definition Framework





#### Full digital

Actions are performed entirely within the digital interface; this model is best suited for tasks where variety of input or direct visual feedback is essential.
The overlay may include standard digital buttons (e.g., “Start”, “Cancel”) or dedicated on-screen controls resembling joysticks or sliders.



Example of Focus Mode with a Full Digital interaction on Med Surg Bed.

Example of Focus Mode with a Full Digital interaction on Med Surg Bed.



#### Hybrid

This interaction model combines digital UI prompts with physical inputs, an approach that guides the user while still leveraging tactile inputs, ensuring clarity and reducing cognitive load when multiple controls share the same hardware.
The Focus Mode displays Action Hints that highlight which physical buttons should be pressed to complete the current adjustment, as functions may vary slightly depending on the selected command. A highly scalable approach to reduce clutter on the physical panel, taking advantage of the dynamic and contextual nature of the UI elements on the screen.

See: Action Hint



Example of Focus Mode with a Hybrid interaction on Med Surg Bed.



#### Full Physical

This model is useful when the user’s focus needs to remain on the patient hence when tactile feedback is preferred, since actions are performed using the product’s hardware buttons or controls. It typically involves feature-specific buttons that immediately act on the control or multi-purpose physical inputs (e.g. directional arrows) that adjust different settings depending on the current context.

A safety delay may be a safety requirement to prevent the user from accidentally pressing the button: in this case, the Focus Mode appears anyway to make the most out of the waiting time and help users verify that they are pressing the correct button. To strengthen the reference to the real movement of the table, an animation is displayed. The upper area of the focus mode overlay gives a visual cue of whether the adjustment is happening or not, with the safety delay countdown being replaced by the live number as soon as the bed is actually moving.



Examples of Focus Mode with a Physical interaction on GSS remote.





### Application examples

As a context-aware component, the content of the Focus Mode Overlay depends on touchpoint specific features or peculiar conditions; one of the most compelling use cases is leveraging the screen to display the current status of a physical adjustment – especially when the effects of the command aren’t immediately visible – to add a layer of interpretation to something happening in the real world (e.g. movement).





#### Precise adjustments

For precise adjustments, the dialog provides continuous, accurate feedback that supports fine-tuning with confidence. The current value is displayed as a live data, giving feedback on the successful interaction and providing an insight on the adjustment status.





Example of Focus Mode with Action Hints for a precise adjustment of HOB on Med Surg Bed. Here, the Status rim acts as a gyroscope to visually show the current angle.

Example of Focus Mode for a precise adjustment of Trendelenburg with a Physical interaction on GSS Remote.



#### Presets

For presets – which often include a series of steps that perform automatically in sequence – the Focus Mode Overlay clarifies the command’s effect on the product by displaying a preview. It also ensures that even when a user interacts with a control whose effects are not prominent or immediately visible on the product, the system acknowledges the action.
Based on the complexity of the preset, the screen real estate and the focus of the preview, the style of the visualization can change: from pictograms to 3D models, it is usually valuable to include animated elements that highlight movements.





Example of Progress Focus Mode on Med Surg Bed.



#### HMI Information and status

As the Focus Mode Overlay is born with convergent interactions in mind, the layout may include a dedicated area to display information related to the status of the product (e.g. locked, safety delay countdown) or Action Hints that indicate which button to press. This enhances the connection between the physical product and the digital interface, leveraging the screen to display relevant, contextual information in a timely manner.











#### Related components

Action hint



## Full page text

Skip to content
