# Design language: charcoal and red

Updated 7 September 2026. This replaces the initial light/teal concept, following Paulo's explicit preference for **red over black**, with dark charcoal surfaces rather than requiring pure black everywhere. Applies to this fitness product's Studio, Coach and Client experiences. Brand name remains undecided; Repward is a placeholder in the concept.

## Character

An athletic performance journal: dark, precise, confident and readable. Use large performance numbers, strong typographic hierarchy and controlled red accents. Keep forms comfortable for long admin sessions and logging controls easy to find during a workout.

All application backgrounds are dark, including cards, inputs, popovers, dialogs, charts, navigation, loading states and empty states. Pale colors are for text and small symbols. Do not reintroduce cream cards, white input fields or teal highlights.

## Palette

These values are the implementation reference. The generated concept is a visual illustration and may approximate the exact colors.

| Token | Hex | Use |
| --- | --- | --- |
| Background | `#101114` | Page canvas and outer application background |
| Navigation | `#131418` | Sidebar and navigation shell |
| Surface | `#191B20` | Main panels and cards |
| Surface raised | `#23262D` | Dialogs, inputs and inset panels |
| Surface hover | `#2C3038` | Hovered rows and secondary controls |
| Divider | `#363A43` | Decorative separators and low-emphasis panel edges |
| Control border | `#777E8B` | Boundaries needed to identify editable controls |
| Primary action | `#D1273B` | Filled buttons with pale text |
| Primary hover | `#C92337` | Hovered primary buttons |
| Primary pressed | `#B71F32` | Pressed primary buttons |
| Accent | `#FF5267` | Active tabs, selected markers, links, focus rings and primary chart series |
| Accent surface | `#321B23` | Selected navigation backgrounds and restrained highlight panels |
| Primary text | `#F4F4F5` | Headings, metrics, input values and primary button labels |
| Secondary text | `#B6BAC4` | Descriptions, field labels, chart labels and supporting data |

Do not use the filled-button red for small text on dark panels; the brighter accent serves that purpose. Do not lower the opacity of important explanatory text. Boundaries for interactive controls use the stronger control border rather than the decorative divider.

## Components and interactions

- **Navigation:** graphite shell, subdued labels, an oxblood selected background and a bright red edge or underline. Selection also uses weight or a marker so color is not the only cue.
- **Primary actions:** filled crimson, pale semibold labels. Use for actions such as Start session, Log set and Publish revision. Keep one dominant action per focused task.
- **Secondary actions:** charcoal fill, visible border and pale label. Selected filters use an accent border and selected-state indicator.
- **Inputs:** raised charcoal backgrounds, visible boundaries, persistent labels, large pale values and explicit units. Focus uses a bright red outline with a gap from the control. Invalid fields include an error symbol and an actionable text explanation.
- **Cards and dialogs:** surface layering and borders establish depth. Avoid bright shadows, neon glow, glossy gradients or large saturated-red backgrounds. Dialog overlays darken the content behind them.
- **Tables:** dark rows, subtle separators, a slightly raised hover state and explicit selection controls. Keep numeric columns aligned.
- **Loading and empty states:** charcoal placeholders; explain what data is missing and provide the relevant action. No default light skeleton cards.
- **Destructive actions:** separate from normal workflow controls and explicitly name the consequence, such as Delete draft. Because red is also the brand color, an icon and wording must identify danger. Do not rely on a red button alone to distinguish deletion from normal actions.
- **Completion and sync:** use a checkmark with clear text such as Saved or Synced. A neutral pale indicator keeps the palette coherent; completed work does not need a green surface. Pending and failed states get distinct icons and explanatory labels.

## Charts and muscle workload

Charts sit directly on charcoal surfaces. Use bright red for the main performance series, pale gray for comparison data, and line styles or point markers to distinguish them. Label axes, units, time windows and series directly where practical.

Red is a brand accent, not automatically a warning or a statement that a muscle is overworked. Muscle maps need a named scale, legend and values available outside the illustration. A selected muscle receives a red outline. Present direct and indirect sets separately; a filled red bar and an outlined/patterned pale bar can distinguish them without relying only on hue.

Goal progress can use crimson/bright red on a dark track with the actual values alongside it. Never make the graphic imply a greater percentage than the data supports.

## Typography, spacing and shape

Use a readable sans-serif interface family, such as the existing client Inter family where available. Use semibold headings and tabular numerals for loads, repetitions, timers and trends. Avoid condensed uppercase text for instructions or lengthy labels.

Use an 8px spacing rhythm, approximately 12px panel corners and 8px control corners. Reserve pill shapes for compact statuses and filters. Client logging targets should be generously sized, around 48px or more, with visible units beside numeric values. Admin layouts can be denser while maintaining readable labels and keyboard access.

Honor text scaling and reduced-motion preferences. Focus must remain visible on every surface. Each interactive state must be distinguishable without color alone.

## Screen applications

| Area | Application of the language |
| --- | --- |
| Studio exercise editor | Graphite navigation; charcoal editor; darker grouping panels; red active tab; pale field labels; crimson Publish/Create revision action |
| Coach review | Dark roster and calendar; red selected athlete; clearly labelled trend comparisons; explicit status symbols |
| Client Today | Dark canvas; goal and next-session panels; prominent pale values; red Start/Resume action |
| Client Train | Large charcoal number inputs; previous results in readable secondary text; crimson Log set action; dark rest timer |
| Client Progress | Red trend line and goal fill; pale comparison labels; dark muscle-work panel |
| Client Circle | Dark participant rows; red emphasis on the selected challenge or own entry; labelled ranks, points and evidence states |

## Review scope

The current deliverable is a revised design concept and specification, not an application theme implementation. Color-pair contrast can be checked from these tokens; keyboard, screen-reader, scaling and device behavior require verification when the actual interfaces are built. The original light concept remains only as historical reference.

Calculated token contrast: primary text on primary action 4.70:1; secondary text on raised surface 7.80:1; bright accent on raised surface 4.80:1; control border against raised surface 3.71:1. The implementation action red was darkened slightly from the generated-image brief to keep pale button labels above 4.5:1. These checks cover the listed pairs, not every element in the generated image.
