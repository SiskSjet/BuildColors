# Build Colors
![Build Colors](./Mod/thumb.png)

- [Build Colors](#build-colors)
  - [🛠︰Info](#info)
  - [🛠︰Installation](#installation)
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

## 🛠︰Color Sets

To save or load a color set you have to type the listed commands below in your chat box.

`Usage: /bc [command] [arguments]`

**Available commands**:
* **save** [*name*] *- Saves a Color Set with the given name.*
* **load** [*name*] *- Loads a Color Set with the given name.*
* **remove** [*name*] *- Removes a Color Set with given name.*
* **generate** [*name*] *- generates a random color set. It is not saved, only the color palette is changed*
* **list** *- Lists all available color sets.*
* **help** *- Shows a help window with all commands.*

## 🛠︰Paint Jobs

A paint job is a list of rules that is applied to the grid you are looking at. Each rule tests a block against a tree of conditions and paints it with a color, a skin, or both. The first matching rule wins.

What a rule paints with is called its paint source. A source is either a single color and skin, or one of four that work the color out per block from where that block sits on the grid: a **gradient** between stops along an axis, **camo** in patches, **scatter** picking per block by weight, or a **pattern** of stripes or checkers. A rule uses one or the other, never both. Every color a source picks from carries an optional skin, so a rust patch can bring the rusty skin along with the brown. Sources are deterministic: the same job on the same build always gives the same result, so re-applying it changes nothing.

Gradients run along the longest axis of the build by default and stretch across the blocks the rule actually paints, so both end colors always show up even when the rule only covers part of a grid. A build that is mostly one layer thick along the chosen axis is the exception: no gradient along that axis can spread it out, so pick another one.

Every application is recorded, and **undojob** puts one back. The history holds the last ten applications and lives only for the session.

### Hotkeys

One job is the **active** one — whichever you last picked in the paint job panel. These keys act on it with no menu open, and are rebindable in the Rich HUD terminal under Build Colors ▸ Controls:

| Default | Does |
|---|---|
| `Alt` + `P` | Applies the active paint job to the grid you are looking at |
| `Alt` + `Z` | Puts the last application back |
| `Alt` + `.` | Next paint job — a notification names the one you land on |
| `Alt` + `,` | Previous paint job |
| `Alt` + `B` | Opens the workbench |

Two-key combos on purpose: a paint job repaints a whole grid, which is not something a stray keypress should start. They do nothing while the chat is open or while a text field has focus.

Paint jobs are edited in the **workbench**, opened with `Alt`+`B` or from the paint job panel of the color picker screen. It shows the jobs, the rules of the selected job and the detail of the selected rule side by side, and everything it can do is also available in chat.

```
Jobs           Rules - "Hull"          Rule - "Heavy armor"
 Hull          1. Heavy armor          [Conditions] [Paint]
 Interior      2. Damaged               [0] all of
 Camo test     3. Everything else         [1] category is heavy
                                          [2] size is large
```

Edits are written straight back, so there is nothing to save. The window is dragged by its title bar and closed with the button in its corner, and it keeps clear of the game's own toolbar. It stands on its own and does not need the color picker open.

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
