# [3.0.2](https://github.com/SiskSjet/BuildColors/compare/v3.0.1...3.0.2) (2026-08-12)


### Bug Fixes

* panel ran under the vanilla color controls and lost most of its height below 1080p, because the interface was laid out in raw pixels where the framework stops scaling
* gap to the screen edges and the vanilla controls was uneven, and the reserve at the bottom took height nothing used
* rule detail was squeezed to a fraction of its width at 4:3, 5:4 and 16:10, where the job and rule lists now share a row above it instead
* swatches were drawn taller than they were wide in narrow grids



# [3.0.1](https://github.com/SiskSjet/BuildColors/compare/v3.0.0...3.0.1) (2026-08-10)


### Bug Fixes

* build color palette stayed grey and unclickable in the paint job editor, because the swatches were only read once while the local player did not exist yet
* clicks passed through an open dialog to the panel behind it, so a click beside the conditions editor could hit Copy or New and duplicate the selected job
* solid color picked with the HSV sliders was not written to the rule unless another control was touched afterwards
* gradient stops and pattern palette overwrote each other, resetting scatter weights on every reopen
* scatter weights were matched to the wrong palette entries when the palette held an empty entry



# [3.0.0](https://github.com/SiskSjet/BuildColors/compare/v2.1.7...v3.0.0) (2026-08-09)


### Features

* add paint rule functionality ([aa6ad96](https://github.com/SiskSjet/BuildColors/commit/aa6ad96))
* consolidate UI into a single navigable panel ([9e1027f](https://github.com/SiskSjet/BuildColors/commit/9e1027f))
* implement multiplayer sharing for color sets and paint jobs ([3e6db6e](https://github.com/SiskSjet/BuildColors/commit/3e6db6e))


### Bug Fixes

* block game input when typing in UI text fields ([12cd7ac](https://github.com/SiskSjet/BuildColors/commit/12cd7ac))
* prevent mod hotkeys from triggering game controls ([e5532e7](https://github.com/SiskSjet/BuildColors/commit/e5532e7))


### Code Refactoring

* refactor BuildColors and migrate to updated RichHudFramework API ([6e55225](https://github.com/SiskSjet/BuildColors/commit/6e55225))
* remove 'Respect Ownership' option from paint jobs ([2aecbbb](https://github.com/SiskSjet/BuildColors/commit/2aecbbb))


### Build System

* migrate project to MDK ([36f05df](https://github.com/SiskSjet/BuildColors/commit/36f05df))


### BREAKING CHANGES

* the 'Respect Ownership' paint job option was removed, along with its UI control and
command-line switch. The ownership check is now mandatory: grids you may not modify are
always skipped, except in creative.



# [2.1.7](https://github.com/SiskSjet/BuildColors/compare/v2.1.6...v2.1.7) (2024-10-01)


### Features

* improve colorpicker handling ([11439df](https://github.com/SiskSjet/BuildColors/commit/11439df828193e5116308af3725e16b55dc2618a))



# [2.1.6](https://github.com/SiskSjet/BuildColors/compare/v2.1.5...v2.1.6) (2024-07-26)


### Bug Fixes

* fixed a bug that caused the mod to not unload properly if the player lost connection due to connection problems ([23ea6b8](https://github.com/SiskSjet/BuildColors/commit/23ea6b8d0d6c6c962678b75c6b7ccf7aab5add36))



# [2.1.5](https://github.com/SiskSjet/BuildColors/compare/v2.1.4...v2.1.5) (2024-03-30)


### Bug Fixes

* fix `Invalid Expression Term ]` I introducedwith last hotfix... ([8f8b7a6](https://github.com/SiskSjet/BuildColors/commit/8f8b7a68517827bfd7a0158027326ce9e99f3776))



# [2.1.4](https://github.com/SiskSjet/BuildColors/compare/v2.1.3...v2.1.4) (2024-03-30)


### Bug Fixes

* fix a crash that can happen when loading to a server which is restarting ([5b2cd63](https://github.com/SiskSjet/BuildColors/commit/5b2cd63a2677322fd7b91223beae2971fc27b493))



# [2.1.3](https://github.com/SiskSjet/BuildColors/compare/v2.1.2...v2.1.3) (2024-03-17)


### Bug Fixes

* `ServerId` is not persistant, so the mod had never restored the colors. It will not use the world name instead ([32c2f69](https://github.com/SiskSjet/BuildColors/commit/32c2f69f64ae17951a62af2ac6c8eba4e8c55578))



# [2.1.2](https://github.com/SiskSjet/BuildColors/compare/v2.1.1...v2.1.2) (2024-03-16)


### Bug Fixes

* fixed a crash on dedicated ([58aac2b](https://github.com/SiskSjet/BuildColors/commit/58aac2bab63224407d00a0f9f3a756a3574a1173))



# [2.1.1](https://github.com/SiskSjet/BuildColors/compare/v2.1.0...v2.1.1) (2024-03-15)


### Features

* add the function back to restore colors on server, but now it's local ([12cf9af](https://github.com/SiskSjet/BuildColors/commit/12cf9af9321ab87ec22d5bf6b923b141b6fc816f))



# [2.0.2](https://github.com/SiskSjet/BuildColors/compare/v2.0.1...v2.0.2) (2023-05-12)


### Bug Fixes

* fix scaling issue with lower resolutions and different aspect ratios ([de2cbd4](https://github.com/SiskSjet/BuildColors/commit/de2cbd4ed77b11d493488e862ef983e05e37cd45))



# [2.0.1](https://github.com/SiskSjet/BuildColors/compare/v2.0.0...v2.0.1) (2023-05-05)


### Features

* add functionality to load a color set by double clicking it & some ui sounds ([91af1f8](https://github.com/SiskSjet/BuildColors/commit/91af1f8f1189cba13e2d72645a289b233efe0b53)), closes [#7](https://github.com/SiskSjet/BuildColors/issues/7)



# [2.0.0](https://github.com/SiskSjet/BuildColors/compare/v1.1.1...v2.0.0) (2023-04-13)


### Bug Fixes

* fix an issue which prevented a generated color set to save to file ([812eaa2](https://github.com/SiskSjet/BuildColors/commit/812eaa2d46661d0d4571ad953f33808ac78498a3))


### Code Refactoring

* remove network code ([359c660](https://github.com/SiskSjet/BuildColors/commit/359c66018ab43aa2bc820fbbff9a7b3d286b7998))


### Features

* add color set generator to ui ([475b189](https://github.com/SiskSjet/BuildColors/commit/475b189f8d543a5100fcd11834fb23c6412a65e4))
* add save load and delete options to ui ([31a07ec](https://github.com/SiskSjet/BuildColors/commit/31a07ecda71a24b7fe1f861b89bc87db5754fdc7))
* add simple console command to generate a color set ([667ec0a](https://github.com/SiskSjet/BuildColors/commit/667ec0a05ee45c8cb18f08c6b06d54cb71d008d1))


### BREAKING CHANGES

* player color will no longer get synchronized with the server



# [1.1.2](https://github.com/SiskSjet/BuildColors/compare/v1.1.1...v1.1.2) (2019-02-25)


### Bug Fixes

* fix a crash when enter a message shorter than command prefix ([13cf082](https://github.com/SiskSjet/BuildColors/commit/13cf082))



# [1.1.1](https://github.com/SiskSjet/BuildColors/compare/v1.1.0...v1.1.1) (2019-01-30)

This is just a maintance update. No new functions or fixes are added.

* updated mod utils
* some code rearrangement



<a name="1.1.0"></a>
# [1.1.0](https://github.com/SiskSjet/BuildColors/compare/v1.0.0...v1.1.0) (2018-08-12)


### Features

* add a command to remove a color set ([18ee83c](https://github.com/SiskSjet/BuildColors/commit/18ee83c))
* add german translation ([f5c135e](https://github.com/SiskSjet/BuildColors/commit/f5c135e))



<a name="1.0.0"></a>
# 1.0.0 (2018-08-11)


### Features

* add ability for client to request saved color at World start ([5033afe](https://github.com/SiskSjet/BuildColors/commit/5033afe))
* add the ability to save and load build color sets ([720cb09](https://github.com/SiskSjet/BuildColors/commit/720cb09))
* save player colors on world save ([995b431](https://github.com/SiskSjet/BuildColors/commit/995b431))
