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
