Text Format Example
===================

Self-contained demo of the MioHelper.TextFormat module. No scene assets needed:
attach SampleTextFormatDemo to any GameObject in a scene and press Play. It builds
a canvas + a MioTextSettings asset at runtime and renders each grammar feature as
a raw template next to its formatted output.

Grammar (see MioTextFormatter for the full reference):

  @{key}              place a value (letters/digits; e.g. @{damage} or @{401})
  @{key:fmt}          place a value, number-formatted by name (e.g. @{hp:pct})
  {C:name}..{/C}      named character-style span; {S:name} is an alias of {C:name}
  {N:fmt}..{/N}       number format applied to a literal or resolved value
  {L:keyword}..{/L}   TMP link span
  \{ \} \@ \\         literal escapes

Notes:
  - Nesting is arbitrary across families: {C:buff}{N:pct+}@{chance}{/N}{/C}
  - Closes are case-tolerant: {/c} closes a {C:...}
  - @{br} is a built-in control key for a line break
  - Values that contain template tags are re-tokenized
  - Unknown keys/styles/formats pass through verbatim + log a warning in dev builds
