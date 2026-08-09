# Build Colors
![Build Colors](./Mod/thumb.png)

- [Build Colors](#build-colors)
  - [🛠︰Info](#info)
  - [🛠︰Installation](#installation)
  - [🛠︰The panel](#the-panel)
  - [🛠︰Color Sets](#color-sets)
  - [🛠︰Paint Jobs](#paint-jobs)
  - [🛠︰Configs](#configs)
  - [🛠︰Support](#support)
  - [🛠︰Credits](#credits)

## 🛠︰Info

This mod allows you to create customized color sets for building, which can then be easily imported into another world.

## 🛠︰Installation

The easiest way is to download it from [SteamWorkshop](https://steamcommunity.com/sharedfiles/filedetails/?id=1475392343).

You can also download the latest zip from [releases](https://github.com/SiskSjet/RotorReturnHome/BuildColors) and extract it to your '%appdata%\SpaceEngineers\Mods' folder

## 🛠︰The panel

Everything this mod does lives on the game's own color picker screen, in the room left of the vanilla controls. One panel, with a rail down its left side switching between the three things it does:

```
┌ BUILD COLORS ─────────────────────────────────────────┐
│ ▸ Paint Jobs │ Jobs      Rules - "Hull"   Rule detail │
│   Color Sets │ Hull      1. Heavy armor   [Cond][Paint]│
│   Generator  │ Interior  2. Damaged        [0] all of  │
│              │                                        │
│              │ Alt+P apply · Alt+Z undo  [Undo][Apply]│
└───────────────────────────────────────────────────────┘
```

**Paint Jobs** edits jobs and their rules, **Color Sets** manages saved palettes, and **Generator** builds a palette from a color scheme. Anything that needs its own screen — a condition tree, an elaborate paint source, naming something — opens as a dialog over the panel. Close the color picker and the whole thing goes away; there is no second window to keep track of. The hotkeys are spelled out on the Paint Jobs page as they are actually bound, so a rebind shows up there.

## 🛠︰Color Sets

A color set is the fourteen build color slots saved under a name. The **Color Sets** tab lists them and shows the selected set beside the palette you are actually building with, in the same slot order — loading replaces the palette wholesale, so seeing both is how you know what that costs.

From there a set can be loaded, renamed, duplicated, marked a favourite, exported or removed. Double clicking loads it, **Load row 1** and **Load row 2** load only that half of the slots and leave the rest of your palette alone, and clicking a swatch opens that one slot in a colour picker. **Save current palette** goes the other way and stores what you have now; if the name is taken it asks before replacing.

Favourites sort to the top and the filter box narrows a long list as you type.

### Sharing

**Export** turns a set into a short code. **Import** takes one back — paste it into the dialog, or use the chat command, which is the easier direction because the game's chat accepts a paste.

### Generator

The **Generator** tab builds a set instead of collecting one by hand.

Pick a scheme — analogous, complementary, split complementary, triadic, tetradic, square, monochromatic, or hull and accent — and a preset that decides the mood. The preset shapes every slot, not only the colour the scheme is grown from.

With no preset the palette follows the base colour itself: a dark base gives a dark palette, a washed out one a washed out palette, and the middle of the first row lands on the base colour you picked. A preset overrides that, which is what a preset is for.

The layout is deliberate: the first row is a light to dark ramp of the primary hue, the row of number keys you build most of a hull with. The second row carries the accents of the scheme and then a neutral ramp, tinted slightly towards the primary hue so the greys do not read as dirty next to it. How many of those greys you get is yours to choose.

**Base color** says where the colour the scheme is grown from comes from: rolled at **random**, taken **from palette**, sampled **from the target grid** you are looking at, or **custom** on the picker. The two sampling sources lay their colours out as swatches and let you click the one you want, starting on the most colourful. The picker appears with them and holds whatever is chosen — moving it switches to custom — and stays out of the way while the base colour is being rolled.

**Generate** rolls a new palette. Everything else — scheme, preset, greys, base colour — re-rolls in place, so changing one of them shows you what that change did rather than an unrelated palette.

Click any generated swatch to edit that slot; right click to lock it. Locked slots are held through every following roll, so a palette can be worked out one colour at a time instead of rolled until it happens to be right. No two slots are allowed to end up looking the same: they are compared perceptually and pushed apart.

`Usage: /bc [command] [arguments]`

**Available commands**:
* **save** [*name*] *- Saves a Color Set with the given name.*
* **load** [*name*] *- Loads a Color Set with the given name.*
* **remove** [*name*] *- Removes a Color Set with given name.*
* **generate** [*scheme*] [*preset*] *- Rolls a palette into the build colors. Both arguments are optional and may be given in any order.*
* **export** [*name*] *- Prints the share code of a color set.*
* **import** [*code*] *- Adds a color set from a share code.*
* **list** *- Lists all available color sets.*
* **help** *- Shows a help window with all commands.*

### Stored colors

Colour sets are kept as the hue, saturation and value the game itself holds, not as RGB. Before this they went through eight bit RGB on the way to the file, which rounded the numbers the player had dialled in and clamped anything outside the sRGB gamut, so a set did not always load back as it was saved.

Files written by an older version are read and converted the first time they are loaded. Once converted they are stored the new way only, so a set saved by this version cannot be read by an older build of the mod.

## 🛠︰Paint Jobs

A paint job is a list of rules that is applied to the grid you are looking at. Each rule tests a block against a tree of conditions and paints it with a color, a skin, or both. The first matching rule wins.

What a rule paints with is called its paint source. A source is either a single color and skin, or one of four that work the color out per block from where that block sits on the grid: a **gradient** between stops along an axis, **camo** in patches, **scatter** picking per block by weight, or a **pattern** of stripes or checkers. A rule uses one or the other, never both. Every color a source picks from carries an optional skin, so a rust patch can bring the rusty skin along with the brown. Sources are deterministic: the same job on the same build always gives the same result, so re-applying it changes nothing.

Gradients run along the longest axis of the build by default and stretch across the blocks the rule actually paints, so both end colors always show up even when the rule only covers part of a grid. A build that is mostly one layer thick along the chosen axis is the exception: no gradient along that axis can spread it out, so pick another one.

Every application is recorded, and **undojob** puts one back. The history holds the last ten applications and lives only for the session.

### Hotkeys

One job is the **active** one — whichever you last picked in the Paint Jobs tab. These keys act on it with no menu open, and are rebindable in the Rich HUD terminal under Build Colors ▸ Controls:

| Default | Does |
|---|---|
| `Alt` + `P` | Applies the active paint job to the grid you are looking at |
| `Alt` + `Z` | Puts the last application back |
| `Alt` + `.` | Next paint job — a notification names the one you land on |
| `Alt` + `,` | Previous paint job |

Two-key combos on purpose: a paint job repaints a whole grid, which is not something a stray keypress should start. They do nothing while the chat is open or while a text field has focus.

Paint jobs are edited in the **Paint Jobs** tab of the panel, which shows the jobs, the rules of the selected job and the detail of the selected rule side by side. Everything it can do is also available in chat.

```
Jobs           Rules - "Hull"          Rule - "Heavy armor"
 Hull          1. Heavy armor          [Conditions] [Paint]
 Interior      2. Damaged               [0] all of
 Camo test     3. Everything else         [1] category is heavy
                                          [2] size is large
```

Edits are written straight back, so there is nothing to save. Conditions and the more elaborate paint sources open in a dialog over the panel.

`Usage: /bc [command] [arguments]`

Names hold spaces, so arguments are quoted: `/bc ApplyJob "My Hull"`. Instead of a name you can select a job or rule by its position in the matching listing with `#2`.

**Jobs**:
* **jobs** *- Lists all paint jobs.*
* **showjob** [*job*] *- Shows a paint job with its rules, conditions and their paths.*
* **applyjob** [*job*] *- Applies a paint job to the targeted grid.*
* **undojob** [*#position*] *- Puts back an application of a paint job. Without a position the most recent one is undone.*
* **painthistory** *- Lists the applications that can still be undone.*
* **newjob** [*name*] *- Creates a new paint job.*
* **removejob** [*job*] *- Removes a paint job.*
* **renamejob** [*job*] [*new name*] *- Renames a paint job.*
* **copyjob** [*job*] [*new name*] *- Copies a paint job.*
* **joboption** [*job*] [*subgrids\|projected\|preview\|ownership*] [*on\|off*] *- Sets an option of a paint job. Without a value the option is flipped.*

**Rules**:
* **addrule** [*job*] [*rule name*] *- Adds a rule. Rules are tested top down, so the position matters.*
* **removerule** [*job*] [*rule*] *- Removes a rule.*
* **renamerule** [*job*] [*rule*] [*new name*] *- Renames a rule.*
* **moverule** [*job*] [*rule*] [*position*] *- Moves a rule to another position.*
* **ruleaction** [*job*] [*rule*] [*field=value ...*] *- Sets what the rule paints: `color=R,G,B` or `color=#RRGGBB`, `applycolor=on|off`, `skin=<id>`, `applyskin=on|off`. Naming a color or a skin turns applying it on.*

**Paint sources** are set with the same **ruleaction** command:

* `source=solid|gradient|camo|scatter|pattern` *- Which kind of source the rule paints with.*
* `stops=<color>[@position][:skin];...` *- Gradient stops, separated by semicolons because a color may itself be written as `R,G,B`. Positions run from 0 to 1; stops written without one are spread evenly. Listing stops turns the source into a gradient.*
* `palette=<color>[*weight][:skin];...` *- Colors for camo, scatter and pattern. The weight is only read by scatter.*
* `axis=longest|x|y|z|up|radial` *- Direction a gradient or a stripe runs along, `longest` by default. `up` follows gravity, `radial` runs outwards from the middle of the grid.*
* `fit=blocks|grid` *- What the ends of a gradient are pinned to. `blocks`, the default, spans the blocks the rule paints so both end colors always appear; `grid` spans the whole build so several rules can share one gradient.*
* `blend=lab|hsv|rgb` *- Space a gradient mixes its colors in. Lab spaces the steps the way an eye reads them, HSV keeps them saturated, RGB darkens through the middle.*
* `steps=<n>` *- Number of bands a gradient is snapped to, 0 for a smooth blend. Bands are worth keeping: a smooth gradient gives nearly every block its own color, which costs one network message per block.*
* `scale=<blocks>` *- Rough width of a camo patch.*
* `shape=stripes|checker` and `period=<blocks>` *- Shape and width of a pattern.*
* `seed=<n>` *- Varies camo and scatter without changing anything else about them.*
* `reverse=on|off` *- Flips which end of the axis a gradient starts at.*

`/bc RuleAction "My Hull" "Armor" stops=#1b2838;#c9dfe6 axis=up steps=10` paints a ten band gradient from a dark keel to a light deck.

**Conditions**:

Conditions live in groups, and a group combines its members with `and` or `or` and can be negated, which is what makes expressions such as `(heavy armor and red) or damaged` possible. Every condition and group is addressed by a path that `showjob` prints: `0` is the root group, `1` its first member, `2.1` the first member of its second member. Members are numbered conditions first, nested groups after them.

* **addcondition** [*job*] [*rule*] [*group path*] [*field=value ...*] *- Adds a condition, by default to the root group.*
* **setcondition** [*job*] [*rule*] [*path*] [*field=value ...*] *- Changes a condition.*
* **removecondition** [*job*] [*rule*] [*path*] *- Removes a condition or a group with everything in it.*
* **movecondition** [*job*] [*rule*] [*path*] [*group path*] [*position*] *- Moves a condition or group into another group.*
* **addgroup** [*job*] [*rule*] [*parent path*] [*and\|or*] [*not*] *- Adds a nested group.*
* **setgroup** [*job*] [*rule*] [*path*] [*and\|or*] [*not=on\|off*] *- Changes how a group combines its members.*
* **skins** [*filter*] *- Lists the armor skin ids that can be used.*

Condition fields, where the first field you name also picks the type of a new condition:

| Field | Value |
| --- | --- |
| `type` | `color`, `def`, `skin`, `category`, `gridsize`, `integrity`, `any` |
| `is` | `is` or `isnot` |
| `color` | `R,G,B` or `#RRGGBB` |
| `def` | `TypeId/SubtypeId`, or just a subtype. `*` and `?` work as wildcards |
| `deftype`, `subtype` | the two halves of `def` on their own |
| `skin` | skin id from **skins**, or `none` for unskinned |
| `category` | `armor`, `light`, `heavy`, `functional` |
| `gridsize` | `large` or `small` |
| `integrity` | `intact`, `damaged`, `incomplete`, `below` |
| `threshold` | percentage used by `integrity=below` |

An example, a job that paints heavy armor dark and everything damaged red:

```
/bc newjob "Hull"
/bc addcondition "Hull" "#1" category=heavy
/bc ruleaction "Hull" "#1" color=40,40,45
/bc addrule "Hull" "Damage"
/bc addcondition "Hull" "Damage" integrity=damaged
/bc ruleaction "Hull" "Damage" color=#c81e1e
/bc showjob "Hull"
/bc applyjob "Hull"
```

## 🛠︰Configs

This mod can create two config files.

[OLD can be removed] ~~For servers a settings.xml file is created in your world storage.~~

Color sets are saved in ColorSets.xml in your global storage: `"%appdata%\SpaceEngineers\Storage\ColorSets.xml"`

## 🛠︰Support

It would be nice if you could consider supporting me 

[![Ko-fi](https://steamuserimages-a.akamaihd.net/ugc/2287333413738438809/074D2B10C793252F866EEB91EC748E0E8B3C3210/?imw=64&imh=64&ima=fit&impolicy=Letterbox&letterbox=false)](https://ko-fi.com/sisksjet) [![Patreon](https://steamuserimages-a.akamaihd.net/ugc/2287333413738613768/8FE59EC78463E3EFA52D59347D83D3C9838BF6E6/?imw=64&imh=64&ima=fit&impolicy=Letterbox&letterbox=false)](https://patreon.com/sisk) [![PayPal](https://steamuserimages-a.akamaihd.net/ugc/2287333413738619680/36B89C41163487AD5BFB13B2C673E0F153171D29/?imw=64&imh=64&ima=fit&impolicy=Letterbox&letterbox=true)](https://paypal.me/sisksjet)

or join my [Discord](https://discord.gg/2s22YCqSFg) if you have suggestions, wishes, or just want to know what else I'm working on. My Discord is new, so there is not much going on yet.
You can also check out my other mods in my [Workshop](https://steamcommunity.com/id/sisksjet/myworkshopfiles/?appid=244850).


## 🛠︰Credits

* Thanks [Dark Helmet](https://steamcommunity.com/id/zigglegarf) for the awesome UI library: [Rich HUD Master](https://steamcommunity.com/workshop/filedetails/?id=1965654081)
